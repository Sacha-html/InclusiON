using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using InclusiON.Data;

namespace InclusiON.Tests.TestSupport
{
    /// <summary>
    /// Clase base para tests que requieren un <see cref="AppDbContext"/> en memoria.
    /// Cada instancia de test obtiene un context con una base unica (aislamiento total entre tests).
    /// xUnit construye una instancia nueva por cada <c>[Fact]</c>, por lo que el
    /// <c>Db</c> se crea fresco para cada test y se descarta al final.
    /// </summary>
    public abstract class DbContextTestBase : IDisposable
    {
        /// <summary>
        /// Instancia del <see cref="AppDbContext"/> en memoria para el test actual.
        /// </summary>
        protected AppDbContext Db { get; }

        protected DbContextTestBase()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            Db = new AppDbContext(options);
        }

        public void Dispose()
        {
            Db.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
