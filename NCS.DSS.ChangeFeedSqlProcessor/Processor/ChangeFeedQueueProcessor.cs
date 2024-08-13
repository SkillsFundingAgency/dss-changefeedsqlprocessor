using System;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedSqlProcessor.Models;
using NCS.DSS.ChangeFeedSqlProcessor.Service;
using Microsoft.Azure.Functions.Worker;
using Azure.Messaging.ServiceBus;


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
            [ServiceBusTrigger("%QueueName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message)
        {
            var correlationId = Guid.NewGuid();

            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);


            if (message == null)
            {                
                _logger.LogInformation($"CorrelationId: {correlationId} Message: Brokered Message cannot be null");
                return;
            }
            _logger.LogInformation($"This is the new version 3");
            try
            {
                _changeFeedQueueProcessorService.CorrelationId = correlationId;
                
                //await _changeFeedQueueProcessorService.SendToAzureSql(message, _logger);
            }
            catch (Exception ex)
            {                
                _logger.LogError($"CorrelationId: {correlationId}  Message: Unable to send document to sql Exception: {ex}");
                throw;
            }

        }
    }
}