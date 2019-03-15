using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rancher.Community.Slack.Data;
using Rancher.Community.Slack.SlackApi;

namespace Rancher.Community.Slack.EventProcessor
{
    public static class SlackEventRouter
    {
        static string _generalId;
        public static async Task ProcessSlackEvents(HttpContext context, ILogger logger, string slackServer, string authorizationToken)
        {
            var slackClient = new ApiClient(slackServer, authorizationToken);
            var type = FindEventType(context.Request);
            logger.LogInformation($"Received event type {type}.");

            var general = GetGeneralId(context, logger);
            
            switch (type)
            {
                case "message":
                    await Events.ProcessMessageEvent(context, logger);
                    break;
                case "member_joined_channel":
                    await Events.ProcessMemberJoinedChannelEvent(context, logger, general, slackClient);
                    break;
                case "channel_created":
                    await Events.ProcessChannelCreatedEvent(context, logger);
                    break;
                default:
                    logger.LogInformation($"Unknown event type {type}. Ignoring.");
                    break;
            }
        }

        private static string GetGeneralId(HttpContext context, ILogger logger)
        {
            while (_generalId == null)
            {
                using (var scope = context.RequestServices.CreateScope())
                using (var dbContext =
                    scope.ServiceProvider.GetRequiredService<SlackTrackerContext>())
                {
                    logger.LogInformation("Checking db for General channel.");
                    if (dbContext.Channels.Count(c => c.Name.Equals("general")).Equals(0))
                    {
                        logger.LogInformation("General channel not yet loaded into the database.  Waiting 10 seconds.");
                        Thread.Sleep(new TimeSpan(0, 0, 10));
                    }
                    else
                    {
                        _generalId = dbContext.Channels.Single(c => c.Name.Equals("general")).Id;
                    }
                }
            }

            return _generalId;
        }

        private static object FindEventType(HttpRequest request)
        {
            using (StreamReader reader = new StreamReader(request.Body, Encoding.UTF8, true, 200, true))
            {
                var buffer = new char[200];
                reader.Read(buffer);
                var paritalBody = new string(buffer);
                request.Body.Position = 0L;
                
                const string typeSearch = "type\":\"";
                const string quoteSearch = "\"";
                var startSearchIndex = paritalBody.IndexOf(typeSearch) + typeSearch.Length;
                var endSearchIndex = paritalBody.IndexOf(quoteSearch, startSearchIndex);
                var typeLength = endSearchIndex - startSearchIndex;
                var type = paritalBody.Substring(startSearchIndex, typeLength);
                return type;
            }
        }
    }
}
