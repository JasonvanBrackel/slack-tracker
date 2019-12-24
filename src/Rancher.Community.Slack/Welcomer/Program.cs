using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Prometheus;
using Rancher.Community.Slack.Data;
using Rancher.Community.Slack.SlackApi;

namespace Rancher.Community.Slack.Welcomer
{
    class Program
    {
        private static Logger _logger;
        private static string _slackUrl;
        private static string _authToken;
        private static ApiClient _client;
        private static string _db_connection;
        private static string _welcomeExistingUserMessage;
        private static string _welcomeNewUserMessage;
        private static string _welcomerId;

        static void Main(string[] args)
        {
            ConfigureStructuredLogging();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Starting Slack Welcomer.");

            StartMetricsEndpoint();
            
            GatherEnvironmentVariables();
            
            ConfigureSlackApiClient();
            
            _logger.Info("Starting welcome users loop.");
            while (true)
            {
                _logger.Info("Waking up, preparing to welcome new users.");
                WelcomeUsers();
                _logger.Info("Done welcoming new users.  Sleeping for 15 minutes");
                Thread.Sleep(new TimeSpan(0, 15, 0));
            }
        }
        
        private static void WelcomeUsers()
        {
            try
            {
                if(_client.IsActive(_welcomerId))
                {
                    _logger.Log(LogLevel.Info, "Welcoming users users.");
                    using (var context = new SlackTrackerContext())
                    {
                        var usersToWelcome =
                            context.UserJoinedGeneralEvents.Where(joins => joins.HasBeenWelcomed.Equals(false)).Take(5);

                        foreach (var user in usersToWelcome)
                        {
                            _logger.Log(LogLevel.Info, $"Welcoming {user.User}.");
                            _client.PostMessage(user.User, _welcomeNewUserMessage);
                        }

                        context.Database.ExecuteSqlCommand(
                            "UPDATE dbo.UserJoinedGeneralEvents SET HasBeenWelcomed = 1 FROM dbo.UserJoinedGeneralEvents");

                        context.Database.ExecuteSqlCommand(
                            "UPDATE dbo.Users SET HasBeenWelcomed = e.HasBeenWelcomed FROM dbo.Users u INNER JOIN dbo.UserJoinedGeneralEvents e ON u.Id = e.[User]");

                        var existingUsersToWelcome = context.Users.Where(u => u.HasBeenWelcomed.Equals(false)).Take(5);

                        foreach (var user in existingUsersToWelcome)
                        {
                            _logger.Log(LogLevel.Info, $"Welcoming Existing user {user.Id}.");
                            _client.PostMessage(user.Id, _welcomeExistingUserMessage);
                            user.HasBeenWelcomed = true;
                            context.Update(user);
                        }
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Info, $"Waiting until welcoming user {_welcomerId} is active to welcome users.");
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "An error occurred while welcoming new users.  Continuing execution.");
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
                _logger.Info("Grabbing welcome_message_new_users.");
                _welcomeNewUserMessage = Environment.GetEnvironmentVariable("welcome_message_new_users", EnvironmentVariableTarget.Process);
                _logger.Info("Grabbing welcome_message_existing_users.");
                _welcomeExistingUserMessage = Environment.GetEnvironmentVariable("welcome_message_existing_users", EnvironmentVariableTarget.Process);
                _logger.Info("Grabbing welcomer_id.");
                _welcomerId = Environment.GetEnvironmentVariable("welcomer_id", EnvironmentVariableTarget.Process);

            }

            catch (Exception e)
            {
                _logger.Error(e,
                    "There was a problem grabbing environment variables.  The variables required are 'slack_url', 'authorization_token', 'db_connection', 'welcome_message'");
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