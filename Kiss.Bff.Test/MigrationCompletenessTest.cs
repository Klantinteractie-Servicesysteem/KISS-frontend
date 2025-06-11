
using System.Text.Json;
using Kiss.Bff.Beheer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace Kiss.Bff.Test
{
    [TestClass]
    public class MigrationCompletenessTest
    {
        // https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-6.0/breaking-changes#mitigations-17
        [TestMethod]
        public void TestIfWeForgotToAddAMigration()
        {
            using var context = new BeheerDbContext(new DbContextOptionsBuilder<BeheerDbContext>().UseNpgsql().Options);
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
