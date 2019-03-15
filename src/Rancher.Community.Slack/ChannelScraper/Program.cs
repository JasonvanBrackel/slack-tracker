using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using Rancher.Community.Slack.Data;
using Rancher.Community.Slack.Data.Models;
using Rancher.Community.Slack.Models;
using Rancher.Community.Slack.SlackApi;

namespace Rancher.Community.Slack.ChannelScraper
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
            StartMetricsEndpoint();
            
            GatherEnvironmentVariables();
            
            ConfigureLogging();
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Starting Slack Channel Scrapper.");

            ConfigureClient();
            
            // Start Scraper Loop
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    ScrapeChannelData(_slackUrl, _authToken);
                    _logger.Log(LogLevel.Info, "Done scrapping channel data.  Sleeping for an hour");
                    Thread.Sleep(new TimeSpan(1, 0, 0));
                }
            }, TaskCreationOptions.LongRunning);
        }

        private static void ConfigureClient()
        {
            _client = new ApiClient(_slackUrl, _authToken);
        }

        private static void ScrapeChannelData(string slackServer, string authoriztionToken)
        {
            _logger.Log(LogLevel.Info, "Scrapping Channels");
            using (var dbContext = new SlackTrackerContext())
            {
                foreach (var channel in _client.GetChannels().channels)
                {
                    _logger.Log(LogLevel.Info, $"Looking for Channel {channel.name}.");
                    if (dbContext.Channels.Count(c => c.Id.Equals(channel.id)).Equals(0))
                    {
                        _logger.Log(LogLevel.Info, $"Adding Channel {channel.name}.");
                        dbContext.Channels.Add(new Channels() {Id = channel.id, Name = channel.name});
                        dbContext.SaveChanges();
                    }
                }

                string cursor;

                CultureInfo cultureInfo = new CultureInfo("en-US");
                Calendar calendar = cultureInfo.Calendar;

                _logger.Log(LogLevel.Info, "Populating Historical Message Data");
                foreach (var channel in dbContext.Channels)
                {
                    _logger.Log(LogLevel.Info, $"Checking Channel {channel.Name}");
                    var oldest = DateTime.Parse("1/1/2019").ToUniversalTime();
                    var latest = DateTime.UtcNow;


                    _logger.Log(LogLevel.Info, $"Checking Channel {channel.Name}, Between {oldest} and {latest}.");

                    cursor = null;

                    ConversationHistoryResponse messages;
                    do
                    {
                        messages = _client.GetMessageHistory(channel.Id, oldest, latest, cursor);

                        if (messages.messages != null)
                        {
                            _logger.Log(LogLevel.Info, $"Received {messages.messages.Count()} messages.");
                            if (messages.messages != null)
                            {
                                foreach (var message in messages.messages.Where(m =>
                                    !string.IsNullOrWhiteSpace(m.client_msg_id)))
                                {
                                    if (dbContext.MessageHistory.Count(m => m.Id.Equals(message.client_msg_id))
                                        .Equals(0))
                                    {
                                        _logger.Log(LogLevel.Info, 
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
                        }
                        else
                        {
                            _logger.Log(LogLevel.Info, "No Messages found.");
                        }

                        if (messages.has_more)
                        {
                            cursor = messages.response_metadata.next_cursor;
                        }
                    } while (messages.has_more);
                }
            }
        }
        
        private static void StartMetricsEndpoint()
        {
            var metricServer = new Prometheus.MetricServer(3001);
            metricServer.Start();
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
                    "There was a problem grabbing environment variables.  The variables required are 'slack_url', 'authorization_token', and 'db_connection'");
                throw;
            }
        }

        private static void ConfigureLogging()
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
    }
}