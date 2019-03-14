using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Rancher.Community.Slack.Utilities
{
    public static class Logging
    {
        public static async Task LogAndReturnError(ILogger logger, HttpResponse response,
            string errorText)
        {
            logger.LogError(errorText);
            response.StatusCode = (int) HttpStatusCode.BadRequest;
            await response.WriteJson(new {error = errorText});
        }
    }
}