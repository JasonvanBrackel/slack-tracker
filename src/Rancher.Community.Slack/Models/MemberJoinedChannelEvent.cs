namespace Rancher.Community.Slack.Models
{
    public class MemberJoinedChannelEvent : AbstractSlackEvent
    {
        public string channel { get; set; }
        public string channel_type { get; set; }
        public string team { get; set; }
        public string inviter { get; set; }
    }
}