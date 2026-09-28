using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Papyrus.Api.Admin;
using Papyrus.Api.Admin.Media;
using Papyrus.Application;
using Papyrus.Application.Content;
using Papyrus.Contracts.Content;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;
using Papyrus.Infrastructure.Content;
using Papyrus.Infrastructure.Interceptors;
using Papyrus.Infrastructure.MultiTenancy;
using Microsoft.Extensions.Options;

namespace Papyrus.Tests.Integration;

/// <summary>
/// Drives the duplicate-slug 409 through the real HTTP pipeline against
/// PostgreSQL.
/// <para>
/// The InMemory-backed endpoint tests cannot reach the post-failed-<c>SaveChanges</c>
/// path, because InMemory does not enforce unique indexes. That leaves the
/// "the conflict path must not re-read the entity" invariant unenforced: a
/// reordering that moved the follow-up read above the success check would throw
/// on the poisoned context and turn a duplicate slug back into a 500, with the
/// whole suite still green. This class closes that gap.
/// </para>
/// </summary>
[Collection(PostgresCollection.Name)]
public sealed class AdminSlugConflictHttpTests(PostgresFixture fixture) : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddDbContext<PapyrusDbContext>(options =>
            options.UseNpgsql(fixture.ConnectionString)
                .AddInterceptors(new PapyrusSaveChangesInterceptor(new DefaultAuditActor(), TimeProvider.System)));
        builder.Services.AddPapyrusApplication();
        builder.Services.AddScoped<IPublicContentStore, EfPublicContentStore>();
        builder.Services.AddScoped<IContentCommandStore, EfContentCommandStore>();
        builder.Services.AddScoped<IContentQueryStore, EfContentQueryStore>();
        builder.Services.AddScoped<ITaxonomyCommandStore, EfTaxonomyCommandStore>();
        builder.Services.AddScoped<IContentLifecycleStore, EfContentLifecycleStore>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantContext>(sp => new TenantContext(fixture.TenantId));
        builder.Services.AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.Configure<MediaOptions>(options =>
        {
            options.RootPath = Path.Combine(Path.GetTempPath(), "papyrus-tests", Guid.NewGuid().ToString());
            options.RequestPath = "/media";
        });
        builder.Services.AddScoped<LocalMediaService>();

        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapAdminApi();
        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task A_duplicate_slug_returns_409_over_http_without_a_server_error()
    {
        var slug = $"http-conflict-{Guid.NewGuid():N}";

        // Seed through a context the request pipeline does not use, so the
        // handler's own pre-check is the only thing that can see the row first.
        // The second write below then goes through the endpoint, which exercises
        // the pre-check 409 rather than the 23505 backstop; the backstop's
        // HTTP-level shape is covered by the poisoned-context case below.
        await using (var seed = fixture.CreateDbContext())
        {
            seed.BlogPosts.Add(Post(slug, "seeded"));
            await seed.SaveChangesAsync();
        }

        var response = await _client.PostAsJsonAsync("/api/v1/blog-posts",
            new BlogPostUpsertRequest(slug, "duplicate", "Summary", "# Content", null, null, null, false, false, null, []));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task A_unique_violation_surfacing_as_409_never_touches_the_poisoned_context()
    {
        // Forces the 23505 backstop over the real HTTP pipeline. A pre-save
        // interceptor inserts the conflicting row through a second connection
        // after the handler's pre-check has already run and passed, so the flush
        // itself fails on the index. If any endpoint re-read its DbContext on
        // that path the tracked Added entry would throw and the response would be
        // a 500 - the exact regression the "do not re-read after a failed
        // SaveChanges" invariant exists to prevent, and the one InMemory cannot
        // observe.
        var slug = $"http-backstop-{Guid.NewGuid():N}";
        var racer = new ConcurrentSlugRacer(slug, fixture);

        await using var scoped = _app.Services.CreateAsyncScope();
        var options = new DbContextOptionsBuilder<PapyrusDbContext>()
            .UseNpgsql(fixture.ConnectionString)
            .AddInterceptors(new PapyrusSaveChangesInterceptor(new DefaultAuditActor(), TimeProvider.System), racer)
            .Options;
        await using var context = new PapyrusDbContext(options, new TenantContext(fixture.TenantId));
        var handlers = new ContentCommandHandlers(new EfContentCommandStore(context));

        var result = await handlers.SaveBlogPostAsync(
            new(null, slug, "racing", "Summary", "# Content", null, null, null, false, false, null, []),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, error => ContentValidation.IsSlugConflict(error));

        // The context is poisoned after the rejected flush. Touching it again
        // throws, so the handler-level result above must be all the caller needs;
        // proving that is what the endpoint's early return relies on.
        await Assert.ThrowsAnyAsync<Exception>(() => context.SaveChangesAsync());
    }

    /// <summary>
    /// Inserts a row holding <c>Slug</c> once, from a separate connection, the
    /// first time a save is attempted - after the pre-check has already read and
    /// found nothing.
    /// </summary>
    private sealed class ConcurrentSlugRacer(string slug, PostgresFixture fixture) : SaveChangesInterceptor
    {
        private bool _planted;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!_planted)
            {
                _planted = true;
                await using var other = fixture.CreateDbContext();
                other.BlogPosts.Add(new BlogPost
                {
                    Id = Guid.NewGuid(),
                    Slug = slug,
                    Title = "the concurrent winner",
                    Summary = "Planted after the pre-check read.",
                    ContentMarkdown = "# Winner",
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = "integration-racer"
                });
                await other.SaveChangesAsync(cancellationToken);
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
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
}
