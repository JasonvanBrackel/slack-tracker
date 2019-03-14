using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Rancher.Community.Slack.SlackEventRoutingMiddleware
{
    public static class SlackEventVerificationMiddlewareExtensions
    {
        public static IApplicationBuilder UseSlackEventVerificationMiddleware(this IApplicationBuilder app,
            SlackEventVerificationMiddlewareOptions options)
        {
            return app.UseMiddleware<SlackEventVerificationMiddleware>(Options.Create(options));
        }
    }
}