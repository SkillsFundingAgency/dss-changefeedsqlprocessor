using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace NCS.DSS.ChangeFeedSqlProcessor.Service
{
    public interface IChangeFeedQueueProcessorService
    {
        Task<bool> SendToAzureSql(Message queueItem, ILogger log);

    }

}