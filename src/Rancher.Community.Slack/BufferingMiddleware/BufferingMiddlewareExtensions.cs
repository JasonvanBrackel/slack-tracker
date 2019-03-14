using Microsoft.AspNetCore.Builder;

namespace Rancher.Community.Slack.BufferingMiddleware
{
    public static class BufferingMiddlewareExtensions
    {
        public static IApplicationBuilder UseBufferingMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<BufferingMiddleware>();
        }
    }
}