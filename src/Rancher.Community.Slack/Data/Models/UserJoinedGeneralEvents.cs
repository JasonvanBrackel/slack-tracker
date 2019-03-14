using System;

namespace Rancher.Community.Slack.Data.Models
{
    public partial class UserJoinedGeneralEvents
    {
        public string User { get; set; }
        public DateTime? Timestamp { get; set; }
        public bool? HasBeenWelcomed { get; set; }
    }
}
