using System.Collections.Generic;

namespace Rancher.Community.Slack.Models
{
    public class MembersListResponse
    {
        public string ok { get; set; }
        public string error { get; set; }
        public List<MemberResponse> members { get; set; }
        public ResponseMetadata response_metadata { get; set; }
    }
}