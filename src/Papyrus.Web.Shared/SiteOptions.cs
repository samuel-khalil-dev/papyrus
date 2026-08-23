namespace Papyrus.Web.Shared;

public sealed class SiteOptions
{
    public const string SectionName = "Site";

    public string Name { get; set; } = "Papyrus";

    public string PublicBaseUrl { get; set; } = string.Empty;

    public string MediaBaseUrl { get; set; } = string.Empty;

    public string BlogBaseUrl { get; set; } = string.Empty;

    public string PortfolioBaseUrl { get; set; } = string.Empty;

    public string Kind { get; set; } = "blog";

    public string FooterHomeLabel { get; set; } = "Home";
}
