using System;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using NCS.DSS.ChangeFeedSqlProcessor.Models;

namespace NCS.DSS.ChangeFeedSqlProcessor.Service
{
    public interface IChangeFeedQueueProcessorService
    {
        Guid CorrelationId { get; set; }
        Task<bool> SendToAzureSql(ChangeFeedMessageModel message, ILogger log);
        Task<bool> SendToAzureSql(string message, ILogger log);
    }

}