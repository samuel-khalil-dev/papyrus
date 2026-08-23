using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Papyrus.Application.Content;
using Papyrus.Contracts.Content;
using Papyrus.Core.Content;
using Papyrus.Web.Shared;
using Microsoft.Extensions.Options;

namespace Papyrus.Web.Portfolio.Pages;

public class ProjectModel(PublicContentQueryHandlers queries, IOptions<SiteOptions> siteOptions) : PageModel
{
    public PublicProjectDetail? Project { get; private set; }
    public IReadOnlyList<PublicProjectCard> RelatedProjects { get; private set; } = Array.Empty<PublicProjectCard>();
    public IReadOnlyList<PublicPostCard> RelatedPosts { get; private set; } = Array.Empty<PublicPostCard>();
    public IReadOnlyList<PublicTag> AllTags { get; private set; } = Array.Empty<PublicTag>();
    public string BlogBaseUrl { get; private set; } = string.Empty;
    public int ReadingTimeMinutes { get; private set; }
    public string RenderedDescription { get; private set; } = string.Empty;
    public string? ResolvedCoverImageUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var now = DateTimeOffset.UtcNow;

        var result = await queries.GetProjectPageAsync(slug, now, HttpContext.RequestAborted);
        if (result is null)
        {
            return NotFound();
        }
        Project = result.Project;
        RelatedProjects = result.RelatedProjects;
        RelatedPosts = result.RelatedPosts;
        AllTags = result.AllTags;
        var project = result.Project;

        var publicBaseUrl = SiteUrl.ResolvePublicBase(siteOptions.Value.PublicBaseUrl, Request);
        ResolvedCoverImageUrl = MediaUrl.Resolve(siteOptions.Value.MediaBaseUrl, project.CoverImageUrl, publicBaseUrl);

        RenderedDescription = MarkdownRenderer.ToSafeHtml(project.Description);
        ReadingTimeMinutes = Math.Max(1, (int)Math.Ceiling(CountWords(project.Description) / 210d));
        BlogBaseUrl = SiteUrl.ResolvePublicBase(siteOptions.Value.BlogBaseUrl, Request) ?? string.Empty;

        var baseUrl = SiteUrl.ResolvePublicBase(siteOptions.Value.PublicBaseUrl, Request);
        ViewData["Description"] = project.Summary;
        ViewData["Canonical"] = SiteUrl.Combine(baseUrl, $"/Project/{project.Slug}");
        ViewData["OgImage"] = ResolvedCoverImageUrl;
        ViewData["OgType"] = "article";

        return Page();
    }

    private static int CountWords(string? content)
    {
        return System.Text.RegularExpressions.Regex.Matches(content ?? string.Empty, @"\b\w+\b").Count;
    }
}
