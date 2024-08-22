using DFC.AzureSql.Standard;
using DFC.Common.Standard.Logging;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedSqlProcessor.Models;
using System.Text.Json;

namespace NCS.DSS.ChangeFeedSqlProcessor.Service
{
    public class ChangeFeedQueueProcessorService : IChangeFeedQueueProcessorService
    {

        private readonly ILoggerHelper _loggerHelper;
        private readonly ISQLServerProvider _sqlServerProvider;

        public Guid CorrelationId { get; set; }

        public ChangeFeedQueueProcessorService(ILoggerHelper loggerHelper, ISQLServerProvider sqlServerProvider)
        {
            _loggerHelper = loggerHelper;
            _sqlServerProvider = sqlServerProvider;
        }

        public async Task<bool> SendToAzureSql(string message, ILogger log)
        {
            if (string.IsNullOrEmpty(message))
            {
                _loggerHelper.LogInformationMessage(log, CorrelationId, "document message is null");
                return false;
            }

            var messageObject = JsonDocument.Parse(message);

            var documentElementFound = messageObject.RootElement.TryGetProperty("Document", out var documentJsonElement);

            if (!documentElementFound)
            {
                _loggerHelper.LogInformationMessage(log, CorrelationId, "document is not found in the message");
                return false;
            }

            var messageModel = JsonSerializer.Deserialize<ChangeFeedMessageModel>(message);

            return await SendToStoredProc(messageModel, documentJsonElement.ToString(), log);
        }

        private async Task<bool> SendToStoredProc(ChangeFeedMessageModel documentModel, string documentJson, ILogger log)
        {
            _loggerHelper.LogMethodEnter(log);

            var resourceName = GetResourceName(documentModel);
            var commandText = "Change_Feed_Insert_Update_" + resourceName;
            const string parameterName = "@Json";
            var returnValue = false;

            if (string.IsNullOrWhiteSpace(resourceName))
            {
                _loggerHelper.LogInformationMessage(log, CorrelationId, "resource Name is null");
                return false;
            }

            try
            {
                _loggerHelper.LogInformationMessage(log, CorrelationId, "attempting to insert document into SQL");

                returnValue = await _sqlServerProvider.UpsertResource(documentJson, log, commandText, parameterName);
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(log, Guid.NewGuid(), "Error when trying to insert & update change feed request into SQL", ex);
                throw;
            }

            _loggerHelper.LogMethodExit(log);

            return returnValue;
        }

        private static string GetResourceName(ChangeFeedMessageModel documentModel)
        {
            var resourceMappings = new Dictionary<Func<ChangeFeedMessageModel, bool>, string>
                                        {
                                            { x => x.IsAction, "dss-actions" },
                                            { x => x.IsActionPlan, "dss-actionplans" },
                                            { x => x.IsAddress, "dss-addresses" },
                                            { x => x.IsAdviserDetail, "dss-adviserdetails" },
                                            { x => x.IsCollection, "dss-collections" },
                                            { x => x.IsContact, "dss-contacts" },
                                            { x => x.IsCustomer, "dss-customers" },
                                            { x => x.IsDiversity, "dss-diversity" },
                                            { x => x.IsEmploymentProgression, "dss-employmentprogressions" },
                                            { x => x.IsGoal, "dss-goals" },
                                            { x => x.IsInteraction, "dss-interactions" },
                                            { x => x.IsLearningProgression, "dss-learningprogressions" },
                                            { x => x.IsOutcome, "dss-outcomes" },
                                            { x => x.IsSession, "dss-sessions" },
                                            { x => x.IsSubscription, "dss-subscriptions" },
                                            { x => x.IsTransfer, "dss-transfers" },
                                            { x => x.IsWebChat, "dss-webchats" },
                                            { x => x.IsDigitalIdentity, "dss-digitalidentities" }
                                        };

            foreach (var mapping in resourceMappings)
            {
                if (mapping.Key(documentModel))
                {
                    return mapping.Value;
                }
            }

            return string.Empty;
        }
    }
}