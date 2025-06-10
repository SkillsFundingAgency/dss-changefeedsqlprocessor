using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedSqlProcessor.Service;
namespace NCS.DSS.ChangeFeedSqlProcessor
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddSingleton<ISqlDbProvider, SqlDbProvider>();
                    services.AddSingleton<IChangeFeedQueueProcessorService, ChangeFeedQueueProcessorService>();
                    services.Configure<LoggerFilterOptions>(options =>
                    {
                        LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (toRemove is not null)
                        {
                            options.Rules.Remove(toRemove);
                        }
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}

