using System;
using DFC.Common.Standard.Logging;
using DFC.Functions.DI.Standard.Attributes;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedSqlProcessor.Service;

namespace NCS.DSS.ChangeFeedSqlProcessor.Processor
{
    public static class ChangeFeedQueueProcessor
    {

        [FunctionName("ChangeFeedQueueProcessor")]
        public static async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("dss.changefeedsqlprocessor", Connection = "ServiceBusConnectionString")]Message queueItem, 
            ILogger log,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IChangeFeedQueueProcessorService changeFeedQueueProcessorService)
        {
            if (queueItem == null)
            {
                log.LogError("Brokered Message cannot be null");
                return;
            }

            try
            {
                await changeFeedQueueProcessorService.SendToAzureSql(queueItem, log);
            }
            catch (Exception ex)
            {
                log.LogError(ex.StackTrace);
                throw;
            }


        }
    }
}
