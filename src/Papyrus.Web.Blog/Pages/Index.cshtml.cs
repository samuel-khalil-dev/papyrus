using Microsoft.AspNetCore.Mvc.RazorPages;
using Papyrus.Application.Content;
using Papyrus.Contracts.Content;
using Papyrus.Web.Shared;
using Microsoft.Extensions.Options;

namespace Papyrus.Web.Blog.Pages;

public class IndexModel(PublicContentQueryHandlers queries, IOptions<SiteOptions> siteOptions) : PageModel
{
    private const int PageSize = 6;

    public string? SearchTerm { get; private set; }
    public string? Tag { get; private set; }
    public PublicPostCard? FeaturedPost { get; private set; }
    public IReadOnlyList<PublicPostCard> Posts { get; private set; } = Array.Empty<PublicPostCard>();
    public IReadOnlyList<PublicTag> Tags { get; private set; } = Array.Empty<PublicTag>();
    public int CurrentPage { get; private set; } = 1;
    public int TotalPages { get; private set; } = 1;
    public int TotalPosts { get; private set; }
    public string? FeaturedCoverImageUrl { get; private set; }
    public IReadOnlyDictionary<Guid, string?> CoverImageUrls { get; private set; } = new Dictionary<Guid, string?>();

    public async Task OnGetAsync(string? q, string? tag, int page = 1)
    {
        SearchTerm = q?.Trim();
        Tag = tag?.Trim();
        CurrentPage = page < 1 ? 1 : page;

        var now = DateTimeOffset.UtcNow;
        var result = await queries.GetBlogIndexAsync(SearchTerm, Tag, now, CurrentPage, PageSize, HttpContext.RequestAborted);
        FeaturedPost = result.FeaturedPost;

        var publicBaseUrl = SiteUrl.ResolvePublicBase(siteOptions.Value.PublicBaseUrl, Request);
        FeaturedCoverImageUrl = MediaUrl.Resolve(siteOptions.Value.MediaBaseUrl, FeaturedPost?.CoverImageUrl, publicBaseUrl);

        TotalPosts = result.TotalPosts;
        TotalPages = result.TotalPages;
        CurrentPage = Math.Min(CurrentPage, TotalPages);
        Posts = result.Posts;
        CoverImageUrls = Posts.ToDictionary(post => post.Id, post => MediaUrl.Resolve(siteOptions.Value.MediaBaseUrl, post.CoverImageUrl, publicBaseUrl));

        Tags = result.Tags;

        ViewData["Description"] = FeaturedPost?.Summary ?? "Long-form technical writing about projects, implementation details, and lessons learned.";
        ViewData["Canonical"] = SiteUrl.Combine(publicBaseUrl, "/");
        ViewData["OgImage"] = FeaturedCoverImageUrl;
        ViewData["OgType"] = "website";
    }
}
