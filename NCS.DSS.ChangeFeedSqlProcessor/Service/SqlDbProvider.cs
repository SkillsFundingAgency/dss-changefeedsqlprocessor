using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Data;


namespace NCS.DSS.ChangeFeedSqlProcessor.Service
{
    public class SqlDbProvider : ISqlDbProvider
    {
        private readonly ILogger<SqlDbProvider> _logger;

        private SqlConnection _dbConnection;

        private readonly string _sqlConnString = Environment.GetEnvironmentVariable("SQLConnString");

        private readonly Guid _correlationId = Guid.NewGuid();

        public SqlDbProvider(ILogger<SqlDbProvider> logger)
        {
            _logger = logger;
        }

        public async Task<bool> UpsertResource(string entity,string commandText, string parameterName)
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
                _logger.LogError(exception,"{CorrelationId} Failed to Execute SQL Command for {document}", _correlationId,entity);
                throw;
            }
        }

        private void Execute(string document, string commandText, string parameterName)
        {
            using (_dbConnection = new SqlConnection(_sqlConnString))
            {
                using SqlCommand dbCommand = BuildCommand(commandText);
                try
                {
                    _dbConnection.Open();
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
                    _dbConnection.Close();
                }
            }
        }

        private SqlParameter BuildParameter(SqlCommand command, string document, string parameterName)
        {
            SqlParameter dbDataParameter = command.CreateParameter();
            dbDataParameter.ParameterName = parameterName;
            dbDataParameter.Direction = ParameterDirection.Input;
            dbDataParameter.Value = document;
            return dbDataParameter;
        }

        private SqlCommand BuildCommand(string commandText)
        {
            SqlCommand dbCommand = _dbConnection.CreateCommand();
            dbCommand.CommandType = CommandType.StoredProcedure;
            dbCommand.CommandText = commandText;
            return dbCommand;
        }
    }
}
