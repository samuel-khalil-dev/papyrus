using Papyrus.Web.Shared;

namespace Papyrus.Tests;

/// <summary>
/// Covers the M1 routing-truth policy. The cross-host distinctness rule cannot
/// be enforced at startup, because a process cannot see the other public host's
/// configuration, so it is asserted here over the shipped settings instead.
/// </summary>
public sealed class SiteHostValidatorTests
{
    [Fact]
    public void Configured_public_base_url_resolves_to_the_bare_host_without_its_port()
    {
        var site = new SiteOptions { PublicBaseUrl = "https://blog.example.com:8443/" };

        var allowed = SiteHostValidator.ResolveAllowedHosts(site);

        // HostFilteringOptions ignores ports, so an entry carrying one would
        // never match and every request would be refused.
        Assert.Contains("blog.example.com", allowed);
        Assert.DoesNotContain(allowed, host => host.Contains(':', StringComparison.Ordinal)
            && host != "[::1]");
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    [InlineData("[::1]")]
    public void Loopback_is_always_allowed_so_probes_still_reach_the_host(string loopback)
    {
        // CI, perf-baseline, release, the Aspire AppHost, and the local run path
        // all probe over loopback while the configured base URLs are real
        // domains. Without this every one of those probes would 400.
        var site = new SiteOptions { PublicBaseUrl = "https://blog.example.com" };

        Assert.Contains(loopback, SiteHostValidator.ResolveAllowedHosts(site));
    }

    [Fact]
    public void A_deployment_with_no_configured_base_url_keeps_the_permissive_behaviour()
    {
        // SiteUrl.ResolvePublicBase already falls back to relative URLs for an
        // unset public base, so an entirely unset deployment must not start
        // refusing every request.
        var site = new SiteOptions();

        Assert.Equal(["*"], SiteHostValidator.ResolveAllowedHosts(site));
    }

    [Fact]
    public void A_malformed_public_base_url_degrades_to_the_permissive_behaviour()
    {
        var site = new SiteOptions { PublicBaseUrl = "not a url" };

        Assert.Equal(["*"], SiteHostValidator.ResolveAllowedHosts(site));
    }

    [Fact]
    public void A_matching_kind_and_consistent_base_urls_validate()
    {
        var site = new SiteOptions
        {
            Kind = "blog",
            PublicBaseUrl = "https://blog.example.com",
            BlogBaseUrl = "https://blog.example.com",
            PortfolioBaseUrl = "https://example.com"
        };

        SiteHostValidator.Validate(site, SiteHostValidator.BlogKind);
    }

    [Fact]
    public void A_host_running_under_the_wrong_kind_fails_startup()
    {
        // Site:Kind is what the shared layout and About content branch on, so a
        // mismatch would publish one site's chrome on the other's pages.
        var site = new SiteOptions
        {
            Kind = "portfolio",
            PublicBaseUrl = "https://blog.example.com",
            BlogBaseUrl = "https://blog.example.com"
        };

        var error = Assert.Throws<InvalidOperationException>(
            () => SiteHostValidator.Validate(site, SiteHostValidator.BlogKind));

        Assert.Contains("Site:Kind", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_host_advertising_the_other_sites_canonical_urls_fails_startup()
    {
        var site = new SiteOptions
        {
            Kind = "blog",
            PublicBaseUrl = "https://portfolio.example.com",
            BlogBaseUrl = "https://blog.example.com"
        };

        var error = Assert.Throws<InvalidOperationException>(
            () => SiteHostValidator.Validate(site, SiteHostValidator.BlogKind));

        // The message must name the key so an operator can fix the deployment.
        Assert.Contains("Site:PublicBaseUrl", error.Message, StringComparison.Ordinal);
        Assert.Contains("Site:BlogBaseUrl", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void An_unset_public_base_url_stays_legal()
    {
        var site = new SiteOptions { Kind = "portfolio" };

        SiteHostValidator.Validate(site, SiteHostValidator.PortfolioKind);
    }

    [Fact]
    public void The_two_public_hosts_resolve_to_different_canonical_hosts()
    {
        // This is the milestone's "requests route to the correct container per
        // host" rule. It cannot be a startup check because each process only
        // sees its own configuration, so it is asserted over the shipped
        // settings here.
        var blog = LoadSiteOptions(Path.Combine("src", "Papyrus.Web.Blog", "appsettings.json"));
        var portfolio = LoadSiteOptions(Path.Combine("src", "Papyrus.Web.Portfolio", "appsettings.json"));

        SiteHostValidator.Validate(blog, SiteHostValidator.BlogKind);
        SiteHostValidator.Validate(portfolio, SiteHostValidator.PortfolioKind);

        var blogHost = SiteHostValidator.ResolveCanonicalHost(blog);
        var portfolioHost = SiteHostValidator.ResolveCanonicalHost(portfolio);

        Assert.NotNull(blogHost);
        Assert.NotNull(portfolioHost);
        Assert.NotEqual(blogHost, portfolioHost);
    }

    private static SiteOptions LoadSiteOptions(string relativePath)
    {
        var path = Path.Combine(FindRepositoryRoot(), relativePath);
        Assert.True(File.Exists(path), $"Expected {relativePath} to exist.");

        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path));
        if (!document.RootElement.TryGetProperty(SiteOptions.SectionName, out var site))
        {
            return new SiteOptions();
        }

        return new SiteOptions
        {
            Name = ReadString(site, nameof(SiteOptions.Name)),
            PublicBaseUrl = ReadString(site, nameof(SiteOptions.PublicBaseUrl)),
            MediaBaseUrl = ReadString(site, nameof(SiteOptions.MediaBaseUrl)),
            BlogBaseUrl = ReadString(site, nameof(SiteOptions.BlogBaseUrl)),
            PortfolioBaseUrl = ReadString(site, nameof(SiteOptions.PortfolioBaseUrl)),
            Kind = ReadString(site, nameof(SiteOptions.Kind)),
            FooterHomeLabel = ReadString(site, nameof(SiteOptions.FooterHomeLabel))
        };
    }

    private static string ReadString(System.Text.Json.JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == System.Text.Json.JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : string.Empty;

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Papyrus.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate the repository root.");
    }
}
