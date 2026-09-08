using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Papyrus.Core.Content;
using Papyrus.Application.Content;
using Papyrus.Infrastructure.Content;
using BlogIndexModel = Papyrus.Web.Blog.Pages.IndexModel;
using PortfolioIndexModel = Papyrus.Web.Portfolio.Pages.IndexModel;
using Papyrus.Web.Shared;

namespace Papyrus.Tests;

public sealed class PublicFilteringTests
{
    [Fact]
    public async Task Blog_index_returns_only_published_live_posts()
    {
        await using var db = TestDb.Create();
        db.BlogPosts.AddRange(Post("live", true, DateTimeOffset.UtcNow.AddMinutes(-1)), Post("draft", false, null), Post("future", true, DateTimeOffset.UtcNow.AddDays(1)));
        await db.SaveChangesAsync();
        var model = new BlogIndexModel(new PublicContentQueryHandlers(new EfPublicContentStore(db)), Options.Create(new SiteOptions())) { PageContext = PageContext() };

        await model.OnGetAsync(null, null);

        Assert.Single(model.Posts);
        Assert.Equal("live", model.Posts[0].Slug);
        Assert.Equal("live", model.FeaturedPost?.Slug);
    }

    [Fact]
    public async Task Portfolio_index_returns_only_published_live_projects()
    {
        await using var db = TestDb.Create();
        db.Projects.AddRange(Project("live", true, DateTimeOffset.UtcNow.AddMinutes(-1)), Project("draft", false, null), Project("future", true, DateTimeOffset.UtcNow.AddDays(1)));
        await db.SaveChangesAsync();
        var model = new PortfolioIndexModel(new PublicContentQueryHandlers(new EfPublicContentStore(db)), Options.Create(new SiteOptions())) { PageContext = PageContext() };

        await model.OnGetAsync(null, null);

        Assert.Single(model.Projects);
        Assert.Equal("live", model.Projects[0].Slug);
    }

    [Fact]
    public void Public_base_url_never_uses_the_request_host_when_unconfigured()
    {
        var request = new DefaultHttpContext().Request;
        request.Scheme = "https";
        request.Host = new HostString("attacker.example");

        Assert.Null(SiteUrl.ResolvePublicBase(null, request));
        Assert.Equal("/Post/example", SiteUrl.Combine(SiteUrl.ResolvePublicBase(null, request), "/Post/example"));
    }

    [Fact]
    public async Task Public_index_page_size_is_bounded_by_the_handler()
    {
        await using var db = TestDb.Create();
        db.Projects.AddRange(Enumerable.Range(0, 5).Select(index => Project($"project-{index}", true, DateTimeOffset.UtcNow.AddMinutes(-index))));
        await db.SaveChangesAsync();

        var result = await new PublicContentQueryHandlers(new EfPublicContentStore(db)).GetPortfolioIndexAsync(null, null, DateTimeOffset.UtcNow, 1, 2, CancellationToken.None);

        Assert.Equal(2, result.Projects.Count);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task Public_search_is_case_insensitive_and_bounded_to_the_requested_page()
    {
        await using var db = TestDb.Create();
        var securityPost = Post("security", true, DateTimeOffset.UtcNow.AddMinutes(-1));
        securityPost.Title = "Security Search";
        db.BlogPosts.AddRange(securityPost, Post("other", true, DateTimeOffset.UtcNow.AddMinutes(-2)));
        await db.SaveChangesAsync();

        var result = await new PublicContentQueryHandlers(new EfPublicContentStore(db)).GetBlogIndexAsync("SECURITY", null, DateTimeOffset.UtcNow, 1, 1, CancellationToken.None);

        Assert.Single(result.Posts);
        Assert.Equal("security", result.Posts[0].Slug);
    }

    private static PageContext PageContext()
    {
        var httpContext = new DefaultHttpContext();
        var pageContext = new PageContext { HttpContext = httpContext };
        pageContext.ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
        return pageContext;
    }

    private static BlogPost Post(string slug, bool published, DateTimeOffset? at) => new() { Id = Guid.NewGuid(), Slug = slug, Title = slug, IsPublished = published, PublishedAt = at, CreatedAt = DateTimeOffset.UtcNow };
    private static Project Project(string slug, bool published, DateTimeOffset? at) => new() { Id = Guid.NewGuid(), Slug = slug, Title = slug, IsPublished = published, PublishedAt = at, CreatedAt = DateTimeOffset.UtcNow };
}
