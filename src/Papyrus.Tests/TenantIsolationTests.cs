using Microsoft.EntityFrameworkCore;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;
using Papyrus.Infrastructure.MultiTenancy;

namespace Papyrus.Tests;

public sealed class TenantIsolationTests
{
    [Fact]
    public async Task Every_tenant_entity_enumeration_is_scoped_to_the_current_tenant()
    {
        var databaseName = Guid.NewGuid().ToString();
        var mainIds = await SeedAsync(databaseName, "main", "main");
        var otherIds = await SeedAsync(databaseName, "other", "other");

        await using var main = Create(databaseName, "main");
        Assert.Equal(mainIds.CategoryId, (await main.ContentCategories.SingleAsync()).Id);
        Assert.Equal(mainIds.TagId, (await main.ContentTags.SingleAsync()).Id);
        Assert.Equal(mainIds.ProjectId, (await main.Projects.SingleAsync()).Id);
        Assert.Equal(mainIds.PostId, (await main.BlogPosts.SingleAsync()).Id);
        Assert.Equal(mainIds.ProjectId, (await main.ProjectTags.SingleAsync()).ProjectId);
        Assert.Equal(mainIds.PostId, (await main.BlogPostTags.SingleAsync()).BlogPostId);

        await using var other = Create(databaseName, "other");
        Assert.Equal(otherIds.CategoryId, (await other.ContentCategories.SingleAsync()).Id);
        Assert.Equal(otherIds.TagId, (await other.ContentTags.SingleAsync()).Id);
        Assert.Equal(otherIds.ProjectId, (await other.Projects.SingleAsync()).Id);
        Assert.Equal(otherIds.PostId, (await other.BlogPosts.SingleAsync()).Id);
        Assert.Equal(otherIds.ProjectId, (await other.ProjectTags.SingleAsync()).ProjectId);
        Assert.Equal(otherIds.PostId, (await other.BlogPostTags.SingleAsync()).BlogPostId);
    }

    [Fact]
    public async Task Published_feed_and_sitemap_source_queries_cannot_include_another_tenant()
    {
        var databaseName = Guid.NewGuid().ToString();
        await SeedAsync(databaseName, "main", "main");
        await SeedAsync(databaseName, "other", "other");

        await using var main = Create(databaseName, "main");
        var mainFeed = await main.BlogPosts.AsNoTracking()
            .Where(post => post.IsPublished && post.PublishedAt <= DateTimeOffset.UtcNow)
            .Select(post => post.Slug)
            .ToListAsync();
        var mainSitemap = await main.Projects.AsNoTracking()
            .Where(project => project.IsPublished && project.PublishedAt <= DateTimeOffset.UtcNow)
            .Select(project => project.Slug)
            .ToListAsync();

        Assert.Equal(["main-post"], mainFeed);
        Assert.Equal(["main-project"], mainSitemap);
    }

    [Fact]
    public async Task A_tenant_cannot_mutate_another_tenants_entity_even_when_attached_by_id()
    {
        var databaseName = Guid.NewGuid().ToString();
        var mainIds = await SeedAsync(databaseName, "main", "main");

        await using var other = Create(databaseName, "other");
        var crossTenantProject = new Project { Id = mainIds.ProjectId, TenantId = "main", Title = "tampered" };
        other.Projects.Attach(crossTenantProject);

        await Assert.ThrowsAsync<InvalidOperationException>(() => other.SaveChangesAsync());
        await using var verification = Create(databaseName, "main");
        Assert.Equal("main project", (await verification.Projects.SingleAsync()).Title);
    }

    private static PapyrusDbContext Create(string databaseName, string tenantId) =>
        new(new DbContextOptionsBuilder<PapyrusDbContext>().UseInMemoryDatabase(databaseName).Options, new TenantContext(tenantId));

    private static async Task<SeedIds> SeedAsync(string databaseName, string tenantId, string prefix)
    {
        var categoryId = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        await using var db = Create(databaseName, tenantId);
        var now = DateTimeOffset.UtcNow.AddMinutes(-1);
        db.ContentCategories.Add(new ContentCategory { Id = categoryId, Slug = $"{prefix}-category", Name = prefix });
        db.ContentTags.Add(new ContentTag { Id = tagId, Slug = $"{prefix}-tag", Name = prefix });
        db.Projects.Add(new Project { Id = projectId, Slug = $"{prefix}-project", Title = $"{prefix} project", Summary = prefix, IsPublished = true, PublishedAt = now, CreatedAt = now });
        db.BlogPosts.Add(new BlogPost { Id = postId, Slug = $"{prefix}-post", Title = prefix, Summary = prefix, ContentMarkdown = prefix, IsPublished = true, PublishedAt = now, CreatedAt = now });
        db.ProjectTags.Add(new ProjectTag { ProjectId = projectId, TagId = tagId });
        db.BlogPostTags.Add(new BlogPostTag { BlogPostId = postId, TagId = tagId });
        await db.SaveChangesAsync();
        return new SeedIds(categoryId, tagId, projectId, postId);
    }

    private sealed record SeedIds(Guid CategoryId, Guid TagId, Guid ProjectId, Guid PostId);
}
