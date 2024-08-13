using DFC.AzureSql.Standard;
using DFC.Common.Standard.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.ChangeFeedSqlProcessor.Models;

namespace NCS.DSS.ChangeFeedSqlProcessor.Service.Tests
{
    public class ChangeFeedQueueProcessorServiceTests
    {
        private Mock<ILogger> _logger;
        private Mock<ILoggerHelper> _loggerHelper;
        private Mock<ISQLServerProvider> _sqlServerProvider;
        private ChangeFeedQueueProcessorService _service;

        [SetUp]
        public void Setup()
        {
            _loggerHelper = new Mock<ILoggerHelper>();
            _logger = new Mock<ILogger>();
            _sqlServerProvider = new Mock<ISQLServerProvider>();
            _service = new ChangeFeedQueueProcessorService(_loggerHelper.Object, _sqlServerProvider.Object);
        }

        [Test]
        public async Task SendToAzureSql_ReturnsTrueAndLogsInformation_WhenMessageDocumentIsNull()
        {
            //Arrange
            var logMessage = "document model is null";
            _loggerHelper.Setup(l => l.LogInformationMessage(_logger.Object, It.IsAny<Guid>(), logMessage)).Verifiable();

            ChangeFeedMessageModel model = null;

            //Act
            var result = await _service.SendToAzureSql(model, _logger.Object);

            //Assert
            Assert.That(result, Is.True);
            _loggerHelper.Verify(l => l.LogInformationMessage(_logger.Object, It.IsAny<Guid>(), logMessage), Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_ReturnsTrueAndLogsInformation_WhenResourceNameIsEmptyInMessageDocumentModel()
        {
            //Arrange
            var logMessage = "resource Name is null";
            _loggerHelper.Setup(l => l.LogInformationMessage(_logger.Object, It.IsAny<Guid>(), logMessage)).Verifiable();

            //Act
            var result = await _service.SendToAzureSql(new Models.ChangeFeedMessageModel { Document = new Microsoft.Azure.Documents.Document() }, _logger.Object);

            //Assert
            Assert.That(result, Is.True);
            _loggerHelper.Verify(l => l.LogInformationMessage(_logger.Object, It.IsAny<Guid>(), logMessage), Times.Once);            
        }

        [Test]
        public void SendToAzureSql_ThrowsAndLogsException_WhenExceptionOccursWhileUpsertingResourceToSqlDatabase()
        {
            //Arrange
            const string parameterName = "@Json";
            var logMessage = "Error when trying to insert & update change feed request into SQL";
            var exception = new Exception();

            _sqlServerProvider.Setup(sp => sp.UpsertResource(It.IsAny<Microsoft.Azure.Documents.Document>(), It.IsAny<ILogger>(), It.IsAny<string>(), parameterName))
                .Throws(exception);

            //Act and Assert
            var documentModel = new Models.ChangeFeedMessageModel { Document = new Microsoft.Azure.Documents.Document(), IsAction = true };

            Assert.ThrowsAsync<Exception>(async () => await _service.SendToAzureSql(documentModel, _logger.Object));
            
            _loggerHelper.Verify(l => l.LogException(_logger.Object, It.IsAny<Guid>(), logMessage, exception), Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_UpsertsResource_WhenDocumentMessageIsForActionsEntity()
        {
            //Arrange
            var parameterName = "@Json";
            var logMessage = "attempting to insert document into SQL";
            var commandText = "Change_Feed_Insert_Update_dss-actions";
            var exception = new Exception();

            _sqlServerProvider.Setup(sp => sp.UpsertResource(It.IsAny<Microsoft.Azure.Documents.Document>(), It.IsAny<ILogger>(), It.IsAny<string>(), parameterName))
                .ReturnsAsync(true);

            //Act
            var documentModel = new Models.ChangeFeedMessageModel
            {
                Document = new Microsoft.Azure.Documents.Document(),
                IsAction = true
            };
            var result = await _service.SendToAzureSql(documentModel, _logger.Object);

            //Assert
            Assert.That(result, Is.True);
            _loggerHelper.Verify(l => l.LogInformationMessage(_logger.Object, It.IsAny<Guid>(), logMessage), Times.Once);
            _sqlServerProvider.Verify(sp => sp.UpsertResource(documentModel.Document, It.IsAny<ILogger>(), commandText, parameterName), Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_UpsertsResource_WhenDocumentMessageIsForActionPlansEntity()
        {
            //Arrange
            var parameterName = "@Json";
            var logMessage = "attempting to insert document into SQL";
            var commandText = "Change_Feed_Insert_Update_dss-actionplans";
            var exception = new Exception();

            _sqlServerProvider.Setup(sp => sp.UpsertResource(It.IsAny<Microsoft.Azure.Documents.Document>(), It.IsAny<ILogger>(), It.IsAny<string>(), parameterName))
                .ReturnsAsync(true);

            //Act
            var documentModel = new Models.ChangeFeedMessageModel
            {
                Document = new Microsoft.Azure.Documents.Document(),
                IsActionPlan = true
            };
            var result = await _service.SendToAzureSql(documentModel, _logger.Object);

            //Assert
            Assert.That(result, Is.True);
            _loggerHelper.Verify(l => l.LogInformationMessage(_logger.Object, It.IsAny<Guid>(), logMessage), Times.Once);
            _sqlServerProvider.Verify(sp => sp.UpsertResource(documentModel.Document, It.IsAny<ILogger>(), commandText, parameterName), Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_UpsertsResource_WhenDocumentMessageIsForAddressesEntity()
        {
            //Arrange
            var parameterName = "@Json";
            var logMessage = "attempting to insert document into SQL";
            var commandText = "Change_Feed_Insert_Update_dss-addresses";
            var exception = new Exception();

            _sqlServerProvider.Setup(sp => sp.UpsertResource(It.IsAny<Microsoft.Azure.Documents.Document>(), It.IsAny<ILogger>(), It.IsAny<string>(), parameterName))
                .ReturnsAsync(true);

            //Act
            var documentModel = new Models.ChangeFeedMessageModel
            {
                Document = new Microsoft.Azure.Documents.Document(),
                IsAddress = true
            };
            var result = await _service.SendToAzureSql(documentModel, _logger.Object);

            //Assert
            Assert.That(result, Is.True);
            _loggerHelper.Verify(l => l.LogInformationMessage(_logger.Object, It.IsAny<Guid>(), logMessage), Times.Once);
            _sqlServerProvider.Verify(sp => sp.UpsertResource(documentModel.Document, It.IsAny<ILogger>(), commandText, parameterName), Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_UpsertsResource_WhenDocumentMessageIsForGoalsEntity()
        {
            //Arrange
            var parameterName = "@Json";
            var logMessage = "attempting to insert document into SQL";
            var commandText = "Change_Feed_Insert_Update_dss-goals";
            var exception = new Exception();

            _sqlServerProvider.Setup(sp => sp.UpsertResource(It.IsAny<Microsoft.Azure.Documents.Document>(), It.IsAny<ILogger>(), It.IsAny<string>(), parameterName))
                .ReturnsAsync(true);

            //Act
            var documentModel = new Models.ChangeFeedMessageModel
            {
                Document = new Microsoft.Azure.Documents.Document(),
                IsGoal = true
            };
            var result = await _service.SendToAzureSql(documentModel, _logger.Object);

            //Assert
            Assert.That(result, Is.True);
            _loggerHelper.Verify(l => l.LogInformationMessage(_logger.Object, It.IsAny<Guid>(), logMessage), Times.Once);
            _sqlServerProvider.Verify(sp => sp.UpsertResource(documentModel.Document, It.IsAny<ILogger>(), commandText, parameterName), Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_UpsertsResource_WhenDocumentMessageIsForWebChatsEntity()
        {
            //Arrange
            var parameterName = "@Json";
            var logMessage = "attempting to insert document into SQL";
            var commandText = "Change_Feed_Insert_Update_dss-webchats";
            var exception = new Exception();

            _sqlServerProvider.Setup(sp => sp.UpsertResource(It.IsAny<Microsoft.Azure.Documents.Document>(), It.IsAny<ILogger>(), It.IsAny<string>(), parameterName))
                .ReturnsAsync(true);

            //Act
            var documentModel = new Models.ChangeFeedMessageModel
            {
                Document = new Microsoft.Azure.Documents.Document(),
                IsWebChat = true
            };
            var result = await _service.SendToAzureSql(documentModel, _logger.Object);

            //Assert
            Assert.That(result, Is.True);
            _loggerHelper.Verify(l => l.LogInformationMessage(_logger.Object, It.IsAny<Guid>(), logMessage), Times.Once);
            _sqlServerProvider.Verify(sp => sp.UpsertResource(documentModel.Document, It.IsAny<ILogger>(), commandText, parameterName), Times.Once);
        }

        [Test]
        public async Task SendToAzureSql_UpsertsResource_WhenDocumentMessageIsForDigitalIdentitiesEntity()
        {
            //Arrange
            var parameterName = "@Json";
            var logMessage = "attempting to insert document into SQL";
            var commandText = "Change_Feed_Insert_Update_dss-digitalidentities";
            var exception = new Exception();

            _sqlServerProvider.Setup(sp => sp.UpsertResource(It.IsAny<Microsoft.Azure.Documents.Document>(), It.IsAny<ILogger>(), It.IsAny<string>(), parameterName))
                .ReturnsAsync(true);

            //Act
            var documentModel = new Models.ChangeFeedMessageModel
            {
                Document = new Microsoft.Azure.Documents.Document(),
                IsDigitalIdentity = true
            };
            var result = await _service.SendToAzureSql(documentModel, _logger.Object);

            //Assert
            Assert.That(result, Is.True);
            _loggerHelper.Verify(l => l.LogInformationMessage(_logger.Object, It.IsAny<Guid>(), logMessage), Times.Once);
            _sqlServerProvider.Verify(sp => sp.UpsertResource(documentModel.Document, It.IsAny<ILogger>(), commandText, parameterName), Times.Once);
        }
    }
}