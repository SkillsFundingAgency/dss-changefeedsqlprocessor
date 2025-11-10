using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Data;


namespace NCS.DSS.ChangeFeedSqlProcessor.Service
{
    public class SqlDbProvider : ISqlDbProvider
    {
        private readonly ILogger<SqlDbProvider> _logger;

        private readonly string _sqlConnString = Environment.GetEnvironmentVariable("SQLConnString");

        private readonly Guid _correlationId = Guid.NewGuid();

        public SqlDbProvider(ILogger<SqlDbProvider> logger)
        {
            _logger = logger;
        }

        public async Task<bool> UpsertResource(string entity, string commandText, string parameterName)
        {
            try
            {
                await Task.Run(delegate
                {
                    Execute(entity, commandText, parameterName);
                });
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "{CorrelationId} Failed to Execute SQL Command for {document}", _correlationId, entity);
                throw;
            }
        }

        private void Execute(string document, string commandText, string parameterName)
        {
            using (var dbconn = new SqlConnection(_sqlConnString))
            using (var dbCommand = BuildCommand(commandText, dbconn))
            {
                try
                {
                    dbconn.Open();
                    dbCommand.Parameters.Add(BuildParameter(dbCommand, document, parameterName));
                    dbCommand.ExecuteNonQuery();
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "{CorrelationId} Failed to Execute SQL Command for {document}", _correlationId, document);
                    throw;
                }
                finally
                {
                    dbconn.Close();
                }
            }
        }

        private SqlParameter BuildParameter(SqlCommand command, string document, string parameterName)
        {
            var dbDataParameter = command.CreateParameter();
            dbDataParameter.ParameterName = parameterName;
            dbDataParameter.Direction = ParameterDirection.Input;
            dbDataParameter.Value = document;
            return dbDataParameter;
        }

        private SqlCommand BuildCommand(string commandText, SqlConnection dbconn)
        {
            var dbCommand = dbconn.CreateCommand();
            dbCommand.CommandType = CommandType.StoredProcedure;
            dbCommand.CommandText = commandText;
            return dbCommand;
        }
    }
}
