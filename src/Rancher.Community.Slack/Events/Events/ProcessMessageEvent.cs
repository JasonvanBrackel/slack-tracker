using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rancher.Community.Slack.Data;
using Rancher.Community.Slack.Data.Models;
using Rancher.Community.Slack.Models;
using Rancher.Community.Slack.Utilities;

namespace Rancher.Community.Slack
{
    public static partial class Events
    {
        public static async Task ProcessMessageEvent(HttpContext context, ILogger logger)
        {
            try
            {
                using (var scope = context.RequestServices.CreateScope())
                using (var dbContext =
                    scope.ServiceProvider.GetRequiredService<SlackTrackerContext>())
                {
                    logger.LogInformation("Received message event.");
                    var messageEvent =
                        await context.ReadFromJson<SlackEventCallback<MessageEvent>>();
                    logger.LogInformation(
                        $"Member: {messageEvent.@event.user}, Channel {messageEvent.@event.channel}, Message {messageEvent.@event.text}");
                    logger.LogDebug("Message Event:", messageEvent);

                    dbContext.Messages.Add(new Messages()
                    {
                        EventId = messageEvent.event_id, 
                        User = messageEvent.@event.user, 
                        Channel = messageEvent.@event.channel, Text = messageEvent.@event.text, Timestamp = messageEvent.@event.ts.FromSlackTimestamp()
                    });

                    dbContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                var errorText = "An error occurred during message event processing.";
                logger.LogError(e, errorText);
                await Logging.LogAndReturnError(logger, context.Response, errorText);
            }
        }
    }
}