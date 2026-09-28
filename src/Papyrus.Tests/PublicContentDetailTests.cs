using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Papyrus.Application.Content;
using Papyrus.Core.Content;
using Papyrus.Infrastructure.Content;
using Papyrus.Web.Shared;
using BlogPostModel = Papyrus.Web.Blog.Pages.PostModel;
using ProjectModel = Papyrus.Web.Portfolio.Pages.ProjectModel;

namespace Papyrus.Tests;

/// Covers the public detail pages, which the index tests and the browser suite
/// both skip: the e2e specs only ever visit the two index pages, so a broken
/// detail route, a leaking canonical URL, or a mis-set metadata tag would not
/// have been detected anywhere in the suite.
public sealed class PublicContentDetailTests
{
    [Fact]
    public async Task Post_detail_returns_not_found_for_an_unknown_slug()
    {
        await using var db = TestDb.Create();
        var model = PostModel(db);

        var result = await model.OnGetAsync("no-such-post");

        Assert.IsType<NotFoundResult>(result);
        Assert.Null(model.Post);
    }

    [Fact]
    public async Task Post_detail_returns_not_found_for_an_unpublished_post()
    {
        await using var db = TestDb.Create();
        db.BlogPosts.Add(MakePost("draft", isPublished: false, publishedAt: null));
        await db.SaveChangesAsync();
        var model = PostModel(db);

        var result = await model.OnGetAsync("draft");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Post_detail_returns_not_found_for_a_soft_deleted_post()
    {
        await using var db = TestDb.Create();
        var post = MakePost("removed", isPublished: true, publishedAt: DateTimeOffset.UtcNow.AddMinutes(-1));
        db.BlogPosts.Add(post);
        await db.SaveChangesAsync();
        db.BlogPosts.Remove(post);
        await db.SaveChangesAsync();
        var model = PostModel(db);

        var result = await model.OnGetAsync("removed");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Post_detail_exposes_the_post_and_its_rendered_content()
    {
        await using var db = TestDb.Create();
        var post = MakePost("live", isPublished: true, publishedAt: DateTimeOffset.UtcNow.AddMinutes(-1));
        post.Title = "Live post";
        post.Summary = "A summary";
        post.ContentMarkdown = "# Heading\n\nBody text with enough words to read.";
        db.BlogPosts.Add(post);
        await db.SaveChangesAsync();
        var model = PostModel(db);

        var result = await model.OnGetAsync("live");

        Assert.IsType<PageResult>(result);
        Assert.NotNull(model.Post);
        Assert.Equal("live", model.Post!.Slug);
        Assert.Equal("A summary", model.ViewData["Description"]);
        Assert.Contains("<h1", model.RenderedContent, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Post_detail_does_not_emit_raw_html_from_content_markdown()
    {
        await using var db = TestDb.Create();
        var post = MakePost("hostile", isPublished: true, publishedAt: DateTimeOffset.UtcNow.AddMinutes(-1));
        post.ContentMarkdown = "Safe text\n\n<script>alert('x')</script>";
        db.BlogPosts.Add(post);
        await db.SaveChangesAsync();
        var model = PostModel(db);

        await model.OnGetAsync("hostile");

        Assert.DoesNotContain("<script>", model.RenderedContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Post_detail_reading_time_is_never_below_one_minute()
    {
        await using var db = TestDb.Create();
        var post = MakePost("short", isPublished: true, publishedAt: DateTimeOffset.UtcNow.AddMinutes(-1));
        post.ContentMarkdown = "Hi.";
        db.BlogPosts.Add(post);
        await db.SaveChangesAsync();
        var model = PostModel(db);

        await model.OnGetAsync("short");

        Assert.Equal(1, model.ReadingTimeMinutes);
    }

    [Fact]
    public async Task Post_detail_sets_canonical_and_social_metadata_from_the_configured_base_url()
    {
        await using var db = TestDb.Create();
        var post = MakePost("live", isPublished: true, publishedAt: DateTimeOffset.UtcNow.AddMinutes(-1));
        post.CoverImageUrl = "/media/main/abc.png";
        db.BlogPosts.Add(post);
        await db.SaveChangesAsync();
        var model = PostModel(db, new SiteOptions { PublicBaseUrl = "https://papyrus.test" });

        await model.OnGetAsync("live");

        Assert.Equal("https://papyrus.test/Post/live", model.ViewData["Canonical"]);
        Assert.Equal("article", model.ViewData["OgType"]);
        Assert.Equal("https://papyrus.test/media/main/abc.png", model.ViewData["OgImage"]);
    }

    [Fact]
    public async Task Post_detail_canonical_never_uses_the_request_host_when_no_base_url_is_configured()
    {
        await using var db = TestDb.Create();
        db.BlogPosts.Add(MakePost("live", isPublished: true, publishedAt: DateTimeOffset.UtcNow.AddMinutes(-1)));
        await db.SaveChangesAsync();
        var model = PostModel(db, new SiteOptions());
        model.PageContext.HttpContext.Request.Host = new HostString("attacker.example");
        model.PageContext.HttpContext.Request.Scheme = "https";

        await model.OnGetAsync("live");

        Assert.Equal("/Post/live", model.ViewData["Canonical"]);
    }

    [Fact]
    public async Task Project_detail_returns_not_found_for_an_unknown_slug()
    {
        await using var db = TestDb.Create();
        var model = ProjectModelUnderTest(db);

        var result = await model.OnGetAsync("no-such-project");

        Assert.IsType<NotFoundResult>(result);
        Assert.Null(model.Project);
    }

    [Fact]
    public async Task Project_detail_returns_not_found_for_an_unpublished_project()
    {
        await using var db = TestDb.Create();
        db.Projects.Add(MakeProject("draft", isPublished: false, publishedAt: null));
        await db.SaveChangesAsync();
        var model = ProjectModelUnderTest(db);

        var result = await model.OnGetAsync("draft");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Project_detail_exposes_the_project_with_rendered_description()
    {
        await using var db = TestDb.Create();
        var project = MakeProject("live", isPublished: true, publishedAt: DateTimeOffset.UtcNow.AddMinutes(-1));
        project.Summary = "Project summary";
        project.Description = "## Section\n\nDescription body.";
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        var model = ProjectModelUnderTest(db);

        var result = await model.OnGetAsync("live");

        Assert.IsType<PageResult>(result);
        Assert.NotNull(model.Project);
        Assert.Equal("live", model.Project!.Slug);
        Assert.Equal("Project summary", model.ViewData["Description"]);
        Assert.Contains("<h2", model.RenderedDescription, StringComparison.Ordinal);
        Assert.True(model.ReadingTimeMinutes >= 1);
    }

    [Fact]
    public async Task Project_detail_sets_canonical_and_social_metadata()
    {
        await using var db = TestDb.Create();
        db.Projects.Add(MakeProject("live", isPublished: true, publishedAt: DateTimeOffset.UtcNow.AddMinutes(-1)));
        await db.SaveChangesAsync();
        var model = ProjectModelUnderTest(db, new SiteOptions { PublicBaseUrl = "https://papyrus.test" });

        await model.OnGetAsync("live");

        Assert.Equal("https://papyrus.test/Project/live", model.ViewData["Canonical"]);
        Assert.Equal("article", model.ViewData["OgType"]);
    }

    private static BlogPostModel PostModel(Papyrus.Infrastructure.PapyrusDbContext db, SiteOptions? options = null) =>
        new(new PublicContentQueryHandlers(new EfPublicContentStore(db)), Options.Create(options ?? new SiteOptions()))
        {
            PageContext = PageContext(),
        };

    private static ProjectModel ProjectModelUnderTest(Papyrus.Infrastructure.PapyrusDbContext db, SiteOptions? options = null) =>
        new(new PublicContentQueryHandlers(new EfPublicContentStore(db)), Options.Create(options ?? new SiteOptions()))
        {
            PageContext = PageContext(),
        };

    private static PageContext PageContext()
    {
        var pageContext = new PageContext { HttpContext = new DefaultHttpContext() };
        pageContext.ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
        return pageContext;
    }

    private static BlogPost MakePost(string slug, bool isPublished, DateTimeOffset? publishedAt) =>
        new() { Id = Guid.NewGuid(), Slug = slug, Title = slug, IsPublished = isPublished, PublishedAt = publishedAt, CreatedAt = DateTimeOffset.UtcNow };

    private static Project MakeProject(string slug, bool isPublished, DateTimeOffset? publishedAt) =>
        new() { Id = Guid.NewGuid(), Slug = slug, Title = slug, IsPublished = isPublished, PublishedAt = publishedAt, CreatedAt = DateTimeOffset.UtcNow };
}
