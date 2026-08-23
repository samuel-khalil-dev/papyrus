using Microsoft.EntityFrameworkCore;
using Npgsql;
using Papyrus.Core.Content;

namespace Papyrus.Tests.Integration;

[Collection(PostgresCollection.Name)]
public sealed class ConcurrencyTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Concurrent_duplicate_slug_writes_are_rejected_by_the_partial_unique_index()
    {
        var slug = $"duplicate-{Guid.NewGuid():N}";
        await using var first = fixture.CreateDbContext();
        await using var second = fixture.CreateDbContext();
        first.Projects.Add(CreateProject(slug, "first"));
        second.Projects.Add(CreateProject(slug, "second"));

        await first.SaveChangesAsync();
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => second.SaveChangesAsync());

        Assert.IsType<PostgresException>(exception.InnerException);
    }

    [Fact]
    public async Task Tenant_boundary_rejects_cross_tenant_updates_before_database_write()
    {
        var id = Guid.NewGuid();
        await using var db = fixture.CreateDbContext();
        var project = CreateProject($"boundary-{id:N}", "boundary");
        project.Id = id;
        await db.Projects.AddAsync(project);
        await db.SaveChangesAsync();

        project.TenantId = "other-integration";

        await Assert.ThrowsAsync<InvalidOperationException>(() => db.SaveChangesAsync());
    }

    private Project CreateProject(string slug, string title) => new()
    {
        Id = Guid.NewGuid(),
        Slug = slug,
        Title = title,
        Summary = "Concurrency fixture",
        CreatedBy = "integration-test"
    };
}
