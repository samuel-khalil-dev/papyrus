using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Papyrus.Application.Content;
using Papyrus.Contracts.Content;
using Papyrus.Core.Content;
using Papyrus.Web.Shared;
using Microsoft.Extensions.Options;

namespace Papyrus.Web.Blog.Pages;

public class PostModel(PublicContentQueryHandlers queries, IOptions<SiteOptions> siteOptions) : PageModel
{
    public PublicPostDetail? Post { get; private set; }
    public IReadOnlyList<PublicPostCard> RelatedPosts { get; private set; } = Array.Empty<PublicPostCard>();
    public IReadOnlyList<PublicTag> AllTags { get; private set; } = Array.Empty<PublicTag>();
    public int ReadingTimeMinutes { get; private set; }
    public string RenderedContent { get; private set; } = string.Empty;
    public string? ResolvedCoverImageUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var now = DateTimeOffset.UtcNow;

        var result = await queries.GetPostPageAsync(slug, now, HttpContext.RequestAborted);
        if (result is null)
        {
            return NotFound();
        }
        Post = result.Post;
        RelatedPosts = result.RelatedPosts;
        AllTags = result.AllTags;
        var post = result.Post;

        var publicBaseUrl = SiteUrl.ResolvePublicBase(siteOptions.Value.PublicBaseUrl, Request);
        ResolvedCoverImageUrl = MediaUrl.Resolve(siteOptions.Value.MediaBaseUrl, post.CoverImageUrl, publicBaseUrl);

        ReadingTimeMinutes = Math.Max(1, (int)Math.Ceiling(CountWords(post.ContentMarkdown) / 210d));
        RenderedContent = MarkdownRenderer.ToSafeHtml(post.ContentMarkdown);

        var baseUrl = publicBaseUrl;
        ViewData["Description"] = post.Summary;
        ViewData["Canonical"] = SiteUrl.Combine(baseUrl, $"/Post/{post.Slug}");
        ViewData["OgImage"] = ResolvedCoverImageUrl;
        ViewData["OgType"] = "article";

        return Page();
    }

    private static int CountWords(string content)
    {
        return Regex.Matches(content ?? string.Empty, @"\b\w+\b").Count;
    }
}
