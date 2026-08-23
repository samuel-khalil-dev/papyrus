using Microsoft.EntityFrameworkCore;
using Papyrus.Core.Content;

namespace Papyrus.Tests.Integration;

[Collection(PostgresCollection.Name)]
public sealed class FilterAndProjectionTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Tenant_and_soft_delete_filters_apply_to_entities_and_relationships()
    {
        var id = Guid.NewGuid();
        await using (var db = fixture.CreateDbContext())
        {
            db.Projects.Add(new Project
            {
                Id = id,
                Slug = $"filter-depth-{id:N}",
                Title = "Deleted tenant fixture",
                Summary = "Should not be visible through the default filters.",
                IsDeleted = true,
                CreatedBy = "integration-test"
            });
            await db.SaveChangesAsync();
        }

        await using var currentTenant = fixture.CreateDbContext();
        await using var otherTenant = fixture.CreateDbContext("other-integration");

        Assert.Null(await currentTenant.Projects.AsNoTracking().SingleOrDefaultAsync(project => project.Id == id));
        Assert.Null(await otherTenant.Projects.AsNoTracking().SingleOrDefaultAsync(project => project.Id == id));
        Assert.NotNull(await currentTenant.Projects.IgnoreQueryFilters().AsNoTracking()
            .SingleAsync(project => project.Id == id && project.IsDeleted));
    }

    [Fact]
    public async Task Public_projection_is_no_tracking_and_uses_one_set_based_query()
    {
        await using var db = fixture.CreateDbContext();
        var query = db.Projects
            .AsNoTracking()
            .Where(project => project.IsPublished)
            .OrderBy(project => project.CreatedAt)
            .Select(project => new
            {
                project.Id,
                project.Title,
                Tags = project.ProjectTags
                    .OrderBy(link => link.Tag.Name)
                    .Select(link => link.Tag.Name)
                    .Take(20)
                    .ToArray()
            });

        var sql = query.ToQueryString();
        var projects = await query.Take(6).ToListAsync();

        Assert.NotEmpty(projects);
        Assert.Contains("SELECT", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ProjectTags", sql, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("N+1", sql, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(db.ChangeTracker.Entries());
    }
}
