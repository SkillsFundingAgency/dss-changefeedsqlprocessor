using Microsoft.Extensions.Logging;

namespace NCS.DSS.ChangeFeedSqlProcessor.Service
{
    public interface IChangeFeedQueueProcessorService
    {
        Guid CorrelationId { get; set; }
        Task<bool> SendToAzureSql(string message);
    }

}