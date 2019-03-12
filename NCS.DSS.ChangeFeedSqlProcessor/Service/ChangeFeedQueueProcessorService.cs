using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DFC.AzureSql.Standard;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.InteropExtensions;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedSqlProcessor.Models;
using Newtonsoft.Json;

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


        public async Task<bool> SendToAzureSql(Message queueItem, ILogger log)
        {
            if (queueItem == null)
                return false;

            string body;

            try
            {
                body = Encoding.UTF8.GetString(queueItem.Body);
            }
            catch (Exception e)
            {
                _loggerHelper.LogException(log, CorrelationId, "unable to get string from queue Item body", e);
                throw;
            }

            if (string.IsNullOrEmpty(body))
                return false;

            ChangeFeedMessageModel documentModel;

            try
            {
                documentModel = JsonConvert.DeserializeObject<ChangeFeedMessageModel>(body);
            }
            catch (JsonException e)
            {
                _loggerHelper.LogException(log, CorrelationId, "unable to deserialize document model", e);
                throw;
            }

            if (documentModel == null)
                return false;

            await SendToStoredProc(documentModel, log);

            return true;
        }

        private async Task SendToStoredProc(ChangeFeedMessageModel documentModel, ILogger log)
        {
            _loggerHelper.LogMethodEnter(log);

            if (documentModel == null)
            {
                _loggerHelper.LogInformationMessage(log, CorrelationId, "document model is null");
                return;
            }

            var resourceName = GetResourceName(documentModel);
            var commandText = "Change_Feed_Insert_Update_" + resourceName;
            const string parameterName = "@Json";

            if (string.IsNullOrWhiteSpace(resourceName))
            {
                _loggerHelper.LogInformationMessage(log, CorrelationId, "resource Name is null");
                return;
            }

            try
            {
                _loggerHelper.LogInformationMessage(log, CorrelationId, "attempting to insert document into SQL");
                await _sqlServerProvider.UpsertResource(documentModel.Document, log, commandText, parameterName);
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(log, Guid.NewGuid(), "Error when trying to insert & update change feed request into SQL", ex);
            }

            _loggerHelper.LogMethodExit(log);
                       
        }


        private static string GetResourceName(ChangeFeedMessageModel documentModel)
        {
            if (documentModel.IsAction)
            {
                return "dss-actions";
            }
            else if (documentModel.IsActionPlan)
            {
                return "dss-actionplans";
            }
            else if (documentModel.IsAddress)
            {
                return "dss-addresses";
            }
            else if (documentModel.IsAdviserDetail)
            {
                return "dss-adviserdetails";
            }
            else if (documentModel.IsCustomer)
            {
                return "dss-customers";
            }
            else if (documentModel.IsGoal)
            {
                return "dss-goals";
            }
            else if (documentModel.IsOutcome)
            {
                return "dss-outcomes";
            }
            else if (documentModel.IsSession)
            {
                return "dss-sessions";
            }
            else
            {
                return string.Empty;
            }
        }

    }
}

