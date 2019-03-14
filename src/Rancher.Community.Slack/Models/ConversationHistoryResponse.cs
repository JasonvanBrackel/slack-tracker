using System.Collections.Generic;

namespace Rancher.Community.Slack.Models
{
    public class ConversationHistoryResponse
    {
        public List<SlackMessage> messages { get; set;  }
        public bool ok { get; set; }
        public int pin_count { get; set; }
        public bool has_more { get; set; }
        public ResponseMetadata response_metadata { get; set; }
    }
}