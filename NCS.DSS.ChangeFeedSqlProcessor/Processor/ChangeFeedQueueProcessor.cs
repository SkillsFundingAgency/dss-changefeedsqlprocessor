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
            [ServiceBusTrigger("%QueueName%", Connection = "ServiceBusConnectionString")]Message queueItem, 
            ILogger log,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IChangeFeedQueueProcessorService changeFeedQueueProcessorService)
        {
            var correlationId = Guid.NewGuid();
            if (queueItem == null)
            {
                loggerHelper.LogInformationMessage(log, correlationId, "Brokered Message cannot be null");
                return;
            }

            try
            {
                changeFeedQueueProcessorService.CorrelationId = correlationId;
                await changeFeedQueueProcessorService.SendToAzureSql(queueItem, log);
            }
            catch (Exception ex)
            {
                loggerHelper.LogError(log, correlationId, "Unable to send document to sql", ex);
                throw;
            }


        }
    }
}
