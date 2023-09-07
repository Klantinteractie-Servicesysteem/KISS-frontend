using System.Text.Json;
using Kiss.Bff.Beheer.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class MigrationForgottenTest
    {
        // https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-6.0/breaking-changes#mitigations-17
        [TestMethod]
        public async Task TestIfWeForgotToAddAMigration()
        {
            // we don't use the CustomWebApplicationFactory, we need to use the Postgres version of EF Core
            // we're not actually connecting to a database so it's ok
            using var factory = new WebApplicationFactory<Program>();
            await using var scope = factory.Services.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<BeheerDbContext>();
            var migrationsAssembly = context.GetService<IMigrationsAssembly>();

            var snapshotModel = migrationsAssembly.ModelSnapshot?.Model;

            if (snapshotModel is IMutableModel mutableModel)
            {
                snapshotModel = mutableModel.FinalizeModel();
            }

            if (snapshotModel != null)
            {
                snapshotModel = context.GetService<IModelRuntimeInitializer>().Initialize(snapshotModel);
            }

            var differ = context.GetService<IMigrationsModelDiffer>();

            var differences = differ.GetDifferences(
                snapshotModel?.GetRelationalModel(),
                context.GetService<IDesignTimeModel>().Model.GetRelationalModel());

            Assert.AreEqual(0, differences.Count, JsonSerializer.Serialize(differences));
        }
    }
}
