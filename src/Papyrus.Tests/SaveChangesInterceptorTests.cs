using Microsoft.EntityFrameworkCore;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;
using Papyrus.Infrastructure.Interceptors;
using Papyrus.Infrastructure.MultiTenancy;

namespace Papyrus.Tests;

/// Covers the save interceptor's own contract. Soft delete and audit stamping are
/// otherwise only observed through the lifecycle handlers that call into them, so
/// a change to the interceptor's rules would be attributed to a caller and a
/// regression in the stamping would go unseen.
public sealed class SaveChangesInterceptorTests
{
    private static readonly DateTimeOffset Now = new(2026, 5, 6, 7, 8, 9, TimeSpan.Zero);

    [Fact]
    public async Task Deleting_a_post_converts_the_delete_into_a_soft_delete()
    {
        var (db, post) = await SeedAsync();
        await using (db)
        {
            post.IsDeleted = false;
            db.BlogPosts.Remove(post);

            await db.SaveChangesAsync();

            Assert.True(post.IsDeleted);
            Assert.Equal(Now, post.DeletedAt);
            Assert.Equal("tests.actor", post.DeletedBy);
        }
    }

    [Fact]
    public async Task Soft_delete_stamps_a_default_reason_when_the_caller_supplied_none()
    {
        var (db, post) = await SeedAsync();
        await using (db)
        {
            post.DeleteReason = null;
            db.BlogPosts.Remove(post);

            await db.SaveChangesAsync();

            Assert.Equal("Deleted by content command", post.DeleteReason);
        }
    }

    [Fact]
    public async Task Soft_delete_preserves_a_caller_supplied_reason()
    {
        // DeleteReason is assigned with ??=, so a reason carried by the caller has
        // to survive. This is the assertion most likely to catch a "simplification"
        // of that line into a plain assignment.
        var (db, post) = await SeedAsync();
        await using (db)
        {
            post.DeleteReason = "withdrawn at the author's request";
            db.BlogPosts.Remove(post);

            await db.SaveChangesAsync();

            Assert.Equal("withdrawn at the author's request", post.DeleteReason);
        }
    }

    [Fact]
    public async Task A_soft_deleted_row_is_still_stored_not_removed()
    {
        var (db, post) = await SeedAsync();
        var id = post.Id;
        await using (db)
        {
            db.BlogPosts.Remove(post);
            await db.SaveChangesAsync();

            // A second context reads with the global query filters applied, so the
            // row must be absent there while remaining present in the table.
            var visible = await db.BlogPosts.AsNoTracking().AnyAsync(candidate => candidate.Id == id);
            Assert.False(visible);

            db.ChangeTracker.Clear();
            var stored = await db.BlogPosts.IgnoreQueryFilters().AsNoTracking().SingleAsync(candidate => candidate.Id == id);
            Assert.True(stored.IsDeleted);
        }
    }

    [Fact]
    public async Task Adding_a_row_stamps_creation_audit_fields()
    {
        var db = Create();
        await using (db)
        {
            var post = new BlogPost
            {
                Id = Guid.NewGuid(),
                Slug = "created",
                Title = "Created",
                CreatedAt = DateTimeOffset.MinValue,
                CreatedBy = "unset",
            };
            db.BlogPosts.Add(post);

            await db.SaveChangesAsync();

            Assert.Equal(Now, post.CreatedAt);
            Assert.Equal("tests.actor", post.CreatedBy);
        }
    }

    [Fact]
    public async Task Modifying_a_row_stamps_update_audit_fields_without_rewriting_creation_fields()
    {
        var (db, post) = await SeedAsync();
        await using (db)
        {
            // Capture the values the insert established; the update must leave them.
            var createdAt = post.CreatedAt;
            var createdBy = post.CreatedBy;

            post.Title = "Renamed";
            db.BlogPosts.Attach(post);
            db.Entry(post).Property(candidate => candidate.Title).IsModified = true;

            await db.SaveChangesAsync();

            Assert.Equal(Now, post.UpdatedAt);
            Assert.Equal("tests.actor", post.UpdatedBy);
            Assert.Equal(createdAt, post.CreatedAt);
            Assert.Equal(createdBy, post.CreatedBy);
        }
    }

    [Fact]
    public async Task Audit_timestamps_come_from_the_injected_time_provider()
    {
        var db = Create(fixedNow: new DateTimeOffset(2031, 1, 1, 0, 0, 0, TimeSpan.Zero));
        await using (db)
        {
            var post = new BlogPost { Id = Guid.NewGuid(), Slug = "future", Title = "Future" };
            db.BlogPosts.Add(post);

            await db.SaveChangesAsync();

            Assert.Equal(new DateTimeOffset(2031, 1, 1, 0, 0, 0, TimeSpan.Zero), post.CreatedAt);
        }
    }

    private static PapyrusDbContext Create(DateTimeOffset? fixedNow = null) =>
        new(
            new DbContextOptionsBuilder<PapyrusDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddInterceptors(new PapyrusSaveChangesInterceptor(
                    new FixedActor("tests.actor"),
                    new FixedTimeProvider(fixedNow ?? Now)))
                .Options,
            new TenantContext("main"));

    private static async Task<(PapyrusDbContext Db, BlogPost Post)> SeedAsync()
    {
        var db = Create();
        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = "seeded",
            Title = "Seeded",
            ContentMarkdown = "Body",
            CreatedBy = "seed",
        };
        db.BlogPosts.Add(post);
        await db.SaveChangesAsync();
        return (db, post);
    }

    private sealed class FixedActor(string actorId) : IAuditActor
    {
        public string ActorId { get; } = actorId;
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
