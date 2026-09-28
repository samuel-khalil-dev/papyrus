using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Papyrus.Api.Admin;
using Papyrus.Api.Admin.Media;
using Papyrus.Application;
using Papyrus.Application.Content;
using Papyrus.Contracts.Content;
using Papyrus.Infrastructure;
using Papyrus.Infrastructure.Content;
using Papyrus.Infrastructure.Interceptors;
using Papyrus.Infrastructure.MultiTenancy;

namespace Papyrus.Tests;

/// <summary>
/// Covers the two Admin content-write outcomes that previously answered 500:
/// a slug already held by another row, and an omitted optional tag list.
/// <para>
/// The test host uses the InMemory provider, which does not enforce unique
/// indexes, so the conflict cases exercise only the pre-check path. The
/// race-proof <c>23505</c> backstop is covered separately, against PostgreSQL,
/// in <see cref="Papyrus.Tests.Integration.SlugConflictTests"/>; a green run
/// here is not evidence that the backstop works.
/// </para>
/// <para>
/// The tag cases send raw JSON deliberately: constructing a request record in a
/// test always supplies the <c>tagIds</c> property and would therefore never
/// reproduce the JSON binder's null for an omitted one.
/// </para>
/// </summary>
public sealed class AdminContentWriteConflictTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private string _databaseName = null!;

    public async Task InitializeAsync()
    {
        _databaseName = Guid.NewGuid().ToString();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddDbContext<PapyrusDbContext>(options =>
            options.UseInMemoryDatabase(_databaseName)
                .AddInterceptors(new PapyrusSaveChangesInterceptor(new DefaultAuditActor(), TimeProvider.System)));
        builder.Services.AddPapyrusApplication();
        builder.Services.AddScoped<IPublicContentStore, EfPublicContentStore>();
        builder.Services.AddScoped<IContentCommandStore, EfContentCommandStore>();
        builder.Services.AddScoped<IContentQueryStore, EfContentQueryStore>();
        builder.Services.AddScoped<ITaxonomyCommandStore, EfTaxonomyCommandStore>();
        builder.Services.AddScoped<IContentLifecycleStore, EfContentLifecycleStore>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantContext, TenantContext>();
        builder.Services.AddAuthentication("Test")
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });
        builder.Services.AddAuthorization();
        // MapAdminApi also maps the media endpoints, which take an IFormFile and
        // need the media service registered even though no test calls them.
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
    public async Task Creating_a_second_post_with_the_same_slug_returns_conflict_and_stores_only_one_row()
    {
        var first = await CreatePostAsync("duplicate-post");
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var conflict = await _client.PostAsJsonAsync("/api/v1/blog-posts",
            new BlogPostUpsertRequest("duplicate-post", "Another", "Summary", "Content", null, null, null, false, false, null, []));

        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        var problem = await ReadProblemAsync(conflict);
        Assert.Equal("The slug is already in use.", problem.Title);
        // The message must identify the row that holds the slug, or an operator
        // cannot tell which record to change.
        Assert.NotNull(problem.Detail);
        Assert.Contains("blog post", problem.Detail, StringComparison.OrdinalIgnoreCase);

        await using var db = Db();
        Assert.Equal(1, await db.BlogPosts.CountAsync(post => post.Slug == "duplicate-post"));
    }

    [Fact]
    public async Task Renaming_a_post_onto_another_posts_slug_returns_conflict()
    {
        await CreatePostAsync("first-post");
        var second = await CreatePostAsync("second-post");
        var secondId = await second.Content.ReadFromJsonAsync<BlogPostResponse>();

        var conflict = await _client.PutAsJsonAsync($"/api/v1/blog-posts/{secondId!.Id}",
            new BlogPostUpsertRequest("first-post", "Renamed", "Summary", "Content", null, null, null, false, false, null, []));

        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);

        await using var db = Db();
        // The rejected rename must not have half-applied.
        Assert.Equal("second-post", (await db.BlogPosts.SingleAsync(post => post.Id == secondId.Id)).Slug);
    }

    [Fact]
    public async Task Resubmitting_a_posts_own_slug_still_succeeds()
    {
        // The case a naive "does this slug already exist" check breaks: every
        // ordinary edit re-sends the row's current slug.
        var created = await CreatePostAsync("stable-post");
        var post = await created.Content.ReadFromJsonAsync<BlogPostResponse>();

        var update = await _client.PutAsJsonAsync($"/api/v1/blog-posts/{post!.Id}",
            new BlogPostUpsertRequest("stable-post", "Edited title", "Summary", "Content", null, null, null, false, false, null, []));

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
    }

    [Fact]
    public async Task Reusing_a_soft_deleted_posts_slug_is_allowed()
    {
        // The unique index is partial on "IsDeleted" = false, so the pre-check
        // must not be stricter than the index or a deleted slug would be
        // permanently burned.
        var created = await CreatePostAsync("recycled-post");
        var post = await created.Content.ReadFromJsonAsync<BlogPostResponse>();
        Assert.NotNull(post);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/v1/blog-posts/{post.Id}")).StatusCode);

        var replacement = await CreatePostAsync("recycled-post");

        Assert.Equal(HttpStatusCode.Created, replacement.StatusCode);
    }

    [Fact]
    public async Task Duplicate_project_category_and_tag_slugs_all_return_conflict()
    {
        var project = new ProjectUpsertRequest("dup-project", "Project", "Summary", null, null, null, null, null, false, false, null, []);
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/v1/projects", project)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _client.PostAsJsonAsync("/api/v1/projects", project)).StatusCode);

        var category = new CategoryUpsertRequest("dup-category", "Category", null, true);
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/v1/categories", category)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _client.PostAsJsonAsync("/api/v1/categories", category)).StatusCode);

        var tag = new TagUpsertRequest("dup-tag", "Tag");
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/v1/tags", tag)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _client.PostAsJsonAsync("/api/v1/tags", tag)).StatusCode);
    }

    [Fact]
    public async Task A_malformed_request_is_still_a_validation_problem_not_a_conflict()
    {
        // The 409 mapping must not swallow ordinary field validation.
        var response = await _client.PostAsJsonAsync("/api/v1/tags", new TagUpsertRequest(string.Empty, ".NET"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await ReadProblemAsync(response);
        Assert.Equal("The request contains validation errors.", problem.Title);
    }

    [Fact]
    public async Task Omitting_the_optional_tag_ids_creates_the_post_instead_of_failing_with_a_server_error()
    {
        // tagIds carries no [Required] attribute, so an omitted property binds to
        // null even though the contract declares a non-nullable array. Raw JSON
        // is sent deliberately: constructing a BlogPostUpsertRequest would always
        // supply the property and would not reproduce the binder behaviour.
        const string payload = """
            {"slug":"no-tags-given","title":"No tags","summary":"Summary","contentMarkdown":"# Content"}
            """;
        using var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/blog-posts", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var post = await response.Content.ReadFromJsonAsync<BlogPostResponse>();
        Assert.NotNull(post);
        Assert.Empty(post.Tags);

        await using var db = Db();
        Assert.Equal(1, await db.BlogPosts.CountAsync(post => post.Slug == "no-tags-given"));
    }

    [Fact]
    public async Task Omitting_the_optional_tag_ids_creates_the_project_instead_of_failing_with_a_server_error()
    {
        const string payload = """
            {"slug":"no-tags-given","title":"No tags","summary":"Summary"}
            """;
        using var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/projects", content);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData("blog-post")]
    [InlineData("project")]
    public async Task An_explicitly_empty_tag_id_list_is_still_accepted(string entity)
    {
        // The boundary fix must not change the already-working empty-array path.
        var (route, request) = entity == "blog-post"
            ? ("/api/v1/blog-posts",
                (object)new BlogPostUpsertRequest("explicit-empty", "T", "S", "# C", null, null, null, false, false, null, []))
            : ("/api/v1/projects",
                new ProjectUpsertRequest("explicit-empty", "T", "S", null, null, null, null, null, false, false, null, []));

        var response = await _client.PostAsync(route, JsonContent.Create(request));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private async Task<HttpResponseMessage> CreatePostAsync(string slug) =>
        await _client.PostAsJsonAsync("/api/v1/blog-posts",
            new BlogPostUpsertRequest(slug, "Title", "Summary", "Content", null, null, null, false, false, null, []));

    private static async Task<ProblemShape> ReadProblemAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(payload);
        var root = document.RootElement;
        return new ProblemShape(
            root.TryGetProperty("title", out var title) ? title.GetString() : null,
            root.TryGetProperty("detail", out var detail) ? detail.GetString() : null);
    }

    private PapyrusDbContext Db() =>
        new(new DbContextOptionsBuilder<PapyrusDbContext>().UseInMemoryDatabase(_databaseName).Options);

    private sealed record ProblemShape(string? Title, string? Detail);
}
