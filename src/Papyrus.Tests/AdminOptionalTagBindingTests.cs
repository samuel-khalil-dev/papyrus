using System.Net;
using System.Net.Http.Json;
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
/// Covers the optional <c>tagIds</c> binding on the content write endpoints.
/// <para>
/// The contract declares a non-nullable <c>Guid[]</c> but attaches no
/// <c>[Required]</c> attribute, so the JSON binder hands a <c>null</c> through
/// for an omitted property. The cases below send raw JSON deliberately:
/// constructing a request record in a test always supplies the property and
/// would therefore never reproduce the binder behaviour.
/// </para>
/// </summary>
public sealed class AdminOptionalTagBindingTests : IAsyncLifetime
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
    public async Task Omitting_the_optional_tag_ids_creates_the_post_instead_of_failing_with_a_server_error()
    {
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

    private PapyrusDbContext Db() =>
        new(new DbContextOptionsBuilder<PapyrusDbContext>().UseInMemoryDatabase(_databaseName).Options);
}
