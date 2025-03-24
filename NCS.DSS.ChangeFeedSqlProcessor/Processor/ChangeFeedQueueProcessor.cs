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
            var functionName = nameof(ChangeFeedQueueProcessor);

            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            var correlationId = Guid.NewGuid();

            if (message == null)
            {
                _logger.LogWarning("{CorrelationId} Message: Service Bus Received Message cannot be null",correlationId);
                return;
            }

            try
            {
                _changeFeedQueueProcessorService.CorrelationId = correlationId;
                _logger.LogInformation("{CorrelationId} Attempting to apply update to SQL database",correlationId);
                var response = await _changeFeedQueueProcessorService.SendToAzureSql(message.Body.ToString());
                if(response)
                {
                    _logger.LogInformation("{CorrelationId} Message: Successfully Updated SQL Record",correlationId);
                }
                else
                {
                    _logger.LogWarning("{CorrelationId} Message: Failed to Update SQL Record",correlationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"{CorrelationId} Message: Unable to send document to sql Exception: {Exception}",correlationId,ex.Message);
                throw;
            }
            _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
        }
    }
}