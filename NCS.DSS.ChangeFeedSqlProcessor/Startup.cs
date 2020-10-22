
using DFC.AzureSql.Standard;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.ChangeFeedSqlProcessor;
using NCS.DSS.ChangeFeedSqlProcessor.Service;

[assembly: FunctionsStartup(typeof(Startup))]
namespace NCS.DSS.ChangeFeedSqlProcessor
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
            builder.Services.AddSingleton<ISQLServerProvider, SQLServerProvider>();
            builder.Services.AddSingleton<IChangeFeedQueueProcessorService, ChangeFeedQueueProcessorService>();
        }
    }
}