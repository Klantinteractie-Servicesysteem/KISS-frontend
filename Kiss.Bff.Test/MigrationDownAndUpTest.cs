using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class MigrationDownAndUpTest
    {
        // we use sqlite here because it supports both migrations and an in memory database
        public static WebApplicationFactory<Program> SqliteWebApplicationFactory { get; private set; } = new CustomWebApplicationFactory()
            .WithWebHostBuilder(b =>
            {
                b.ConfigureTestServices(s =>
                {
                    s.AddSingleton(_ =>
                    {
                        var connection = new SqliteConnection();
                        connection.Open();
                        return connection;
                    })
                    .AddSingleton(x => new DbContextOptionsBuilder<BeheerDbContext>()
                        .UseSqlite(x.GetRequiredService<SqliteConnection>())
                        // Since EF Core 9, PendingModelChangesWarning is thrown as an exception, which makes this test fail.
                        // The migrations and model snapshot are generated for postgres, but this test runs on sqlite.
                        // EF then compares the sqlite model to the postgres snapshot and reports changes that do not exist in postgres.
                        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                        .Options); ;
                });
            });

        [ClassCleanup]
        public static void Cleanup()
        {
            SqliteWebApplicationFactory?.Dispose();
        }

        [TestMethod]
        public async Task MigrateUpAndDown()
        {
            using var scope = SqliteWebApplicationFactory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BeheerDbContext>();
            var migrator = db.Database.GetInfrastructure().GetRequiredService<IMigrator>();
            await migrator.MigrateAsync("0");
            await migrator.MigrateAsync();
        }
    }
}
