using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.ChangeFeedSqlProcessor.Models;
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
            var logMessage = "Message: Brokered Message cannot be null";

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
        public async Task RunAsync_CallsServiceMethod_WhenMessageModelIsValid()
        {
            //Act
            var documentModel = new ChangeFeedMessageModel { Document = new Microsoft.Azure.Documents.Document(), IsAction = true };
            await _processor.RunAsync(documentModel);

            //Assert                        
            Assert.Pass();
        }

        [Test]
        public void RunAsync_ThrowsAndLogsException_WhenExceptionOccursWhileCallingServiceMethod()
        {
            //Arrange            
            var logMessage = "Unable to send document to sql";
            var exception = new Exception();

            _changeFeedQueueProcessorService.Setup(s => s.SendToAzureSql(It.IsAny<ChangeFeedMessageModel>(), It.IsAny<ILogger>()))
                .Throws(exception);

            //Act and Assert
            var documentModel = new ChangeFeedMessageModel { Document = new Microsoft.Azure.Documents.Document(), IsAction = true };

            Assert.ThrowsAsync<Exception>(async () => await _processor.RunAsync(documentModel));

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

            var logData = logValues.FirstOrDefault(logValue => logValue.Key == "{OriginalFormat}");

            if (logData.Value == null)
                return false;

            var loggedMessage = logData.Value.ToString();

            return loggedMessage.Contains(message);
        }
    }
}