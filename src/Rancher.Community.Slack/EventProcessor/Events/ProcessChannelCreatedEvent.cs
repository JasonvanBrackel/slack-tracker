using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rancher.Community.Slack.Data;
using Rancher.Community.Slack.Data.Models;
using Rancher.Community.Slack.Models;
using Rancher.Community.Slack.Utilities;

namespace Rancher.Community.Slack.EventProcessor
{
        public static partial class Events
        {
            public static async Task ProcessChannelCreatedEvent(HttpContext context, ILogger logger)
            {
                try
                {
                    using (var scope = context.RequestServices.CreateScope())
                    using (var dbContext =
                        scope.ServiceProvider.GetRequiredService<SlackTrackerContext>())
                    {
                        logger.LogInformation("Received channel created event.");
                        var channelCreatedEvent =
                            await context.ReadFromJson<SlackEventCallback<ChannelCreatedEvent>>();
                        logger.LogInformation(
                            $"Id: {channelCreatedEvent.@event.channel.id}, Channel {channelCreatedEvent.@event.channel.name}");
                        logger.LogDebug("Channel Created Event:", channelCreatedEvent);
    
                        dbContext.Channels.Add(new Channels()
                        {
                            Id = channelCreatedEvent.@event.channel.id, Name = channelCreatedEvent.@event.channel.name
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