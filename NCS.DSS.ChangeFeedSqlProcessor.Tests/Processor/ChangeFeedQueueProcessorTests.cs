using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.ChangeFeedSqlProcessor.Service;

namespace NCS.DSS.ChangeFeedSqlProcessor.Processor.Tests
{
    public class ChangeFeedQueueProcessorTests
    {
        private Mock<ILogger<ChangeFeedQueueProcessor>> _logger;
        private Mock<IChangeFeedQueueProcessorService> _changeFeedQueueProcessorService;
        private ChangeFeedQueueProcessor _processor;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<ChangeFeedQueueProcessor>>();
            _changeFeedQueueProcessorService = new Mock<IChangeFeedQueueProcessorService>();
            _processor = new ChangeFeedQueueProcessor(_changeFeedQueueProcessorService.Object, _logger.Object);
        }

        [Test]
        public async Task RunAsync_LogsInformation_WhenMessageModelIsNull()
        {
            //Arrange
            var logMessage = "Message: Service Bus Received Message cannot be null";

            //Act
            await _processor.RunAsync(null);

            //Assert                        
            _logger.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((x, _) => LogMessageMatcher(x, logMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task RunAsync_CallsServiceMethod_WhenReceivedMessageIsValid()
        {
            var body = "{\"Document\":{\"id\":\"51b29377-76f6-4062-b443-c6bb5e3cad5f\",\"_rid\":\"cGgSAMDrSwAmTgcAAAAAAA==\",\"_self\":\"dbs/cGgSAA==/colls/cGgSAMDrSwA=/docs/cGgSAMDrSwAmTgcAAAAAAA==/\",\"_ts\":1723544012,\"_etag\":\"\\\"9b021ee6-0000-0d00-0000-66bb31cc0000\\\"\",\"DateOfRegistration\":\"2024-08-13T10:13:32.4487063Z\",\"Title\":99,\"GivenName\":\"Bob\",\"FamilyName\":\"Customer\",\"Gender\":99,\"OptInUserResearch\":false,\"OptInMarketResearch\":false,\"IntroducedBy\":99,\"SubcontractorId\":\"\",\"LastModifiedDate\":\"2024-08-13T10:13:32.4487093Z\",\"LastModifiedTouchpointId\":\"9999999999\",\"PriorityGroups\":[1,3],\"CreatedBy\":\"9999999999\",\"_lsn\":717483},\"IsAction\":true}";

            //Act
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(BinaryData.FromString(body), correlationId: Guid.NewGuid().ToString());
            await _processor.RunAsync(serviceBusReceivedMessage);

            //Assert                        
            Assert.Pass();
        }

        [Test]
        public void RunAsync_ThrowsAndLogsException_WhenExceptionOccursWhileCallingServiceMethod()
        {
            //Arrange            
            var logMessage = "Unable to send document to sql";
            var body = "{\"Document\":{\"id\":\"51b29377-76f6-4062-b443-c6bb5e3cad5f\",\"_rid\":\"cGgSAMDrSwAmTgcAAAAAAA==\",\"_self\":\"dbs/cGgSAA==/colls/cGgSAMDrSwA=/docs/cGgSAMDrSwAmTgcAAAAAAA==/\",\"_ts\":1723544012,\"_etag\":\"\\\"9b021ee6-0000-0d00-0000-66bb31cc0000\\\"\",\"DateOfRegistration\":\"2024-08-13T10:13:32.4487063Z\",\"Title\":99,\"GivenName\":\"Bob\",\"FamilyName\":\"Customer\",\"Gender\":99,\"OptInUserResearch\":false,\"OptInMarketResearch\":false,\"IntroducedBy\":99,\"SubcontractorId\":\"\",\"LastModifiedDate\":\"2024-08-13T10:13:32.4487093Z\",\"LastModifiedTouchpointId\":\"9999999999\",\"PriorityGroups\":[1,3],\"CreatedBy\":\"9999999999\",\"_lsn\":717483},\"IsAction\":true}";
            var exception = new Exception();

            _changeFeedQueueProcessorService.Setup(s => s.SendToAzureSql(It.IsAny<string>()))
                .Throws(exception);

            //Act and Assert
            var serviceBusReceivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(BinaryData.FromString(body), correlationId: Guid.NewGuid().ToString());

            Assert.ThrowsAsync<Exception>(async () => await _processor.RunAsync(serviceBusReceivedMessage));

            _logger.Verify(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((x, _) => LogMessageMatcher(x, logMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static bool LogMessageMatcher(object formattedLogValueObject, string message)
        {
            var logValues = formattedLogValueObject as IReadOnlyList<KeyValuePair<string, object>>;

            if (logValues == null)
                return false;

            var loggedMessage = logValues.FirstOrDefault(logValue => logValue.Key == "{OriginalFormat}").Value?.ToString();

            return loggedMessage?.Contains(message) ?? false;
        }
    }
}