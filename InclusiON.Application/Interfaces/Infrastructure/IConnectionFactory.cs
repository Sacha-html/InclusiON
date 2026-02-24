using System.Data;

namespace InclusiON.Application.Interfaces.Infrastructure
{
    public interface IConnectionFactory
    {
        IDbConnection CreateConnection();
        Task<IDbConnection> CreateConnectionAsync();
    }
}
