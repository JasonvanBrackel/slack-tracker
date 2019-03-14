using Microsoft.Extensions.Logging;

namespace Rancher.Community.Slack.SlackEventRoutingMiddleware
{
    public class SlackEventVerificationMiddlewareOptions
    {
        public ILogger Logger { get; set; }
    }
}