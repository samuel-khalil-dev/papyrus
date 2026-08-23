using Microsoft.AspNetCore.Mvc.RazorPages;
using Papyrus.Application.Content;
using Papyrus.Contracts.Content;
using Papyrus.Web.Shared;
using Microsoft.Extensions.Options;

namespace Papyrus.Web.Portfolio.Pages;

public class IndexModel(PublicContentQueryHandlers queries, IOptions<SiteOptions> siteOptions) : PageModel
{
    private const int PageSize = 6;

    public string? SearchTerm { get; private set; }
    public string? Tag { get; private set; }
    public PublicProjectCard? FeaturedProject { get; private set; }
    public IReadOnlyList<PublicProjectCard> Projects { get; private set; } = Array.Empty<PublicProjectCard>();
    public IReadOnlyList<PublicTag> Tags { get; private set; } = Array.Empty<PublicTag>();
    public int CurrentPage { get; private set; } = 1;
    public int TotalPages { get; private set; } = 1;
    public int TotalProjects { get; private set; }
    public string? FeaturedCoverImageUrl { get; private set; }
    public IReadOnlyDictionary<Guid, string?> CoverImageUrls { get; private set; } = new Dictionary<Guid, string?>();

    public async Task OnGetAsync(string? q, string? tag, int page = 1)
    {
        SearchTerm = q?.Trim();
        Tag = tag?.Trim();
        CurrentPage = page < 1 ? 1 : page;

        var now = DateTimeOffset.UtcNow;
        var result = await queries.GetPortfolioIndexAsync(SearchTerm, Tag, now, CurrentPage, PageSize, HttpContext.RequestAborted);
        FeaturedProject = result.FeaturedProject;

        var publicBaseUrl = SiteUrl.ResolvePublicBase(siteOptions.Value.PublicBaseUrl, Request);
        FeaturedCoverImageUrl = MediaUrl.Resolve(siteOptions.Value.MediaBaseUrl, FeaturedProject?.CoverImageUrl, publicBaseUrl);

        TotalProjects = result.TotalProjects;
        TotalPages = result.TotalPages;
        CurrentPage = Math.Min(CurrentPage, TotalPages);
        Projects = result.Projects;
        CoverImageUrls = Projects.ToDictionary(project => project.Id, project => MediaUrl.Resolve(siteOptions.Value.MediaBaseUrl, project.CoverImageUrl, publicBaseUrl));

        Tags = result.Tags;

        ViewData["Description"] = FeaturedProject?.Summary ?? "A curated showcase of development and DevOps projects.";
        ViewData["Canonical"] = SiteUrl.Combine(publicBaseUrl, "/");
        ViewData["OgImage"] = FeaturedCoverImageUrl;
        ViewData["OgType"] = "website";
    }
}
