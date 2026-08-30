using Microsoft.EntityFrameworkCore;
using Papyrus.Api.Admin.AdminUi;
using Papyrus.Core.Content;

namespace Papyrus.Tests;

public sealed class TagResolutionTests
{
    [Fact]
    public async Task ResolveTags_reuses_existing_tags_case_insensitively_and_creates_new_tags()
    {
        await using var db = TestDb.Create();
        var existing = new ContentTag { Id = Guid.NewGuid(), Slug = "dotnet", Name = "Dotnet" };
        db.ContentTags.Add(existing);
        await db.SaveChangesAsync();

        var tags = await AdminContentSupport.ResolveTagsAsync(db, " DOTNET, Testing, dotnet ");

        Assert.Equal(2, tags.Count);
        Assert.Same(existing, tags[0]);
        Assert.Equal("testing", tags[1].Slug);
        Assert.Equal(2, db.ChangeTracker.Entries<ContentTag>().Count());
        Assert.Equal(1, await db.ContentTags.CountAsync());
    }

    [Fact]
    public async Task Set_tag_helpers_replace_relationships_for_posts_and_projects()
    {
        await using var db = TestDb.Create();
        var post = new BlogPost { Id = Guid.NewGuid() };
        var project = new Project { Id = Guid.NewGuid() };
        var first = new ContentTag { Id = Guid.NewGuid(), Slug = "one", Name = "One" };
        var second = new ContentTag { Id = Guid.NewGuid(), Slug = "two", Name = "Two" };
        db.AddRange(post, project, first, second);
        await db.SaveChangesAsync();

        await AdminContentSupport.SetBlogPostTagsAsync(db, post.Id, [first, second]);
        await AdminContentSupport.SetProjectTagsAsync(db, project.Id, [second]);
        await db.SaveChangesAsync();
        await AdminContentSupport.SetBlogPostTagsAsync(db, post.Id, [second]);
        await db.SaveChangesAsync();

        Assert.Equal([second.Id], await db.BlogPostTags.Select(x => x.TagId).ToListAsync());
        Assert.Equal([second.Id], await db.ProjectTags.Select(x => x.TagId).ToListAsync());
        var loaded = await db.BlogPostTags.Include(x => x.Tag).SingleAsync();
        Assert.Equal("Two", loaded.Tag.Name);
    }

    [Fact]
    public async Task ResolveTags_cancels_database_lookup_when_token_is_cancelled()
    {
        await using var db = TestDb.Create();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            AdminContentSupport.ResolveTagsAsync(db, "cancelled", cancellation.Token));
    }

    [Fact]
    public async Task SetBlogPostTags_cancels_relationship_lookup_when_token_is_cancelled()
    {
        await using var db = TestDb.Create();
        var post = new BlogPost { Id = Guid.NewGuid() };
        db.BlogPosts.Add(post);
        await db.SaveChangesAsync();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            AdminContentSupport.SetBlogPostTagsAsync(db, post.Id, [], cancellation.Token));
    }

    [Fact]
    public async Task SetProjectTags_cancels_relationship_lookup_when_token_is_cancelled()
    {
        await using var db = TestDb.Create();
        var project = new Project { Id = Guid.NewGuid() };
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            AdminContentSupport.SetProjectTagsAsync(db, project.Id, [], cancellation.Token));
    }
}
