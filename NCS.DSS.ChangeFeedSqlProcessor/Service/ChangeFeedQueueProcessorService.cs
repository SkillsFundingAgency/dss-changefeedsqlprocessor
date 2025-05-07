using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedSqlProcessor.Models;
using System.Text.Json;

namespace NCS.DSS.ChangeFeedSqlProcessor.Service
{
    public class ChangeFeedQueueProcessorService : IChangeFeedQueueProcessorService
    {

        private readonly ILogger<ChangeFeedQueueProcessorService> _logger;
        private readonly ISqlDbProvider _sqlDbProvider;

        public Guid CorrelationId { get; set; }

        public ChangeFeedQueueProcessorService(ILogger<ChangeFeedQueueProcessorService> logger, ISqlDbProvider sqldbProvider)
        {
            _logger = logger;
            _sqlDbProvider = sqldbProvider;
        }

        public async Task<bool> SendToAzureSql(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                _logger.LogWarning("{CorrelationId} document message is null",CorrelationId);
                return false;
            }

            var messageObject = JsonDocument.Parse(message);

            var documentElementFound = messageObject.RootElement.TryGetProperty("Document", out var documentJsonElement);

            if (!documentElementFound)
            {
                _logger.LogWarning("{CorrelationId} document is not found in the message",CorrelationId);
                return false;
            }

            var messageModel = JsonSerializer.Deserialize<ChangeFeedMessageModel>(message);

            return await SendToStoredProc(messageModel, documentJsonElement.ToString());
        }

        private async Task<bool> SendToStoredProc(ChangeFeedMessageModel documentModel, string documentJson)
        {           
            var resourceName = GetResourceName(documentModel);
            var commandText = "Change_Feed_Insert_Update_" + resourceName;
            const string parameterName = "@Json";
            var returnValue = false;

            if (string.IsNullOrWhiteSpace(resourceName))
            {
                _logger.LogWarning("{CorrelationId} resource Name is null", CorrelationId);
                return false;
            }

            try
            {
                _logger.LogInformation("{CorrelationId} attempting to insert document into SQL", CorrelationId);

                returnValue = await _sqlDbProvider.UpsertResource(documentJson, commandText, parameterName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{CorrelationId} Error when trying to insert & update change feed request into SQL",CorrelationId);
                throw;
            }

            return returnValue;
        }

        private string GetResourceName(ChangeFeedMessageModel documentModel)
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
                    _logger.LogInformation("{CorrelationId} Update on Cosmos DB {cdb} has been found", CorrelationId,mapping.Value);
                    return mapping.Value;
                }
            }

            return string.Empty;
        }
    }
}