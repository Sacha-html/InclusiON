using System.Data;

namespace InclusiON.ApplicationBusiness.Interfaces.Infrastructure
{
    public interface IConnectionFactory
    {
        IDbConnection CreateConnection();
        Task<IDbConnection> CreateConnectionAsync();
    }
}
