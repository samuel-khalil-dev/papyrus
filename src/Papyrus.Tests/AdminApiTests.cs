using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Papyrus.Api.Admin;
using Papyrus.Application;
using Papyrus.Application.Content;
using Papyrus.Contracts.Content;
using Papyrus.Api.Admin.Media;
using Papyrus.Infrastructure;
using Papyrus.Infrastructure.Content;
using Papyrus.Infrastructure.Interceptors;
using Papyrus.Infrastructure.MultiTenancy;

namespace Papyrus.Tests;

public sealed class AdminApiTests : IAsyncLifetime
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
    public async Task Project_and_blog_post_create_and_update_paths_persist_tags_and_relationships()
    {
        var tagId = Guid.NewGuid();
        await using (var db = Db())
        {
            db.ContentTags.Add(new Papyrus.Core.Content.ContentTag { Id = tagId, Slug = "dotnet", Name = ".NET" });
            await db.SaveChangesAsync();
        }

        var createProject = new ProjectUpsertRequest("papyrus", "Papyrus", "First", "Description", null, "https://live.test", null, null, false, true, DateTimeOffset.UtcNow, [tagId]);
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", createProject);
        Assert.Equal(HttpStatusCode.Created, projectResponse.StatusCode);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);
        Assert.Single(project.Tags);

        var updateProject = createProject with { Title = "Papyrus Updated", Slug = "papyrus-updated", TagIds = Array.Empty<Guid>() };
        var updatedProjectResponse = await _client.PutAsJsonAsync($"/api/projects/{project.Id}", updateProject);
        Assert.Equal(HttpStatusCode.OK, updatedProjectResponse.StatusCode);

        var createPost = new BlogPostUpsertRequest("first-post", "First Post", "Summary", "# Content", null, project.Id, null, false, true, DateTimeOffset.UtcNow, [tagId]);
        var postResponse = await _client.PostAsJsonAsync("/api/blog-posts", createPost);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        var post = await postResponse.Content.ReadFromJsonAsync<BlogPostResponse>();
        Assert.NotNull(post);
        Assert.Equal(project.Id, post.Project?.Id);
        Assert.Single(post.Tags);

        var updatePost = createPost with { Title = "Updated Post", Slug = "updated-post", ProjectId = null, TagIds = Array.Empty<Guid>() };
        var updatedPostResponse = await _client.PutAsJsonAsync($"/api/blog-posts/{post.Id}", updatePost);
        Assert.Equal(HttpStatusCode.OK, updatedPostResponse.StatusCode);

        await using var finalDb = Db();
        Assert.Equal("Papyrus Updated", (await finalDb.Projects.SingleAsync()).Title);
        Assert.Empty(await finalDb.ProjectTags.ToListAsync());
        Assert.Equal("Updated Post", (await finalDb.BlogPosts.SingleAsync()).Title);
        Assert.Null((await finalDb.BlogPosts.SingleAsync()).ProjectId);
        Assert.Empty(await finalDb.BlogPostTags.ToListAsync());
    }

    [Fact]
    public async Task Category_and_tag_endpoints_create_update_and_soft_delete_through_injected_handlers()
    {
        var categoryResponse = await _client.PostAsJsonAsync("/api/v1/categories",
            new CategoryUpsertRequest("engineering", "Engineering", "Build", true));
        Assert.Equal(HttpStatusCode.Created, categoryResponse.StatusCode);
        var category = await categoryResponse.Content.ReadFromJsonAsync<ContentCategoryResponse>();
        Assert.NotNull(category);

        var updatedCategoryResponse = await _client.PutAsJsonAsync($"/api/v1/categories/{category.Id}",
            new CategoryUpsertRequest("platform", "Platform", "Updated", false));
        Assert.Equal(HttpStatusCode.OK, updatedCategoryResponse.StatusCode);

        var tagResponse = await _client.PostAsJsonAsync("/api/v1/tags", new TagUpsertRequest("dotnet", ".NET"));
        Assert.Equal(HttpStatusCode.Created, tagResponse.StatusCode);
        var tag = await tagResponse.Content.ReadFromJsonAsync<ContentTagResponse>();
        Assert.NotNull(tag);

        var updatedTagResponse = await _client.PutAsJsonAsync($"/api/v1/tags/{tag.Id}", new TagUpsertRequest("runtime", "Runtime"));
        Assert.Equal(HttpStatusCode.OK, updatedTagResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/v1/categories/{category.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/v1/tags/{tag.Id}")).StatusCode);

        await using var db = Db();
        Assert.True((await db.ContentCategories.IgnoreQueryFilters().SingleAsync()).IsDeleted);
        Assert.True((await db.ContentTags.IgnoreQueryFilters().SingleAsync()).IsDeleted);
    }

    [Fact]
    public async Task Project_and_blog_post_delete_endpoints_soft_delete_through_injected_lifecycle_handler()
    {
        var projectResponse = await _client.PostAsJsonAsync("/api/v1/projects",
            new ProjectUpsertRequest("project", "Project", "Summary", null, null, null, null, null, false, false, null, []));
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();
        Assert.NotNull(project);
        var postResponse = await _client.PostAsJsonAsync("/api/v1/blog-posts",
            new BlogPostUpsertRequest("post", "Post", "Summary", "Content", null, project.Id, null, false, false, null, []));
        var post = await postResponse.Content.ReadFromJsonAsync<BlogPostResponse>();
        Assert.NotNull(post);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/v1/projects/{project.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/v1/blog-posts/{post.Id}")).StatusCode);

        await using var db = Db();
        Assert.True((await db.Projects.IgnoreQueryFilters().SingleAsync()).IsDeleted);
        Assert.True((await db.BlogPosts.IgnoreQueryFilters().SingleAsync()).IsDeleted);
    }

    [Fact]
    public async Task Legacy_collection_routes_are_paginated()
    {
        await using (var db = Db())
        {
            db.Projects.AddRange(Enumerable.Range(0, 3).Select(index => new Papyrus.Core.Content.Project
            {
                Id = Guid.NewGuid(), Slug = $"project-{index}", Title = $"Project {index}", Summary = "Summary",
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-index)
            }));
            await db.SaveChangesAsync();
        }

        var page = await _client.GetFromJsonAsync<List<ProjectResponse>>("/api/projects?page=2&pageSize=2");

        Assert.NotNull(page);
        Assert.Single(page);
    }

    [Fact]
    public async Task Admin_paths_cannot_read_mutate_link_or_delete_another_tenant()
    {
        var mainProjectId = Guid.NewGuid();
        var otherProjectId = Guid.NewGuid();
        await using (var mainDb = Db(new TenantContext("main")))
        {
            mainDb.Projects.Add(new Papyrus.Core.Content.Project
            {
                Id = mainProjectId, Slug = "main-project", Title = "Main project", Summary = "Main", IsPublished = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await mainDb.SaveChangesAsync();
        }

        await using (var otherDb = Db(new TenantContext("other")))
        {
            otherDb.Projects.Add(new Papyrus.Core.Content.Project
            {
                Id = otherProjectId, Slug = "other-project", Title = "Other project", Summary = "Other", IsPublished = true,
                CreatedAt = DateTimeOffset.UtcNow
            });
            otherDb.ContentTags.Add(new Papyrus.Core.Content.ContentTag
            {
                Id = Guid.NewGuid(), Slug = "other-tag", Name = "Other tag", CreatedAt = DateTimeOffset.UtcNow
            });
            await otherDb.SaveChangesAsync();
        }

        using var otherClient = CreateClient("other");
        var listed = await otherClient.GetFromJsonAsync<List<ProjectResponse>>("/api/projects");
        Assert.NotNull(listed);
        Assert.Single(listed);
        Assert.Equal(otherProjectId, listed[0].Id);

        var crossTenantUpdate = await otherClient.PutAsJsonAsync(
            $"/api/projects/{mainProjectId}",
            new ProjectUpsertRequest("attempt", "Attempt", "Attempt", null, null, null, null, null, false, true, DateTimeOffset.UtcNow, []));
        Assert.Equal(HttpStatusCode.NotFound, crossTenantUpdate.StatusCode);

        var crossTenantDelete = await otherClient.DeleteAsync($"/api/projects/{mainProjectId}");
        Assert.Equal(HttpStatusCode.NotFound, crossTenantDelete.StatusCode);

        var crossTenantLink = await otherClient.PostAsJsonAsync(
            "/api/blog-posts",
            new BlogPostUpsertRequest("cross", "Cross", "Cross", "Cross", null, mainProjectId, null, false, true, DateTimeOffset.UtcNow, []));
        Assert.Equal(HttpStatusCode.BadRequest, crossTenantLink.StatusCode);

        using var upload = new MultipartFormDataContent();
         var image = new ByteArrayContent(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="));
        image.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
        upload.Add(image, "file", "other.png");
        var uploadResponse = await otherClient.PostAsync("/api/media", upload);
        Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);
        var media = await uploadResponse.Content.ReadFromJsonAsync<MediaUploadResult>();
        Assert.NotNull(media);

        _client.DefaultRequestHeaders.Remove(TenantContext.HeaderName);
        using var mainMediaRequest = new HttpRequestMessage(HttpMethod.Get, "/api/media");
        mainMediaRequest.Headers.Add(TenantContext.HeaderName, "main");
        var mainMediaResponse = await _client.SendAsync(mainMediaRequest);
        var mainMedia = await mainMediaResponse.Content.ReadFromJsonAsync<List<MediaFileResult>>();
        Assert.NotNull(mainMedia);
        Assert.Empty(mainMedia);
        using var mainDeleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/media/{media.FileName}");
        mainDeleteRequest.Headers.Add(TenantContext.HeaderName, "main");
        Assert.Equal(HttpStatusCode.NotFound, (await _client.SendAsync(mainDeleteRequest)).StatusCode);
        using var otherDeleteRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/media/{media.FileName}");
        otherDeleteRequest.Headers.Add(TenantContext.HeaderName, "other");
        Assert.Equal(HttpStatusCode.NoContent, (await _client.SendAsync(otherDeleteRequest)).StatusCode);

        await using var verification = Db(new TenantContext("main"));
        Assert.Equal("Main project", (await verification.Projects.SingleAsync()).Title);
        Assert.Empty(await verification.BlogPosts.ToListAsync());
        await using var otherVerification = Db(new TenantContext("other"));
        Assert.Equal("Other project", (await otherVerification.Projects.SingleAsync()).Title);
    }

    private HttpClient CreateClient(string tenantId)
    {
        var client = _app.GetTestClient();
        client.DefaultRequestHeaders.Remove(TenantContext.HeaderName);
        client.DefaultRequestHeaders.Add(TenantContext.HeaderName, tenantId);
        client.DefaultRequestHeaders.Add("X-Admin-Api-Key", "test");
        return client;
    }

    private PapyrusDbContext Db(ITenantContext? tenantContext = null) =>
        new(new DbContextOptionsBuilder<PapyrusDbContext>().UseInMemoryDatabase(_databaseName).Options, tenantContext);

    private sealed class TestAuthenticationHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "test")], "Test");
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
        }
    }
}
