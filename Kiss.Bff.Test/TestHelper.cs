global using Microsoft.VisualStudio.TestTools.UnitTesting;
using Kiss.Bff.Beheer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Kiss.Bff.Test
{
    public class TestHelper
    {
        // This method initializes the in-memory database with a unique database name using Guid.NewGuid().ToString().
        // The Initialize method now calls InitializeInMemoryDatabase to set up the database before each test.
        // this ensures that each test has its own isolated database, preventing conflicts between tests.
        protected DbContextOptions<BeheerDbContext> _dbContextOptions;

        public void InitializeDatabase()
        {
            _dbContextOptions = new DbContextOptionsBuilder<BeheerDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
        }
    }
}



