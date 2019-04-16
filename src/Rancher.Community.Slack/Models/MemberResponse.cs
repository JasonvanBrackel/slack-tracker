namespace Rancher.Community.Slack.Models
{
    public class MemberResponse
    {
        public string id { get; set; }
        public string name { get; set; }
        public string real_name { get; set; }
        
        public string tz { get; set; }
        
        public string tz_label { get; set; }

        public ProfileResponse profile { get; set; }
    }
}