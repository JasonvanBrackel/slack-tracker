namespace Rancher.Community.Slack.Models
{
    public class MessageEvent : AbstractSlackEvent
    {
        public string channel { get; set; }
        public string text { get; set; }
        public string ts { get; set; }
        public string channel_type { get; set; }
    }
}