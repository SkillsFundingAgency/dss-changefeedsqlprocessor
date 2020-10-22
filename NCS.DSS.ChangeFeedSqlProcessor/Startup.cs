
using DFC.AzureSql.Standard;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.ChangeFeedSqlProcessor;

[assembly: FunctionsStartup(typeof(Startup))]
namespace NCS.DSS.ChangeFeedSqlProcessor
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //builder.Services.AddSingleton<IServiceBusClient, ServiceBusClient>();
            builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
            builder.Services.AddSingleton<ISQLServerProvider, SQLServerProvider>();
        }
    }
}