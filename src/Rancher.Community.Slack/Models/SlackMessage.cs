using System;

namespace Rancher.Community.Slack.Models
{
    public class SlackMessage
    {
        public string client_msg_id  {get; set; }
        
        public string type { get; set; }
        public string user { get; set; }
        public string text { get; set; }
        public string ts { get; set; }
    }
}