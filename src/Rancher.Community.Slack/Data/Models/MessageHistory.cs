﻿using System;

namespace Rancher.Community.Slack.Data.Models
{
    public partial class MessageHistory
    {
        public string Id { get; set; }
        public string Channel { get; set; }
        public string User { get; set; }
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
