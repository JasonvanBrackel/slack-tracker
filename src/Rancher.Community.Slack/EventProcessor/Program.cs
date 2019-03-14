using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rancher.Community.Slack.BufferingMiddleware;
using Rancher.Community.Slack.Data;
using Rancher.Community.Slack.Data.Models;
using Rancher.Community.Slack.Models;
using Rancher.Community.Slack.SlackApi;
using Rancher.Community.Slack.SlackEventRoutingMiddleware;

namespace Rancher.Community.Slack.EventProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"Starting up at {DateTime.Now}.");
            var connectionString = System.Environment.GetEnvironmentVariable("db_connection", EnvironmentVariableTarget.Process);
            var authorizationToken = System.Environment.GetEnvironmentVariable("authorization_token", EnvironmentVariableTarget.Process);
            var slackServer = Environment.GetEnvironmentVariable("slack_url", EnvironmentVariableTarget.Process);
            var welcomeMessage = Environment.GetEnvironmentVariable("welcome_message", EnvironmentVariableTarget.Process);
            
            
            
            var host = new WebHostBuilder()
                .UseKestrel(options => options.ListenAnyIP(8080))
                .ConfigureServices(configureServices =>
                {
                    configureServices.AddDbContext<SlackTrackerContext>(opt => opt.UseSqlServer(connectionString));

                })
                .ConfigureLogging(logging => { logging.AddConsole(); })
                .Configure(app =>
                {
                    var logger = app.ApplicationServices.GetRequiredService<ILogger<Program>>();
                    StartWelcomeNewUsersThread(slackServer, authorizationToken, welcomeMessage, logger);
                    StartUsersAndChannelsThread(slackServer, authorizationToken, logger);

                    app
                        .UseBufferingMiddleware()
                        .UseSlackEventVerificationMiddleware(new SlackEventVerificationMiddlewareOptions(){Logger = logger})
                        .Run(
                            async context => { await SlackEventRouter.ProcessSlackEvents(context, logger, slackServer, authorizationToken); }
                        );
                })
                .Build();

            host.Run();
        }

        private static void StartUsersAndChannelsThread(string slackServer, string authorizationToken, ILogger logger)
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    PopulateUsersAndChannels(slackServer, authorizationToken);
                    logger.LogInformation("Done scrapping user and channel data.  Sleeping for an hour");
                    Thread.Sleep(new TimeSpan(1,0,0));
                }


            }, TaskCreationOptions.LongRunning);
        }


        private static void StartWelcomeNewUsersThread(string slackUrl, string authorizationToken, string welcomeMessage, ILogger logger)
        {
            logger.LogInformation("Starting welcome message loop.");
            var client = new ApiClient(slackUrl, authorizationToken);
            Task.Factory.StartNew(() => { WelcomeNewUsers(welcomeMessage, logger, client); }, TaskCreationOptions.LongRunning);
        }

        private static void WelcomeNewUsers(string welcomeMessage, ILogger logger, ApiClient client)
        {
            while (true)
            {
                var now = DateTime.UtcNow;
                if (DateTime.Now.IsDaylightSavingTime())
                {
                    now = now.AddHours(-4);
                }
                else
                {
                    now = now.AddHours(-5);
                }

                if (now.Hour >= 9 &&
                    now.Hour < 17 &&
                    now.DayOfWeek >= DayOfWeek.Monday &&
                    now.DayOfWeek <= DayOfWeek.Friday)
                {
                    logger.LogInformation("Welcoming new users");
                    using (var context = new SlackTrackerContext())
                    {
                        var usersToWelcome =
                            context.UserJoinedGeneralEvents.Where(joins => joins.HasBeenWelcomed.Equals(false));

                        foreach (var user in usersToWelcome)
                        {
                            logger.LogInformation($"Welcoming {user.User}.");
                            client.PostMessage(user.User, welcomeMessage);
                        }

                        context.Database.ExecuteSqlCommand(
                            "UPDATE dbo.UserJoinedGeneralEvents SET HasBeenWelcomed = 1 FROM dbo.UserJoinedGeneralEvents");
                    }
                }
                else
                {
                    logger.LogInformation("Waiting until 9am ET to welcome users.");
                }

                logger.LogInformation("Done welcoming new users, sleeping for 5 minutes.");
                Thread.Sleep(new TimeSpan(0, 5, 0));
            }
        }

        private static void PopulateUsersAndChannels(string slackServer, string authoriztionToken)
        {
            Console.WriteLine("Populating Slack Users and Channels.");
            Console.WriteLine("Checking Channels");
            using (var dbContext = new SlackTrackerContext())
            {
                var client = new ApiClient(slackServer, authoriztionToken);
                foreach (var channel in client.GetChannels().channels)
                {
                    Console.WriteLine($"Looking for Channel {channel.name}.");
                    if (dbContext.Channels.Count(c => c.Id.Equals(channel.id)).Equals(0))
                    {
                        Console.WriteLine($"Adding Channel {channel.name}.");
                        dbContext.Channels.Add(new Channels() {Id = channel.id, Name = channel.name});
                        dbContext.SaveChanges();

                    }
                }

                Console.WriteLine("Checking Users");
                
                string cursor = null;
                MembersListResponse memberResponse;
                do
                {
                    Console.WriteLine("Cursor: " + cursor);
                    memberResponse = client.GetUsers(cursor);
                    var memberResponses = memberResponse.members;

                    if (memberResponses != null)
                        foreach (var member in memberResponses)
                        {
                            Console.WriteLine($"Looking for Member {member.real_name}.");
                            if (dbContext.Users.Count(c => c.Id.Equals(member.id)).Equals(0))
                            {
                                Console.WriteLine($"Adding User {member.name}, {member.real_name}.");
                                dbContext.Users.Add(new Users()
                                    {Id = member.id, Username = member.name, Name = member.real_name});
                                dbContext.SaveChanges();
                            }
                        }
                    
                    if(memberResponse.response_metadata != null &&
                        !string.IsNullOrWhiteSpace(memberResponse.response_metadata.next_cursor))
                    {
                        cursor = memberResponse.response_metadata.next_cursor;
                    }
                    
                } while (memberResponse.response_metadata != null &&
                         !string.IsNullOrWhiteSpace(memberResponse.response_metadata.next_cursor)
                         );

                
                CultureInfo cultureInfo = new CultureInfo("en-US");
                Calendar calendar = cultureInfo.Calendar;
                
                Console.WriteLine("Populating Historical Data");
                foreach (var channel in dbContext.Channels)
                {
                    Console.WriteLine($"Checking Channel {channel.Name }");
                    var oldest = DateTime.Parse("1/1/2019").ToUniversalTime();
                    var latest = DateTime.UtcNow;
                    

                    for (int i = oldest.Month; i <= latest.Month; i++)
                    {
                        Console.WriteLine($"Checking Channel {channel.Name}, Month {i}.");
                        DateTime firstDay;
                        DateTime lastDay;
                        if (oldest.Month == i)
                        {
                            firstDay = oldest;
                        }
                        else
                        {
                            firstDay = new DateTime(oldest.Year, i, 1);
                        }

                        if (latest.Month == i)
                        {
                            lastDay = latest;
                        }
                        else
                        {
                            lastDay =  new DateTime(oldest.Year, i + 1, 1).AddDays(-1);
                        }

                        Console.WriteLine($"Checking Channel {channel.Name}, Between { firstDay } and { lastDay }.");

                        cursor = null;

                        ConversationHistoryResponse messages;
                        do
                        {
                            messages = client.GetMessageHistory(channel.Id, firstDay, lastDay, cursor);

                            if (messages.messages != null)
                            {
                                foreach (var message in messages.messages.Where(m =>
                                    !string.IsNullOrWhiteSpace(m.client_msg_id)))
                                {
                                    if (dbContext.MessageHistory.Count(m => m.Id.Equals(message.client_msg_id))
                                        .Equals(0))
                                    {
                                        Console.WriteLine(
                                            $"Inserting message {message.client_msg_id} into Channel {channel.Name}.");
                                        dbContext.MessageHistory.Add(new MessageHistory()
                                        {
                                            Id = message.client_msg_id,
                                            User = message.user,
                                            Timestamp = message.ts.FromSlackTimestamp(), Channel = channel.Id,
                                            Text = message.text
                                        });

                                        dbContext.SaveChanges();
                                    }
                                }
                            }

                            if (messages.has_more)
                            {
                                cursor = messages.response_metadata.next_cursor;
                            }


                        } while (messages.has_more);
                    }
                }
            }
        }
    }
}