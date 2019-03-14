using System;

namespace Rancher.Community.Slack.Models
{
    public class VerificationChallenge
    {
        public string token { get; set; }
        public string challenge { get; set; }
        public string type { get; set; }
    }
}