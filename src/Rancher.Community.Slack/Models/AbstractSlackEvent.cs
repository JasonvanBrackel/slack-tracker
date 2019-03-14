namespace Rancher.Community.Slack.Models
{
    public abstract class AbstractSlackEvent
    {
        public string type { get; set; }
        public string event_ts { get; set; }
        public string user { get; set; }
    }
}