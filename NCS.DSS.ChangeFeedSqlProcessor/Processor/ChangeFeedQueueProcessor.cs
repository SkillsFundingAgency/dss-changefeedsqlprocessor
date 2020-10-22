using System;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedSqlProcessor.Service;


namespace NCS.DSS.ChangeFeedSqlProcessor.Processor
{
    public class ChangeFeedQueueProcessor
    {
        private readonly IChangeFeedQueueProcessorService _changeFeedQueueProcessorService;
        private readonly ILoggerHelper loggerHelper;

        public ChangeFeedQueueProcessor(IChangeFeedQueueProcessorService changeFeedQueueProcessorService,
            ILoggerHelper _loggerHelper)
        {
            _changeFeedQueueProcessorService = changeFeedQueueProcessorService;
            loggerHelper = _loggerHelper;
        }

        [FunctionName("ChangeFeedQueueProcessor")]
        public async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("%QueueName%", Connection = "ServiceBusConnectionString")]Message queueItem, 
            ILogger log)
        {

            var correlationId = Guid.NewGuid();
            if (queueItem == null)
            {
                loggerHelper.LogInformationMessage(log, correlationId, "Brokered Message cannot be null");
                return;
            }

            try
            {
                _changeFeedQueueProcessorService.CorrelationId = correlationId;
                await _changeFeedQueueProcessorService.SendToAzureSql(queueItem, log);
            }
            catch (Exception ex)
            {
                loggerHelper.LogException(log, correlationId, "Unable to send document to sql", ex);
                throw;
            }

        }
    }
}