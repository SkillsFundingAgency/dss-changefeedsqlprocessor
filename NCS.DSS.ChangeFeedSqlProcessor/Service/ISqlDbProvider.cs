using Microsoft.Extensions.Logging;

namespace NCS.DSS.ChangeFeedSqlProcessor.Service
{
    public interface ISqlDbProvider
    {
        Task<bool> UpsertResource(string entity, string commandText, string parameterName);
    }
}
