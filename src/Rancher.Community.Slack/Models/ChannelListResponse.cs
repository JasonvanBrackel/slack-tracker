using System.Collections.Generic;

namespace Rancher.Community.Slack.Models
{
    public class ChannelListResponse
    {
        public string ok { get; set; }
        public List<ChannelReponse> channels { get; set; }
    }
}