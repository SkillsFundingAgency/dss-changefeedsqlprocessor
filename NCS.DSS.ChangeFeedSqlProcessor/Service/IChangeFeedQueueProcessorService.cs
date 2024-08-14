using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.ChangeFeedSqlProcessor.Service
{
    public interface IChangeFeedQueueProcessorService
    {
        Guid CorrelationId { get; set; }
        Task<bool> SendToAzureSql(string message, ILogger log);
    }

}