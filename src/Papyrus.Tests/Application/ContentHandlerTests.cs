using Microsoft.EntityFrameworkCore;
using Papyrus.Application.Content;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;
using Papyrus.Infrastructure.Content;
using Papyrus.Infrastructure.MultiTenancy;

namespace Papyrus.Tests.Application;

public sealed class ContentCommandHandlerTests
{
    [Fact]
    public async Task Create_project_persists_the_requested_content_and_tags()
    {
        await using var db = TestDb.Create();
        var tag = new ContentTag { Id = Guid.NewGuid(), Slug = "dotnet", Name = ".NET" };
        db.ContentTags.Add(tag);
        await db.SaveChangesAsync();
        var command = new SaveProjectCommand(null, "papyrus", "Papyrus", "A platform", "Details", null, null, null,
            null, true, true, DateTimeOffset.UtcNow, [tag.Id]);

        var result = await new ContentCommandHandlers(new EfContentCommandStore(db)).SaveProjectAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("papyrus", result.Value!.Slug);
        Assert.Equal(db.TenantId, result.Value.TenantId);
        Assert.Equal(tag.Id, await db.ProjectTags.Select(x => x.TagId).SingleAsync());
    }

    [Fact]
    public async Task Update_project_replaces_tags_and_updates_fields()
    {
        await using var db = TestDb.Create();
        var first = new ContentTag { Id = Guid.NewGuid(), Slug = "first", Name = "First" };
        var second = new ContentTag { Id = Guid.NewGuid(), Slug = "second", Name = "Second" };
        db.ContentTags.AddRange(first, second);
        var project = new Project { Id = Guid.NewGuid(), Slug = "old", Title = "Old", Summary = "Old summary" };
        db.Projects.Add(project);
        db.ProjectTags.Add(new ProjectTag { ProjectId = project.Id, TagId = first.Id });
        await db.SaveChangesAsync();
        var command = new SaveProjectCommand(project.Id, "new", "New", "New summary", null, null, null, null, null,
            false, false, null, [second.Id]);

        var result = await new ContentCommandHandlers(new EfContentCommandStore(db)).SaveProjectAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("New", result.Value!.Title);
        Assert.Equal([second.Id], await db.ProjectTags.Select(x => x.TagId).ToListAsync());
    }

    [Fact]
    public async Task Invalid_create_does_not_write_content()
    {
        await using var db = TestDb.Create();

        var result = await new ContentCommandHandlers(new EfContentCommandStore(db)).SaveProjectAsync(
            new SaveProjectCommand(null, "", "", "", null, null, null, null, null, false, false, null, []),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.Code == "slug.invalid");
        Assert.Empty(await db.Projects.ToListAsync());
    }

    [Fact]
    public async Task Create_update_and_missing_blog_post_paths_are_handled()
    {
        await using var db = TestDb.Create();
        var handler = new ContentCommandHandlers(new EfContentCommandStore(db));
        var create = await handler.SaveBlogPostAsync(PostCommand(null, "first", "First"), CancellationToken.None);
        var update = await handler.SaveBlogPostAsync(PostCommand(create.Value!.Id, "updated", "Updated"), CancellationToken.None);
        var missing = await handler.SaveBlogPostAsync(PostCommand(Guid.NewGuid(), "missing", "Missing"), CancellationToken.None);

        Assert.True(create.IsSuccess);
        Assert.True(update.IsSuccess);
        Assert.Equal("updated", (await db.BlogPosts.SingleAsync()).Slug);
        Assert.False(missing.IsSuccess);
        Assert.Contains(missing.Errors, error => error.Code == "post.not_found");
    }

    private static SaveBlogPostCommand PostCommand(Guid? id, string slug, string title) =>
        new(id, slug, title, "Summary", "# Content", null, null, null, false, true, DateTimeOffset.UtcNow, []);
}

