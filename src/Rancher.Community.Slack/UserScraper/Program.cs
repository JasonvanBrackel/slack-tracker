using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Prometheus;
using Rancher.Community.Slack.Data;
using Rancher.Community.Slack.Data.Models;
using Rancher.Community.Slack.Models;
using Rancher.Community.Slack.SlackApi;

namespace Rancher.Community.Slack.UserScraper
{
    class Program
    {
        private static Logger _logger;
        private static string _slackUrl;
        private static string _authToken;
        private static ApiClient _client;
        private static string _db_connection;

        static void Main(string[] args)
        {
            ConfigureStructuredLogging();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Starting User Scraper.");

            StartMetricsEndpoint();
            
            GatherEnvironmentVariables();
            
            ConfigureSlackApiClient();
            
            _logger.Info("Starting user scraper loop.");
            while (true)
            {
                _logger.Info("Waking up.  Preparing to scrape user data.");
                ScrapeUserData();
                _logger.Info("Done scrapping user data.  Sleeping for an hour");
                Thread.Sleep(new TimeSpan(1, 0, 0));
            }
            
        }

        private static void ScrapeUserData()
        {
            using (var dbContext = new SlackTrackerContext())
            {
                string cursor = null;
                MembersListResponse memberResponse;
                do
                {
                    _logger.Log(LogLevel.Info, "Cursor: " + cursor);
                    memberResponse = _client.GetUsers(cursor);
                    var memberResponses = memberResponse.members;

                    if (memberResponses != null)
                        foreach (var member in memberResponses)
                        {
                            _logger.Log(LogLevel.Info, $"Looking for Member {member.real_name}.");
                            if (dbContext.Users.Count(c => c.Id.Equals(member.id)).Equals(0))
                            {
                                _logger.Log(LogLevel.Info, $"Adding User {member.name}, {member.real_name}.");
                                dbContext.Users.Add(new Users()
                                    {Id = member.id, Username = member.name, Name = member.real_name});
                                dbContext.SaveChanges();
                            }
                        }

                    if (memberResponse.response_metadata != null &&
                        !string.IsNullOrWhiteSpace(memberResponse.response_metadata.next_cursor))
                    {
                        cursor = memberResponse.response_metadata.next_cursor;
                    }
                } while (memberResponse.response_metadata != null &&
                         !string.IsNullOrWhiteSpace(memberResponse.response_metadata.next_cursor)
                );
            }
        }
        
         private static void ConfigureStructuredLogging()
        {
            try
            {
                var config = new LoggingConfiguration();
                var consoleTarget = new ConsoleTarget
                {
                    Layout = new JsonLayout
                    {
                        IncludeAllProperties = true,
                        Attributes =
                        {
                            new JsonAttribute("time", new SimpleLayout("${longdate}")),
                            new JsonAttribute("level", new SimpleLayout("${level}")),
                            new JsonAttribute("message", new SimpleLayout("${message}")),
                        }
                    }
                };
                config.AddRuleForAllLevels(consoleTarget, "*");
                LogManager.Configuration = config;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while attempting to configure the logger.  Exiting.");
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }

        
        private static void StartMetricsEndpoint()
        {
            try
            {
                _logger.Info("Starting metrics enpoint.");
                var metricServer = new MetricServer(3001);
                metricServer.Start();
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "An error occurred while attempting to start the metrics endpoint.");
                throw;
            }
        }

        private static void GatherEnvironmentVariables()
        {
            _logger.Info("Grabbing environment variables.");
            try
            {
                _logger.Info("Grabbing slack_url.");
                _slackUrl = Environment.GetEnvironmentVariable("slack_url", EnvironmentVariableTarget.Process);
                _logger.Info("Grabbing authorization_token.");
                _authToken = Environment.GetEnvironmentVariable("authorization_token", EnvironmentVariableTarget.Process);
                _logger.Info("Grabbing db_connection.");
                _db_connection = Environment.GetEnvironmentVariable("db_connection", EnvironmentVariableTarget.Process);
            }
            catch (Exception e)
            {
                _logger.Error(e,
                    "There was a problem grabbing environment variables.  The variables required are 'slack_url', 'authorization_token', 'db_connection'");
                throw;
            }
        }
        
        private static void ConfigureSlackApiClient()
        {
            _logger.Info("Configuring Slack API client");

            try
            {
                _client = new ApiClient(_slackUrl, _authToken);
            }
            catch (Exception e)
            {
                _logger.Fatal(e, "An error occurred configuring the Slack API client.");
                throw;
            }
        }
    }
}