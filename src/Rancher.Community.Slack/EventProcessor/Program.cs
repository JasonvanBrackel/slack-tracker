using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rancher.Community.Slack.BufferingMiddleware;
using Rancher.Community.Slack.Data;
using Rancher.Community.Slack.SlackEventRoutingMiddleware;

namespace Rancher.Community.Slack.EventProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine($"Starting up Slack event processor at {DateTime.Now}.");
            Console.WriteLine("Gathering environment variables.");
            var connectionString = Environment.GetEnvironmentVariable("db_connection", EnvironmentVariableTarget.Process);
            var authorizationToken = Environment.GetEnvironmentVariable("authorization_token", EnvironmentVariableTarget.Process);
            var slackServer = Environment.GetEnvironmentVariable("slack_url", EnvironmentVariableTarget.Process);

            var host = new WebHostBuilder()
                .UseKestrel(options => options.ListenAnyIP(8080))
                .ConfigureServices(configureServices =>
                {
                    configureServices.AddDbContext<SlackTrackerContext>(opt => opt.UseSqlServer(connectionString));
                })
                .ConfigureLogging(logging => { logging.AddConsole(); })
                .Configure(app =>
                {
                    var logger = app.ApplicationServices.GetRequiredService<ILogger<Program>>();

                    app
                        .UseBufferingMiddleware()
                        .UseSlackEventVerificationMiddleware(new SlackEventVerificationMiddlewareOptions()
                            {Logger = logger})
                        .Run(
                            async context =>
                            {
                                await SlackEventRouter.ProcessSlackEvents(context, logger, slackServer, authorizationToken);
                            }
                        );
                })
                .Build();

            host.Run();
        }

    }
}