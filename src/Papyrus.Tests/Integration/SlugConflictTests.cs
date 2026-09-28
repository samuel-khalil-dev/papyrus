using Microsoft.EntityFrameworkCore;
using Papyrus.Application.Content;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;
using Papyrus.Infrastructure.Content;

namespace Papyrus.Tests.Integration;

/// <summary>
/// Covers the race-proof half of duplicate-slug handling against PostgreSQL.
/// <para>
/// The pre-check in the command stores is covered by the InMemory-backed
/// <c>AdminSlugConflictTests</c>, but that provider does not enforce unique
/// indexes and cannot observe a lost race. These cases drive two independent
/// contexts so the <c>23505</c> translation is actually exercised; without them
/// the backstop could be removed and every test would still pass.
/// </para>
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class SlugConflictTests(PostgresFixture fixture)
{
    [Fact]
    public async Task A_racing_duplicate_post_returns_the_conflict_error_instead_of_throwing()
    {
        // Pre-seed, then attempt a second insert with a context that has not
        // seen the row. The pre-check would catch this too, so the assertion
        // that matters is that no exception escapes and one row exists.
        var slug = $"race-post-{Guid.NewGuid():N}";
        await SeedAsync(db => db.BlogPosts.Add(Post(slug, "first")));

        await using var db = fixture.CreateDbContext();
        var store = new EfContentCommandStore(db);

        var result = await store.SaveBlogPostAsync(
            new(null, slug, "second", "Summary", "# Content", null, null, null, false, false, null, []),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => ContentValidation.IsSlugConflict(error));
    }

    [Fact]
    public async Task A_unique_violation_raised_by_the_database_is_translated_rather_than_propagated()
    {
        // The pre-check is a plain read, so a concurrent insert can always land
        // between it and the write. This reproduces that interleaving
        // deterministically: a row for the same slug is placed directly in the
        // change tracker, standing in for the insert that won the race, while
        // the store is called with a different, non-conflicting slug so its own
        // pre-check passes and the flush is what fails. Without the 23505
        // translation this surfaces as an unhandled DbUpdateException and a 500.
        var takenSlug = $"race-backstop-{Guid.NewGuid():N}";
        await SeedAsync(db => db.BlogPosts.Add(Post(takenSlug, "winner")));

        await using var db = fixture.CreateDbContext();
        var store = new EfContentCommandStore(db);
        db.BlogPosts.Add(Post(takenSlug, "the concurrent winner"));

        var result = await store.SaveBlogPostAsync(
            new(null, $"unrelated-{Guid.NewGuid():N}", "Unrelated", "Summary", "# Content", null, null, null, false, false, null, []),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => ContentValidation.IsSlugConflict(error));
    }

    [Fact]
    public async Task Resaving_a_post_under_its_own_slug_is_accepted_by_both_the_pre_check_and_the_index()
    {
        var slug = $"resave-{Guid.NewGuid():N}";
        await SeedAsync(db => db.BlogPosts.Add(Post(slug, "original")));

        await using var db = fixture.CreateDbContext();
        var store = new EfContentCommandStore(db);
        var existing = await db.BlogPosts.SingleAsync(post => post.Slug == slug);

        var result = await store.SaveBlogPostAsync(
            new(existing.Id, slug, "renamed title", "Summary", "# Content", null, null, null, false, false, null, []),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("renamed title", (await db.BlogPosts.SingleAsync(post => post.Id == existing.Id)).Title);
    }

    [Fact]
    public async Task A_soft_deleted_posts_slug_can_be_reused()
    {
        // The index is partial on "IsDeleted" = false, so the pre-check must not
        // be stricter than the index or a deleted slug would be permanently
        // unusable.
        var slug = $"recycled-{Guid.NewGuid():N}";
        Guid originalId;
        await using (var db = fixture.CreateDbContext())
        {
            db.BlogPosts.Add(Post(slug, "original"));
            await db.SaveChangesAsync();
            var row = await db.BlogPosts.SingleAsync(post => post.Slug == slug);
            originalId = row.Id;
            row.IsDeleted = true;
            await db.SaveChangesAsync();
        }

        await using var context = fixture.CreateDbContext();
        var store = new EfContentCommandStore(context);

        var result = await store.SaveBlogPostAsync(
            new(null, slug, "replacement", "Summary", "# Content", null, null, null, false, false, null, []),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(originalId, result.Value!.Id);
    }

    private static BlogPost Post(string slug, string title) => new()
    {
        Id = Guid.NewGuid(),
        Slug = slug,
        Title = title,
        Summary = "Slug conflict fixture",
        ContentMarkdown = "# Fixture",
        CreatedAt = DateTimeOffset.UtcNow,
        CreatedBy = "integration-test"
    };

    private async Task SeedAsync(Action<PapyrusDbContext> seed)
    {
        await using var db = fixture.CreateDbContext();
        seed(db);
        await db.SaveChangesAsync();
    }
}
