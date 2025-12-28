using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using Microsoft.Data.SqlClient;
using System.Data;

namespace InclusiON.Infrastructure.Data.Repositories.Base
{
    public abstract class BaseRepository
    {
        protected readonly IUnitOfWork _unitOfWork;

        protected BaseRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        protected async Task<IDbConnection> GetConnectionAsync()
        {
            return await _unitOfWork.GetConnectionAsync();
        }

        protected IDbTransaction? GetCurrentTransaction()
        {
            return _unitOfWork.GetCurrentTransaction();
        }

        protected IDbCommand CreateCommand(IDbConnection connection, string query)
        {
            var command = connection.CreateCommand();
            command.CommandText = query;

            var transaction = GetCurrentTransaction();

            if (transaction is not null)
            {
                command.Transaction = transaction;
            }

            return command;
        }

        protected SqlCommand CreateSqlCommand(IDbConnection connection, string query)
        {
            if (connection is not SqlConnection sqlConnection)
            {
                throw new InvalidOperationException("Expected Sql Connection");
            }

            var command = new SqlCommand(query, sqlConnection);

            var transaction = GetCurrentTransaction();
            if (transaction is SqlTransaction sqlTransaction)
            {
                command.Transaction = sqlTransaction;
            }

            return command;

        }


        protected static void AddParameter(IDbCommand command, string name, object? value)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            command.Parameters.Add(param);
        }
    }
}
