using System.Collections.Generic;

namespace Rancher.Community.Slack.Models
{
    public class SlackEventCallback<T> where T : AbstractSlackEvent
    {
        public string token { get; set; }
        public string team_id { get; set; }
        public string api_app_id { get; set; }
        public T @event { get; set; }
        public string type { get; set; }
        public List<string> authed_users { get; set; }
        public string event_id { get; set; }
        public string event_time { get; set; }
    }
}