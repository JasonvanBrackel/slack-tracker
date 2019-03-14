using System.Collections.Generic;

namespace Rancher.Community.Slack.Models
{
    public class UserInfoResponse
    {
        public string ok { get; set; }
        public UserResponse user { get; set; }
    }
}