public sealed class ContentQueryHandlerTests
{
    [Fact]
    public async Task Content_lists_are_projected_and_ordered_by_publish_date()
    {
        await using var db = TestDb.Create();
        db.Projects.AddRange(
            new Project { Id = Guid.NewGuid(), Slug = "older", Title = "Older", Summary = "Summary", PublishedAt = DateTimeOffset.UtcNow.AddDays(-1) },
            new Project { Id = Guid.NewGuid(), Slug = "newer", Title = "Newer", Summary = "Summary", PublishedAt = DateTimeOffset.UtcNow });
        db.BlogPosts.Add(new BlogPost { Id = Guid.NewGuid(), Slug = "post", Title = "Post", Summary = "Summary", ContentMarkdown = "Content" });
        await db.SaveChangesAsync();

        var handler = new ContentQueryHandlers(new EfContentQueryStore(db));
        var projects = await handler.GetProjectsAsync(CancellationToken.None);
        var posts = await handler.GetBlogPostsAsync(CancellationToken.None);

        Assert.Equal(["newer", "older"], projects.Select(x => x.Slug).ToArray());
        Assert.Single(posts);
        Assert.Equal("post", posts[0].Slug);
    }
}

public sealed class PublicContentQueryHandlerTests
{
    [Fact]
    public async Task Public_feed_search_excludes_drafts_and_future_posts_and_bounds_results()
    {
        await using var db = TestDb.Create();
        var now = DateTimeOffset.UtcNow;
        db.BlogPosts.AddRange(
            new BlogPost { Id = Guid.NewGuid(), Slug = "live", Title = "Security", Summary = "Search", ContentMarkdown = "Content", IsPublished = true, PublishedAt = now.AddMinutes(-1) },
            new BlogPost { Id = Guid.NewGuid(), Slug = "draft", Title = "Security", Summary = "Search", ContentMarkdown = "Content" },
            new BlogPost { Id = Guid.NewGuid(), Slug = "future", Title = "Security", Summary = "Search", ContentMarkdown = "Content", IsPublished = true, PublishedAt = now.AddDays(1) });
        await db.SaveChangesAsync();

        var result = await new PublicContentQueryHandlers(new EfPublicContentStore(db)).GetBlogIndexAsync("security", null, now, 1, 1, CancellationToken.None);

        Assert.Single(result.Posts);
        Assert.Equal("live", result.Posts[0].Slug);
        Assert.Equal(1, result.TotalPosts);
    }

    [Fact]
    public async Task Public_detail_returns_only_live_matching_content()
    {
        await using var db = TestDb.Create();
        db.Projects.Add(new Project { Id = Guid.NewGuid(), Slug = "live", Title = "Live", Summary = "Summary", IsPublished = true, PublishedAt = DateTimeOffset.UtcNow });
        db.Projects.Add(new Project { Id = Guid.NewGuid(), Slug = "draft", Title = "Draft", Summary = "Summary" });
        await db.SaveChangesAsync();
        var handler = new PublicContentQueryHandlers(new EfPublicContentStore(db));

        var live = await handler.GetProjectPageAsync("live", DateTimeOffset.UtcNow, CancellationToken.None);
        var draft = await handler.GetProjectPageAsync("draft", DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.NotNull(live);
        Assert.Equal("live", live!.Project.Slug);
        Assert.Null(draft);
    }
}

public sealed class TaxonomyAndLifecycleHandlerTests
{
    [Fact]
    public async Task Taxonomy_handlers_create_and_validate_category_and_tag()
    {
        await using var db = TestDb.Create();
        var handlers = new TaxonomyCommandHandlers(new EfTaxonomyCommandStore(db));

        var category = await handlers.SaveCategoryAsync(new SaveCategoryCommand(null, "engineering", "Engineering", null, true), CancellationToken.None);
        var tag = await handlers.SaveTagAsync(new SaveTagCommand(null, "dotnet", ".NET"), CancellationToken.None);
        var invalid = await handlers.SaveTagAsync(new SaveTagCommand(null, "", ""), CancellationToken.None);

        Assert.True(category.IsSuccess);
        Assert.True(tag.IsSuccess);
        Assert.False(invalid.IsSuccess);
        Assert.Equal(1, await db.ContentCategories.CountAsync());
        Assert.Equal(1, await db.ContentTags.CountAsync());
    }

