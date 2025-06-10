using Microsoft.Extensions.Logging;
using Moq;

namespace NCS.DSS.ChangeFeedSqlProcessor.Service.Tests
{
    public class ChangeFeedQueueProcessorServiceTests
    {
        private Mock<ILogger<ChangeFeedQueueProcessorService>> _logger;
        private Mock<ISqlDbProvider> _sqlDbProvider;
        private ChangeFeedQueueProcessorService _service;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<ChangeFeedQueueProcessorService>>();
            _sqlDbProvider = new Mock<ISqlDbProvider>();
            _service = new ChangeFeedQueueProcessorService(_logger.Object, _sqlDbProvider.Object);
        }

        [Test]
        public async Task SendToAzureSql_ReturnsFalseAndLogsInformation_WhenMessageIsNull()
        {
            //Arrange
            var logMessage = "document message is null";

            //Act
            var result = await _service.SendToAzureSql(null);

            //Assert
            Assert.That(result, Is.False);
            _logger.Verify(x => x.Log(
               LogLevel.Information,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((x, _) => LogMessageMatcher(x, logMessage)),
               It.IsAny<Exception>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_ReturnsFalseAndLogsInformation_WhenResourceNameIsEmptyInMessageDocumentModel()
        {
            //Arrange
            var logMessage = "resource Name is null";
           

            var message = "{\"Document\":{\"id\":\"51b29377-76f6-4062-b443-c6bb5e3cad5f\",\"_rid\":\"cGgSAMDrSwAmTgcAAAAAAA==\",\"_self\":\"dbs/cGgSAA==/colls/cGgSAMDrSwA=/docs/cGgSAMDrSwAmTgcAAAAAAA==/\",\"_ts\":1723544012,\"_etag\":\"\\\"9b021ee6-0000-0d00-0000-66bb31cc0000\\\"\",\"DateOfRegistration\":\"2024-08-13T10:13:32.4487063Z\",\"Title\":99,\"GivenName\":\"Bob\",\"FamilyName\":\"Customer\",\"Gender\":99,\"OptInUserResearch\":false,\"OptInMarketResearch\":false,\"IntroducedBy\":99,\"SubcontractorId\":\"\",\"LastModifiedDate\":\"2024-08-13T10:13:32.4487093Z\",\"LastModifiedTouchpointId\":\"9999999999\",\"PriorityGroups\":[1,3],\"CreatedBy\":\"9999999999\",\"_lsn\":717483}}";

            //Act
            var result = await _service.SendToAzureSql(message);

            //Assert
            Assert.That(result, Is.False);
            _logger.Verify(x => x.Log(
               LogLevel.Information,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((x, _) => LogMessageMatcher(x, logMessage)),
               It.IsAny<Exception>(),
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Once);
        }

        [Test]
        public void SendToAzureSql_ThrowsAndLogsException_WhenExceptionOccursWhileUpsertingResourceToSqlDatabase()
        {
            //Arrange
            const string parameterName = "@Json";
            var logMessage = "Error when trying to insert & update change feed request into SQL";
            var exception = new Exception();

            _sqlDbProvider.Setup(sp => sp.UpsertResource(It.IsAny<string>(),It.IsAny<string>(), parameterName))
                .Throws(exception);

            //Act and Assert            
            var message = "{\"Document\":{\"id\":\"51b29377-76f6-4062-b443-c6bb5e3cad5f\",\"_rid\":\"cGgSAMDrSwAmTgcAAAAAAA==\",\"_self\":\"dbs/cGgSAA==/colls/cGgSAMDrSwA=/docs/cGgSAMDrSwAmTgcAAAAAAA==/\",\"_ts\":1723544012,\"_etag\":\"\\\"9b021ee6-0000-0d00-0000-66bb31cc0000\\\"\",\"DateOfRegistration\":\"2024-08-13T10:13:32.4487063Z\",\"Title\":99,\"GivenName\":\"Bob\",\"FamilyName\":\"Customer\",\"Gender\":99,\"OptInUserResearch\":false,\"OptInMarketResearch\":false,\"IntroducedBy\":99,\"SubcontractorId\":\"\",\"LastModifiedDate\":\"2024-08-13T10:13:32.4487093Z\",\"LastModifiedTouchpointId\":\"9999999999\",\"PriorityGroups\":[1,3],\"CreatedBy\":\"9999999999\",\"_lsn\":717483},\"IsAction\":true}";

            Assert.ThrowsAsync<Exception>(async () => await _service.SendToAzureSql(message));

            _logger.Verify(x => x.Log(
               LogLevel.Error,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((x, _) => LogMessageMatcher(x, logMessage)),
               exception,
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_ReturnsFalseAndLogsInformation_WhenDocumentNotFoundInMessage()
        {
            //Arrange
            var logMessage = "document is not found in the message";

            var message = "{\"IsAction\":true}";

            //Act
            var result = await _service.SendToAzureSql(message);

            //Assert
            Assert.That(result, Is.False);
            _logger.Verify(x => x.Log(LogLevel.Information, 
                It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((x, _) => LogMessageMatcher(x, logMessage)), 
                It.IsAny<Exception>(), 
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), 
                Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_UpsertsResource_WhenDocumentMessageIsForActionsEntity()
        {
            //Arrange
            var parameterName = "@Json";
            var logMessage = "attempting to insert document into SQL";
            var commandText = "Change_Feed_Insert_Update_dss-actions";
            var exception = new Exception();

            _sqlDbProvider.Setup(sp => sp.UpsertResource(It.IsAny<string>(), It.IsAny<string>(), parameterName))
                .ReturnsAsync(true);

            //Act
            var documentMessage = "{\"id\":\"51b29377-76f6-4062-b443-c6bb5e3cad5f\",\"_rid\":\"cGgSAMDrSwAmTgcAAAAAAA==\",\"_self\":\"dbs/cGgSAA==/colls/cGgSAMDrSwA=/docs/cGgSAMDrSwAmTgcAAAAAAA==/\",\"_ts\":1723544012,\"_etag\":\"\\\"9b021ee6-0000-0d00-0000-66bb31cc0000\\\"\",\"DateOfRegistration\":\"2024-08-13T10:13:32.4487063Z\",\"Title\":99,\"GivenName\":\"Bob\",\"FamilyName\":\"Customer\",\"Gender\":99,\"OptInUserResearch\":false,\"OptInMarketResearch\":false,\"IntroducedBy\":99,\"SubcontractorId\":\"\",\"LastModifiedDate\":\"2024-08-13T10:13:32.4487093Z\",\"LastModifiedTouchpointId\":\"9999999999\",\"PriorityGroups\":[1,3],\"CreatedBy\":\"9999999999\",\"_lsn\":717483}";
            var message = $"{{\"Document\":{documentMessage},\"IsAction\":true}}";
            var result = await _service.SendToAzureSql(message);

            //Assert
            Assert.That(result, Is.True);
            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((x, _) => LogMessageMatcher(x, logMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
            _sqlDbProvider.Verify(sp => sp.UpsertResource(documentMessage, commandText, parameterName), Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_UpsertsResource_WhenDocumentMessageIsForActionPlansEntity()
        {
            //Arrange
            var parameterName = "@Json";
            var logMessage = "attempting to insert document into SQL";
            var commandText = "Change_Feed_Insert_Update_dss-actionplans";
            var exception = new Exception();

            _sqlDbProvider.Setup(sp => sp.UpsertResource(It.IsAny<string>(), It.IsAny<string>(), parameterName))
                .ReturnsAsync(true);

            //Act
            var documentMessage = "{\"id\":\"51b29377-76f6-4062-b443-c6bb5e3cad5f\",\"_rid\":\"cGgSAMDrSwAmTgcAAAAAAA==\",\"_self\":\"dbs/cGgSAA==/colls/cGgSAMDrSwA=/docs/cGgSAMDrSwAmTgcAAAAAAA==/\",\"_ts\":1723544012,\"_etag\":\"\\\"9b021ee6-0000-0d00-0000-66bb31cc0000\\\"\",\"DateOfRegistration\":\"2024-08-13T10:13:32.4487063Z\",\"Title\":99,\"GivenName\":\"Bob\",\"FamilyName\":\"Customer\",\"Gender\":99,\"OptInUserResearch\":false,\"OptInMarketResearch\":false,\"IntroducedBy\":99,\"SubcontractorId\":\"\",\"LastModifiedDate\":\"2024-08-13T10:13:32.4487093Z\",\"LastModifiedTouchpointId\":\"9999999999\",\"PriorityGroups\":[1,3],\"CreatedBy\":\"9999999999\",\"_lsn\":717483}";
            var message = $"{{\"Document\":{documentMessage},\"IsAction\":false,\"IsActionPlan\":true,\"IsAddress\":false,\"IsAdviserDetail\":false,\"IsCollection\":false,\"IsContact\":false,\"IsCustomer\":false,\"IsDiversity\":false,\"IsEmploymentProgression\":false,\"IsGoal\":false,\"IsInteraction\":false,\"IsLearningProgression\":false,\"IsOutcome\":false,\"IsSession\":false,\"IsSubscription\":false,\"IsTransfer\":false,\"IsWebChat\":false,\"IsDigitalIdentity\":false}}";
            var result = await _service.SendToAzureSql(message);

            //Assert
            Assert.That(result, Is.True);
            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((x, _) => LogMessageMatcher(x, logMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
            _sqlDbProvider.Verify(sp => sp.UpsertResource(documentMessage, commandText, parameterName), Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_UpsertsResource_WhenDocumentMessageIsForAddressesEntity()
        {
            //Arrange
            var parameterName = "@Json";
            var logMessage = "attempting to insert document into SQL";
            var commandText = "Change_Feed_Insert_Update_dss-addresses";
            var exception = new Exception();

            _sqlDbProvider.Setup(sp => sp.UpsertResource(It.IsAny<string>(), It.IsAny<string>(), parameterName))
                .ReturnsAsync(true);

            //Act
            var documentMessage = "{\"id\":\"51b29377-76f6-4062-b443-c6bb5e3cad5f\",\"_rid\":\"cGgSAMDrSwAmTgcAAAAAAA==\",\"_self\":\"dbs/cGgSAA==/colls/cGgSAMDrSwA=/docs/cGgSAMDrSwAmTgcAAAAAAA==/\",\"_ts\":1723544012,\"_etag\":\"\\\"9b021ee6-0000-0d00-0000-66bb31cc0000\\\"\",\"DateOfRegistration\":\"2024-08-13T10:13:32.4487063Z\",\"Title\":99,\"GivenName\":\"Bob\",\"FamilyName\":\"Customer\",\"Gender\":99,\"OptInUserResearch\":false,\"OptInMarketResearch\":false,\"IntroducedBy\":99,\"SubcontractorId\":\"\",\"LastModifiedDate\":\"2024-08-13T10:13:32.4487093Z\",\"LastModifiedTouchpointId\":\"9999999999\",\"PriorityGroups\":[1,3],\"CreatedBy\":\"9999999999\",\"_lsn\":717483}";
            var message = $"{{\"Document\":{documentMessage},\"IsAction\":false,\"IsActionPlan\":false,\"IsAddress\":true,\"IsAdviserDetail\":false,\"IsCollection\":false,\"IsContact\":false,\"IsCustomer\":false,\"IsDiversity\":false,\"IsEmploymentProgression\":false,\"IsGoal\":false,\"IsInteraction\":false,\"IsLearningProgression\":false,\"IsOutcome\":false,\"IsSession\":false,\"IsSubscription\":false,\"IsTransfer\":false,\"IsWebChat\":false,\"IsDigitalIdentity\":false}}";
            var result = await _service.SendToAzureSql(message);

            //Assert
            Assert.That(result, Is.True);
            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((x, _) => LogMessageMatcher(x, logMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
            _sqlDbProvider.Verify(sp => sp.UpsertResource(documentMessage, commandText, parameterName), Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_UpsertsResource_WhenDocumentMessageIsForGoalsEntity()
        {
            //Arrange
            var parameterName = "@Json";
            var logMessage = "attempting to insert document into SQL";
            var commandText = "Change_Feed_Insert_Update_dss-goals";
            var exception = new Exception();

            _sqlDbProvider.Setup(sp => sp.UpsertResource(It.IsAny<string>(), It.IsAny<string>(), parameterName))
                .ReturnsAsync(true);

            //Act
            var documentMessage = "{\"id\":\"51b29377-76f6-4062-b443-c6bb5e3cad5f\",\"_rid\":\"cGgSAMDrSwAmTgcAAAAAAA==\",\"_self\":\"dbs/cGgSAA==/colls/cGgSAMDrSwA=/docs/cGgSAMDrSwAmTgcAAAAAAA==/\",\"_ts\":1723544012,\"_etag\":\"\\\"9b021ee6-0000-0d00-0000-66bb31cc0000\\\"\",\"DateOfRegistration\":\"2024-08-13T10:13:32.4487063Z\",\"Title\":99,\"GivenName\":\"Bob\",\"FamilyName\":\"Customer\",\"Gender\":99,\"OptInUserResearch\":false,\"OptInMarketResearch\":false,\"IntroducedBy\":99,\"SubcontractorId\":\"\",\"LastModifiedDate\":\"2024-08-13T10:13:32.4487093Z\",\"LastModifiedTouchpointId\":\"9999999999\",\"PriorityGroups\":[1,3],\"CreatedBy\":\"9999999999\",\"_lsn\":717483}";
            var message = $"{{\"Document\":{documentMessage},\"IsAction\":false,\"IsActionPlan\":false,\"IsAddress\":false,\"IsAdviserDetail\":false,\"IsCollection\":false,\"IsContact\":false,\"IsCustomer\":false,\"IsDiversity\":false,\"IsEmploymentProgression\":false,\"IsGoal\":true,\"IsInteraction\":false,\"IsLearningProgression\":false,\"IsOutcome\":false,\"IsSession\":false,\"IsSubscription\":false,\"IsTransfer\":false,\"IsWebChat\":false,\"IsDigitalIdentity\":false}}";
            var result = await _service.SendToAzureSql(message);

            //Assert
            Assert.That(result, Is.True);
            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((x, _) => LogMessageMatcher(x, logMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
            _sqlDbProvider.Verify(sp => sp.UpsertResource(documentMessage, commandText, parameterName), Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_UpsertsResource_WhenDocumentMessageIsForWebChatsEntity()
        {
            //Arrange
            var parameterName = "@Json";
            var logMessage = "attempting to insert document into SQL";
            var commandText = "Change_Feed_Insert_Update_dss-webchats";
            var exception = new Exception();

            _sqlDbProvider.Setup(sp => sp.UpsertResource(It.IsAny<string>(), It.IsAny<string>(), parameterName))
                .ReturnsAsync(true);

            //Act
            var documentMessage = "{\"id\":\"51b29377-76f6-4062-b443-c6bb5e3cad5f\",\"_rid\":\"cGgSAMDrSwAmTgcAAAAAAA==\",\"_self\":\"dbs/cGgSAA==/colls/cGgSAMDrSwA=/docs/cGgSAMDrSwAmTgcAAAAAAA==/\",\"_ts\":1723544012,\"_etag\":\"\\\"9b021ee6-0000-0d00-0000-66bb31cc0000\\\"\",\"DateOfRegistration\":\"2024-08-13T10:13:32.4487063Z\",\"Title\":99,\"GivenName\":\"Bob\",\"FamilyName\":\"Customer\",\"Gender\":99,\"OptInUserResearch\":false,\"OptInMarketResearch\":false,\"IntroducedBy\":99,\"SubcontractorId\":\"\",\"LastModifiedDate\":\"2024-08-13T10:13:32.4487093Z\",\"LastModifiedTouchpointId\":\"9999999999\",\"PriorityGroups\":[1,3],\"CreatedBy\":\"9999999999\",\"_lsn\":717483}";
            var message = $"{{\"Document\":{documentMessage},\"IsAction\":false,\"IsActionPlan\":false,\"IsAddress\":false,\"IsAdviserDetail\":false,\"IsCollection\":false,\"IsContact\":false,\"IsCustomer\":false,\"IsDiversity\":false,\"IsEmploymentProgression\":false,\"IsGoal\":false,\"IsInteraction\":false,\"IsLearningProgression\":false,\"IsOutcome\":false,\"IsSession\":false,\"IsSubscription\":false,\"IsTransfer\":false,\"IsWebChat\":true,\"IsDigitalIdentity\":false}}";
            var result = await _service.SendToAzureSql(message);

            //Assert
            Assert.That(result, Is.True);
            _logger.Verify(x => x.Log(LogLevel.Information, It.IsAny<EventId>(), It.Is<It.IsAnyType>((x, _) => LogMessageMatcher(x, logMessage)), It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
            _sqlDbProvider.Verify(sp => sp.UpsertResource(documentMessage, commandText, parameterName), Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_UpsertsResource_WhenDocumentMessageIsForDigitalIdentitiesEntity()
        {
            //Arrange
            var parameterName = "@Json";
            var logMessage = "attempting to insert document into SQL";
            var commandText = "Change_Feed_Insert_Update_dss-digitalidentities";
            var exception = new Exception();

            _sqlDbProvider.Setup(sp => sp.UpsertResource(It.IsAny<string>(), It.IsAny<string>(), parameterName))
                .ReturnsAsync(true);

            //Act
            var documentMessage = "{\"id\":\"51b29377-76f6-4062-b443-c6bb5e3cad5f\",\"_rid\":\"cGgSAMDrSwAmTgcAAAAAAA==\",\"_self\":\"dbs/cGgSAA==/colls/cGgSAMDrSwA=/docs/cGgSAMDrSwAmTgcAAAAAAA==/\",\"_ts\":1723544012,\"_etag\":\"\\\"9b021ee6-0000-0d00-0000-66bb31cc0000\\\"\",\"DateOfRegistration\":\"2024-08-13T10:13:32.4487063Z\",\"Title\":99,\"GivenName\":\"Bob\",\"FamilyName\":\"Customer\",\"Gender\":99,\"OptInUserResearch\":false,\"OptInMarketResearch\":false,\"IntroducedBy\":99,\"SubcontractorId\":\"\",\"LastModifiedDate\":\"2024-08-13T10:13:32.4487093Z\",\"LastModifiedTouchpointId\":\"9999999999\",\"PriorityGroups\":[1,3],\"CreatedBy\":\"9999999999\",\"_lsn\":717483}";
            var message = $"{{\"Document\":{documentMessage},\"IsAction\":false,\"IsActionPlan\":false,\"IsAddress\":false,\"IsAdviserDetail\":false,\"IsCollection\":false,\"IsContact\":false,\"IsCustomer\":false,\"IsDiversity\":false,\"IsEmploymentProgression\":false,\"IsGoal\":false,\"IsInteraction\":false,\"IsLearningProgression\":false,\"IsOutcome\":false,\"IsSession\":false,\"IsSubscription\":false,\"IsTransfer\":false,\"IsWebChat\":false,\"IsDigitalIdentity\":true}}";
            var result = await _service.SendToAzureSql(message);

            //Assert
            Assert.That(result, Is.True);
            _logger.Verify(x => x.Log(LogLevel.Information, 
                It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((x, _) => LogMessageMatcher(x, logMessage)), 
                It.IsAny<Exception>(), 
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), 
                Times.Once);
            _sqlDbProvider.Verify(sp => sp.UpsertResource(documentMessage, commandText, parameterName), Times.Once);
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