using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rancher.Community.Slack.Models;
using Rancher.Community.Slack.Utilities;

namespace Rancher.Community.Slack.SlackEventRoutingMiddleware
{
    public class SlackEventVerificationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly SlackEventVerificationMiddlewareOptions _options;

        public SlackEventVerificationMiddleware(RequestDelegate next,
            IOptions<SlackEventVerificationMiddlewareOptions> options)
        {
            _options = options.Value;
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var logger = _options.Logger;
            var response = httpContext.Response;

            try
            {
                if (IsValidationToken(httpContext.Request))
                {
                    string errorText;
                    logger.Log(LogLevel.Information, "Received verification challenge. Verifying");
                    var challenge = await httpContext.ReadFromJson<VerificationChallenge>();

                    if (Environment.GetEnvironmentVariable("validation_token", EnvironmentVariableTarget.Process) ==
                        null)
                    {
                        errorText = "Environment variable validation_token not found.";
                        await Logging.LogAndReturnError(logger, response, errorText);
                    }

                    if (!System.Environment
                        .GetEnvironmentVariable("validation_token", EnvironmentVariableTarget.Process)
                        .Equals(challenge.token))
                    {
                        errorText = "Expected validation token does not match challenge token.";
                        await Logging.LogAndReturnError(logger, response, errorText);
                    }

                    logger.LogInformation("Challenge accepted!  Sending response.");
                    await response.WriteJson(new {challenge = challenge.challenge});
                }
                else
                {
                    await _next.Invoke(httpContext);
                }
            }
            catch (Exception ex)
            {
                var errorText = "An error occured during slack verification handshake.";
                logger.LogError(ex, errorText);
                await Logging.LogAndReturnError(logger, response, errorText);
            }
        }

        private bool IsValidationToken(HttpRequest request)
        {
            var checkString = request.ReadAsString();
            return checkString.Contains("url_verification");
        }
    }
}