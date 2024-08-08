using System;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedSqlProcessor.Models;
using NCS.DSS.ChangeFeedSqlProcessor.Service;
using Microsoft.Azure.Functions.Worker;


namespace NCS.DSS.ChangeFeedSqlProcessor.Processor
{
    public class ChangeFeedQueueProcessor
    {
        private readonly IChangeFeedQueueProcessorService _changeFeedQueueProcessorService;        
        private readonly ILogger _logger;

        public ChangeFeedQueueProcessor(IChangeFeedQueueProcessorService changeFeedQueueProcessorService,            
            ILogger<ChangeFeedQueueProcessor> logger)
        {
            _changeFeedQueueProcessorService = changeFeedQueueProcessorService;            
            _logger = logger;
        }

        [Function("ChangeFeedQueueProcessor")]
        public async System.Threading.Tasks.Task RunAsync(
            [ServiceBusTrigger("%QueueName%", Connection = "ServiceBusConnectionString")] ChangeFeedMessageModel message)
        {
            var correlationId = Guid.NewGuid();
            if (message == null)
            {                
                _logger.LogInformation($"CorrelationId: {correlationId} Message: Brokered Message cannot be null");
                return;
            }

            try
            {
                _changeFeedQueueProcessorService.CorrelationId = correlationId;
                await _changeFeedQueueProcessorService.SendToAzureSql(message, _logger);
            }
            catch (Exception ex)
            {                
                _logger.LogError("CorrelationId: {0}  Message: {1} Exception: {2}", correlationId, "Unable to send document to sql", ex);
                throw;
            }

        }
    }
}