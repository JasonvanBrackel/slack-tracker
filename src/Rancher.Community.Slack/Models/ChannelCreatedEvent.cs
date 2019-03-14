namespace Rancher.Community.Slack.Models
{
    public class ChannelCreatedEvent: AbstractSlackEvent
    {
        public ChannelReponse channel { get; set; }
    }
}