    [Fact]
    public async Task Taxonomy_handlers_update_category_and_tag_in_the_current_tenant()
    {
        await using var db = TestDb.Create();
        var category = new ContentCategory { Id = Guid.NewGuid(), Slug = "old-category", Name = "Old category" };
        var tag = new ContentTag { Id = Guid.NewGuid(), Slug = "old-tag", Name = "Old tag" };
        db.AddRange(category, tag);
        await db.SaveChangesAsync();
        var handlers = new TaxonomyCommandHandlers(new EfTaxonomyCommandStore(db));

        var updatedCategory = await handlers.SaveCategoryAsync(
            new SaveCategoryCommand(category.Id, "new-category", "New category", "Description", true), CancellationToken.None);
        var updatedTag = await handlers.SaveTagAsync(
            new SaveTagCommand(tag.Id, "new-tag", "New tag"), CancellationToken.None);

        Assert.True(updatedCategory.IsSuccess);
        Assert.True(updatedTag.IsSuccess);
        Assert.Equal("new-category", (await db.ContentCategories.SingleAsync()).Slug);
        Assert.Equal("New tag", (await db.ContentTags.SingleAsync()).Name);
        Assert.Equal(db.TenantId, updatedCategory.Value!.TenantId);
        Assert.Equal(db.TenantId, updatedTag.Value!.TenantId);
    }

    [Theory]
    [InlineData("", "Valid name")]
    [InlineData("valid-slug", "")]
    public async Task Taxonomy_handlers_reject_invalid_category_values(string slug, string name)
    {
        await using var db = TestDb.Create();
        var result = await new TaxonomyCommandHandlers(new EfTaxonomyCommandStore(db)).SaveCategoryAsync(
            new SaveCategoryCommand(null, slug, name, null, false), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => error.Code == "category.invalid");
        Assert.Empty(await db.ContentCategories.ToListAsync());
    }

    [Fact]
    public async Task Lifecycle_handlers_soft_delete_and_restore_project_and_post()
    {
        await using var db = TestDb.Create();
        var project = new Project { Id = Guid.NewGuid(), Slug = "project", Title = "Project", Summary = "Summary" };
        var post = new BlogPost { Id = Guid.NewGuid(), Slug = "post", Title = "Post", Summary = "Summary", ContentMarkdown = "Content" };
        db.AddRange(project, post);
        await db.SaveChangesAsync();
        var handlers = new ContentLifecycleHandlers(new EfContentLifecycleStore(db, TimeProvider.System));

        Assert.True(await handlers.SoftDeleteProjectAsync(project.Id, "retired", CancellationToken.None));
        Assert.True(await handlers.SoftDeleteBlogPostAsync(post.Id, "retired", CancellationToken.None));
        Assert.Empty(await db.Projects.ToListAsync());
        Assert.Empty(await db.BlogPosts.ToListAsync());
        Assert.True(await handlers.RestoreProjectAsync(project.Id, CancellationToken.None));
        Assert.True(await handlers.RestoreBlogPostAsync(post.Id, CancellationToken.None));
        Assert.Single(await db.Projects.ToListAsync());
        Assert.Single(await db.BlogPosts.ToListAsync());
        Assert.False(await handlers.RestoreProjectAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task Lifecycle_handlers_ignore_cross_tenant_ids_without_tracking_or_modifying_them()
    {
        var databaseName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<PapyrusDbContext>().UseInMemoryDatabase(databaseName).Options;
        var projectId = Guid.NewGuid();

        await using (var otherDb = new PapyrusDbContext(options, new TenantContext("other")))
        {
            otherDb.Projects.Add(new Project { Id = projectId, Slug = "other", Title = "Other", Summary = "Other" });
            await otherDb.SaveChangesAsync();
        }

        await using var mainDb = new PapyrusDbContext(options, new TenantContext("main"));
        var deleted = await new ContentLifecycleHandlers(new EfContentLifecycleStore(mainDb, TimeProvider.System)).SoftDeleteProjectAsync(projectId, "attack", CancellationToken.None);

        Assert.False(deleted);
        Assert.Empty(mainDb.ChangeTracker.Entries<Project>());
        await using var verificationDb = new PapyrusDbContext(options, new TenantContext("other"));
        var project = await verificationDb.Projects.IgnoreQueryFilters().SingleAsync();
        Assert.False(project.IsDeleted);
        Assert.Equal("other", project.TenantId);
    }
}
