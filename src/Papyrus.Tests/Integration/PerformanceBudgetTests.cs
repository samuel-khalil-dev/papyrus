using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Papyrus.Application.Content;
using Papyrus.Infrastructure;
using Papyrus.Infrastructure.Content;
using Papyrus.Infrastructure.MultiTenancy;

namespace Papyrus.Tests.Integration;

[Collection(PostgresCollection.Name)]
public sealed class PerformanceBudgetTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Public_blog_feed_stays_within_the_four_query_budget()
    {
        var counter = new CountingCommandInterceptor();
        await using var db = fixture.CreateDbContext(null, counter);
        var handler = new PublicContentQueryHandlers(new EfPublicContentStore(db));

        var result = await handler.GetBlogIndexAsync(null, null, DateTimeOffset.UtcNow, 1, 20, CancellationToken.None);

        Assert.NotEmpty(result.Posts);
        Assert.InRange(counter.ReaderCommands, 1, 4);
        Assert.Equal(0, counter.NonQueryCommands);
        Assert.Equal(0, counter.ScalarCommands);
    }

    [Fact]
    public async Task Admin_collection_shape_stays_within_count_plus_page_budget()
    {
        var counter = new CountingCommandInterceptor();
        await using var db = fixture.CreateDbContext(null, counter);
        var query = db.BlogPosts.AsNoTracking()
            .OrderByDescending(post => post.PublishedAt ?? post.CreatedAt)
            .Select(post => new { post.Id, post.Slug, post.Title });

        var total = await query.CountAsync(CancellationToken.None);
        var page = await query.Skip(0).Take(20).ToArrayAsync(CancellationToken.None);

        Assert.NotEmpty(page);
        Assert.True(total >= page.Length);
        Assert.Equal(2, counter.ReaderCommands);
        Assert.Equal(0, counter.NonQueryCommands);
        Assert.Equal(0, counter.ScalarCommands);
    }
}

internal sealed class CountingCommandInterceptor : DbCommandInterceptor
{
    private int readerCommands;
    private int scalarCommands;
    private int nonQueryCommands;

    public int ReaderCommands => readerCommands;

    public int ScalarCommands => scalarCommands;

    public int NonQueryCommands => nonQueryCommands;

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref readerCommands);
        return ValueTask.FromResult(result);
    }

    public override ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref scalarCommands);
        return ValueTask.FromResult(result);
    }

    public override ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref nonQueryCommands);
        return ValueTask.FromResult(result);
    }
}
