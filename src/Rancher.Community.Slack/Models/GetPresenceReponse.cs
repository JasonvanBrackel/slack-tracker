namespace Rancher.Community.Slack.Models
{
    public class GetPresenceReponse
    {
        public bool ok { get; set; }
        public string presence { get; set; }
        public bool online { get; set; }
        public bool auto_away { get; set; }
        public int connection_count { get; set; }
        public string last_activity { get; set; }
    }
}