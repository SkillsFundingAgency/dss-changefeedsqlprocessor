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

        public ChangeFeedQueueProcessorService(ILoggerHelper loggerHelper, ISQLServerProvider sqlServerProvider)
        {
            _loggerHelper = loggerHelper;
            _sqlServerProvider = sqlServerProvider;
        }

        public async Task<bool> SendToAzureSql(Message queueItem, ILogger log)
        {

            var body = new StreamReader(queueItem.GetBody<Stream>(), Encoding.UTF8).ReadToEnd();

            var documentModel = JsonConvert.DeserializeObject<ChangeFeedMessageModel>(body);

            if (documentModel == null)
            {
                return false;
            }
            else
            {
                await SendToStoredProc(documentModel, log);

                return true;
            }
        }

        private async Task SendToStoredProc(ChangeFeedMessageModel documentModel, ILogger log)
        {
            _loggerHelper.LogMethodEnter(log);

            try
            {
                string resourceName = GetResourceName(documentModel);
                string CommandText = "Change_Feed_Insert_Update_" + resourceName;
                string ParameterName = "@Json";

                if (!string.IsNullOrWhiteSpace(resourceName))
                {
                    await _sqlServerProvider.UpsertResource(documentModel.Document, log, CommandText, ParameterName);
                }
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(log, Guid.NewGuid(), "Error when trying to insert & update change feed request into SQL", ex);
            }

            _loggerHelper.LogMethodExit(log);
                       
        }


        private string GetResourceName(ChangeFeedMessageModel documentModel)
        {
            if (documentModel.IsAction == true)
            {
                return "dss-actions";
            }
            else if (documentModel.IsActionPlan == true)
            {
                return "dss-actionplans";
            }
            else if (documentModel.IsAddress == true)
            {
                return "dss-addresses";
            }
            else if (documentModel.IsAdviserDetail == true)
            {
                return "dss-adviserdetails";
            }
            else if (documentModel.IsCustomer == true)
            {
                return "dss-customers";
            }
            else if (documentModel.IsGoal == true)
            {
                return "dss-goals";
            }
            else if (documentModel.IsOutcome == true)
            {
                return "dss-outcomes";
            }
            else if (documentModel.IsSession == true)
            {
                return "dss-sessions";
            }
            else
            {
                return "";
            }
        }

    }
}

