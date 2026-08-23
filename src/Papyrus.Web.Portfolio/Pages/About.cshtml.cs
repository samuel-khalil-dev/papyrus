using Microsoft.AspNetCore.Mvc.RazorPages;
using Papyrus.Web.Shared;
using Microsoft.Extensions.Options;

namespace Papyrus.Web.Portfolio.Pages;

public sealed class AboutModel(IOptions<SiteOptions> siteOptions) : PageModel
{
    public void OnGet()
    {
        var baseUrl = SiteUrl.ResolvePublicBase(siteOptions.Value.PublicBaseUrl, Request);
        ViewData["Description"] = "Learn about Papyrus, a personal site for technical writing and development work.";
        ViewData["Canonical"] = SiteUrl.Combine(baseUrl, "/About");
        ViewData["OgType"] = "website";
    }
}
