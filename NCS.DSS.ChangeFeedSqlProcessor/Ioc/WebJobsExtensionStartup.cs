using System;
using System.Data;
using System.Data.SqlClient;
using DFC.AzureSql.Standard;
using DFC.Common.Standard.Logging;
using DFC.Functions.DI.Standard;
using DFC.JSON.Standard;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.ChangeFeedSqlProcessor.Ioc;
using NCS.DSS.ChangeFeedSqlProcessor.Service;

[assembly: WebJobsStartup(typeof(WebJobsExtensionStartup), "Web Jobs Extension Startup")]

namespace NCS.DSS.ChangeFeedSqlProcessor.Ioc
{
    public class WebJobsExtensionStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddDependencyInjection();

            RegisterHelpers(builder);
            RegisterServices(builder);
            RegisterSQLServer(builder);
        }

        private void RegisterHelpers(IWebJobsBuilder builder)
        {
            builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
            builder.Services.AddSingleton<IJsonHelper, JsonHelper>();
        }

        private void RegisterServices(IWebJobsBuilder builder)
        {
            builder.Services.AddScoped<IChangeFeedQueueProcessorService, ChangeFeedQueueProcessorService>();
        }

        private void RegisterSQLServer(IWebJobsBuilder builder)
        {
            builder.Services.AddScoped<ISQLServerProvider, SQLServerProvider>();
        }
    }
}
