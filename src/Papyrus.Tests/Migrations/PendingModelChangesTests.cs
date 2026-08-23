using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Papyrus.Infrastructure;

namespace Papyrus.Tests.Migrations;

public sealed class PendingModelChangesTests
{
    [Fact]
    public void Ef_model_has_no_pending_migration_changes()
    {
        // This is the same model-diff check used by the EF CLI gate, but runs as
        // part of the solution test suite so drift fails CI before deployment.
        using var db = CreatePostgresModelContext();

        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public void Migration_snapshot_is_present_and_create_script_contains_all_owned_tables()
    {
        using var db = CreatePostgresModelContext();
        var migrations = db.GetService<IMigrationsAssembly>();
        var snapshot = migrations.ModelSnapshot;
        var latestMigration = migrations.Migrations.Keys.OrderBy(id => id, StringComparer.Ordinal).Last();
        var createScript = db.Database.GenerateCreateScript();

        Assert.NotNull(snapshot);
        Assert.Contains(latestMigration, migrations.Migrations.Keys);
        Assert.Contains("CREATE TABLE", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Projects", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("BlogPosts", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ContentCategories", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ContentTags", createScript, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(typeof(PapyrusDbContext).Assembly, snapshot!.GetType().Assembly);
    }

    private static PapyrusDbContext CreatePostgresModelContext()
    {
        var options = new DbContextOptionsBuilder<PapyrusDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=papyrus_test;Username=postgres;Password=postgres")
            .Options;
        return new PapyrusDbContext(options);
    }
}
