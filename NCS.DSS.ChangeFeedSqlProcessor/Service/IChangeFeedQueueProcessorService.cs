using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace NCS.DSS.ChangeFeedSqlProcessor.Service
{
    public interface IChangeFeedQueueProcessorService
    {
        Guid CorrelationId { get; set; }
        Task<bool> SendToAzureSql(Message queueItem, ILogger log);

    }

}