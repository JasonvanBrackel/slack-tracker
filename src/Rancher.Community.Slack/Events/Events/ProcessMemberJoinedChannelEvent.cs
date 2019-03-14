using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rancher.Community.Slack.Data;
using Rancher.Community.Slack.Data.Models;
using Rancher.Community.Slack.Models;
using Rancher.Community.Slack.SlackApi;
using Rancher.Community.Slack.Utilities;

namespace Rancher.Community.Slack
{
    public static partial class Events
    {
        public static async Task ProcessMemberJoinedChannelEvent(HttpContext context, ILogger logger, string generalId, ApiClient slackClient)
        {
            try
            {
                using (var scope = context.RequestServices.CreateScope())
                using (var dbContext =
                    scope.ServiceProvider.GetRequiredService<SlackTrackerContext>())
                {
                    logger.LogInformation("Received new member event.");
                    var memberJoinedChannelEvent =
                        await context.ReadFromJson<SlackEventCallback<MemberJoinedChannelEvent>>();
                    logger.LogInformation(
                        $"Member: {memberJoinedChannelEvent.@event.user}, Channel {memberJoinedChannelEvent.@event.channel}");
                    logger.LogDebug("New Member Events:", memberJoinedChannelEvent);

                    if (memberJoinedChannelEvent.@event.channel.Equals(generalId))
                    {
                        logger.LogInformation("Adding user to database");
                        var newUser = slackClient.GetUser(memberJoinedChannelEvent.@event.user);
                        logger.LogInformation("User: ", newUser.user);
                        dbContext.Users.Add(new Users() { Id = newUser.user.id, Username = newUser.user.name, Name = newUser.user.real_name});
                        dbContext.SaveChanges();
                        
                        dbContext.UserJoinedGeneralEvents.Add(new UserJoinedGeneralEvents()
                        {
                            User = memberJoinedChannelEvent.@event.user,
                            Timestamp = memberJoinedChannelEvent.@event.event_ts.FromSlackTimestamp()
                        });
                        dbContext.SaveChanges();                        
                    }
                }
            }
            catch (Exception e)
            {
                var errorText = "An error occurred during member event processing.";
                logger.LogError(e, errorText);
                await Logging.LogAndReturnError(logger, context.Response, errorText);
            }
        }
    }
}