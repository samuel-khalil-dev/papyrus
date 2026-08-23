using Microsoft.EntityFrameworkCore;
using Npgsql;
using Papyrus.Core.Content;

namespace Papyrus.Tests.Integration;

[Collection(PostgresCollection.Name)]
public sealed class PostgresTest(PostgresFixture fixture)
{
    [Fact]
    public async Task Uses_postgres_provider_and_applies_real_migrations()
    {
        await using var db = fixture.CreateDbContext();

        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", db.Database.ProviderName);
        Assert.True(await db.Database.CanConnectAsync());
        Assert.Contains("20260826005202_SyncSearchTrigramIndexesModel", await db.Database.GetAppliedMigrationsAsync());
    }

    [Fact]
    public async Task Global_filters_exclude_soft_deleted_rows_and_isolate_tenants()
    {
        var id = Guid.NewGuid();
        var slug = $"filter-{id:N}";
        await using (var writer = fixture.CreateDbContext())
        {
            writer.Projects.Add(new Project
            {
                Id = id,
                Slug = slug,
                Title = "Filtered project",
                Summary = "Filter test",
                IsDeleted = true,
                CreatedBy = "integration-test"
            });
            await writer.SaveChangesAsync();
        }

        await using var tenantDb = fixture.CreateDbContext();
        Assert.Empty(await tenantDb.Projects.AsNoTracking().Where(project => project.Slug == slug).ToListAsync());

        await using var otherTenantDb = fixture.CreateDbContext("other-integration");
        Assert.Empty(await otherTenantDb.Projects.AsNoTracking().Where(project => project.Slug == slug).ToListAsync());
    }

    [Fact]
    public async Task Transaction_rollback_is_atomic()
    {
        var id = Guid.NewGuid();
        await using var db = fixture.CreateDbContext();
        await using var transaction = await db.Database.BeginTransactionAsync();
        db.Projects.Add(new Project
        {
            Id = id,
            Slug = $"rollback-{id:N}",
            Title = "Rolled back project",
            Summary = "Transaction test",
            CreatedBy = "integration-test"
        });
        await db.SaveChangesAsync();
        await transaction.RollbackAsync();

        Assert.Null(await db.Projects.AsNoTracking().SingleOrDefaultAsync(project => project.Id == id));
    }

    [Fact]
    public async Task Optimistic_concurrency_rejects_stale_updates()
    {
        var id = Guid.NewGuid();
        var slug = $"concurrency-{id:N}";
        await using (var seed = fixture.CreateDbContext())
        {
            seed.Projects.Add(new Project { Id = id, Slug = slug, Title = "Original", Summary = "Concurrency test", CreatedBy = "integration-test" });
            await seed.SaveChangesAsync();
        }

        await using var first = fixture.CreateDbContext();
        await using var second = fixture.CreateDbContext();
        var firstProject = await first.Projects.SingleAsync(project => project.Id == id);
        var secondProject = await second.Projects.SingleAsync(project => project.Id == id);
        firstProject.Title = "First update";
        firstProject.ConcurrencyToken = Guid.NewGuid();
        await first.SaveChangesAsync();
        secondProject.Title = "Stale update";

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => second.SaveChangesAsync());
    }

    [Fact]
    public async Task PostgreSQL_indexes_cover_tenant_slug_and_soft_delete_filter()
    {
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT indexname, indexdef FROM pg_indexes WHERE schemaname = 'public' AND tablename = 'Projects'",
            connection);
        await using var reader = await command.ExecuteReaderAsync();
        var definitions = new List<string>();
        while (await reader.ReadAsync())
        {
            definitions.Add($"{reader.GetString(0)} {reader.GetString(1)}");
        }

        Assert.Contains(definitions, definition => definition.Contains("TenantId", StringComparison.Ordinal) && definition.Contains("Slug", StringComparison.Ordinal));
        Assert.Contains(definitions, definition => definition.Contains("IsDeleted", StringComparison.Ordinal) && definition.Contains("TenantId", StringComparison.Ordinal));
    }

    [Fact]
    public async Task No_tracking_projection_loads_related_data_without_n_plus_one_shape()
    {
        await using var db = fixture.CreateDbContext();

        var projects = await db.Projects
            .AsNoTracking()
            .Where(project => project.TenantId == fixture.TenantId)
            .Select(project => new { project.Id, project.Title, TagCount = project.ProjectTags.Count })
            .ToListAsync();

        Assert.NotEmpty(projects);
        var sql = db.Projects
            .AsNoTracking()
            .Where(project => project.TenantId == fixture.TenantId)
            .Select(project => new { project.Id, project.Title, TagCount = project.ProjectTags.Count })
            .ToQueryString();
        Assert.Contains("SELECT", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ProjectTags", sql, StringComparison.OrdinalIgnoreCase);
    }
}
