using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedSqlProcessor.Service;

namespace NCS.DSS.ChangeFeedSqlProcessor.Processor
{
    public class ChangeFeedQueueProcessor
    {
        private readonly IChangeFeedQueueProcessorService _changeFeedQueueProcessorService;
        private readonly ILogger<ChangeFeedQueueProcessor> _logger;

        public ChangeFeedQueueProcessor(IChangeFeedQueueProcessorService changeFeedQueueProcessorService,
            ILogger<ChangeFeedQueueProcessor> logger)
        {
            _changeFeedQueueProcessorService = changeFeedQueueProcessorService;
            _logger = logger;
        }

        [Function("ChangeFeedQueueProcessor")]
        public async Task RunAsync(
            [ServiceBusTrigger("%QueueName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message)
        {
            var correlationId = Guid.NewGuid();

            if (message == null)
            {
                _logger.LogInformation($"CorrelationId: {correlationId} Message: Service Bus Received Message cannot be null");
                return;
            }

            try
            {
                _changeFeedQueueProcessorService.CorrelationId = correlationId;
                await _changeFeedQueueProcessorService.SendToAzureSql(message.Body.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError($"CorrelationId: {correlationId}  Message: Unable to send document to sql Exception: {ex}");
                throw;
            }

        }
    }
}