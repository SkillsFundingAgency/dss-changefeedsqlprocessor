using DFC.AzureSql.Standard;
using DFC.Common.Standard.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.ChangeFeedSqlProcessor.Service;
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services => {
        services.AddLogging();
        services.AddSingleton<ILoggerHelper, LoggerHelper>();
        services.AddSingleton<ISQLServerProvider, SQLServerProvider>();
        services.AddSingleton<IChangeFeedQueueProcessorService, ChangeFeedQueueProcessorService>();
    })
    .Build();

host.Run();
