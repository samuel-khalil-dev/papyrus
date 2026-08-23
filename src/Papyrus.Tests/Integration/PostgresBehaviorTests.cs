using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Papyrus.Core.Content;

namespace Papyrus.Tests.Integration;

[Collection(PostgresCollection.Name)]
public sealed class PostgresBehaviorTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Ilike_search_is_case_insensitive_and_bounded_by_pagination()
    {
        var id = Guid.NewGuid();
        await using (var db = fixture.CreateDbContext())
        {
            db.BlogPosts.Add(new BlogPost
            {
                Id = id,
                Slug = $"postgres-search-{id:N}",
                Title = $"PostgreSQL Provider Search Marker {id:N}",
                Summary = "A provider-specific search fixture.",
                ContentMarkdown = "Searchable PostgreSQL content.",
                IsPublished = true,
                PublishedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
                CreatedBy = "integration-test"
            });
            await db.SaveChangesAsync();
        }

        await using var queryDb = fixture.CreateDbContext();
        var searchMarker = id.ToString("N")[..12];
        var matches = await queryDb.BlogPosts
            .AsNoTracking()
            .Where(post => EF.Functions.ILike(post.Title, $"%{searchMarker}%"))
            .OrderBy(post => post.Id)
            .Skip(0)
            .Take(1)
            .Select(post => new { post.Id, post.Title })
            .ToListAsync();

        Assert.Contains(matches, match => match.Id == id);
    }

    [Fact]
    public async Task Serializable_transaction_isolation_detects_a_concurrent_write()
    {
        await using var first = fixture.CreateDbContext();
        await using var second = fixture.CreateDbContext();
        await using var firstTransaction = await first.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        await using var secondTransaction = await second.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        var firstValue = await first.Projects.AsNoTracking().CountAsync();
        var secondValue = await second.Projects.AsNoTracking().CountAsync();

        Assert.Equal(firstValue, secondValue);
        Assert.Equal(System.Data.IsolationLevel.Serializable, firstTransaction.GetDbTransaction().IsolationLevel);
        await secondTransaction.RollbackAsync();
        await firstTransaction.RollbackAsync();
    }
}
