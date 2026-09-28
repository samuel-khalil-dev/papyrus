namespace Papyrus.Web.Shared;

/// <summary>
/// Derives the host allowlist and validates the public-host identity of a
/// deployment.
/// <para>
/// M1 makes the "root domain is Portfolio, subdomain is Blog" routing decision
/// real. The configuration already existed; nothing enforced it, because
/// <c>AllowedHosts</c> was <c>"*"</c> in both public hosts, so any Host header
/// was served. This type supplies the two enforcement halves: a fail-fast
/// startup check that a host cannot advertise the other site's canonical URLs
/// or run under the wrong <see cref="SiteOptions.Kind"/>, and the derived
/// <c>AllowedHosts</c> list that rejects every other host with a 400.
/// </para>
/// <para>
/// Loopback is always permitted. The Aspire AppHost, the CI workflows, the
/// release verification, and the local <c>dotnet run</c> path all probe the
/// hosts over <c>127.0.0.1</c> while the configured public base URLs point at
/// real domains, so an allowlist without the loopback literals would return 400
/// for every health check. Loopback is not attacker-controlled in a deployed
/// edge topology.
/// </para>
/// </summary>
public static class SiteHostValidator
{
    public const string BlogKind = "blog";

    public const string PortfolioKind = "portfolio";

    /// <summary>
    /// The hosts always allowed regardless of configuration, because the
    /// orchestrator, the build pipeline, and the local developer all reach the
    /// public hosts over loopback.
    /// </summary>
    public static readonly string[] LoopbackHosts = ["localhost", "127.0.0.1", "[::1]"];

    /// <summary>
    /// The bare host of the configured public base URL, or null when none is
    /// configured or it is not a parseable absolute URL.
    /// </summary>
    public static string? ResolveCanonicalHost(SiteOptions site)
    {
        ArgumentNullException.ThrowIfNull(site);

        return HostOf(site.PublicBaseUrl);
    }

    /// <summary>
    /// Resolves the <c>AllowedHosts</c> entries for a public host: the bare host
    /// of the configured public base URL, plus the loopback literals.
    /// <para>
    /// Ports are stripped because <c>HostFilteringOptions.AllowedHosts</c> does
    /// not consider them, and a value carrying one never matches.
    /// </para>
    /// <para>
    /// Returns a single <c>"*"</c> only when no base URL is configured at all,
    /// which preserves the pre-M1 behaviour for an entirely unset deployment
    /// rather than rejecting every request.
    /// </para>
    /// </summary>
    public static string[] ResolveAllowedHosts(SiteOptions site)
    {
        ArgumentNullException.ThrowIfNull(site);

        var configured = HostOf(site.PublicBaseUrl);
        if (configured is null)
        {
            return ["*"];
        }

        var hosts = new List<string>(LoopbackHosts.Length + 1) { configured };
        hosts.AddRange(LoopbackHosts);
        return hosts.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    /// <summary>
    /// Throws when a public host is configured in a way that would make the
    /// routing decision false, naming the offending key so the operator can fix
    /// the deployment.
    /// <para>
    /// Cross-host distinctness is deliberately not checked here: a process
    /// cannot see the other public host's configuration, so that rule is
    /// enforced by a repository test over both hosts' settings rather than at
    /// startup. An empty public base URL is also left legal, because
    /// <see cref="SiteUrl.ResolvePublicBase"/> already has a safe
    /// relative-URL fallback for it.
    /// </para>
    /// </summary>
    public static void Validate(SiteOptions site, string expectedKind)
    {
        ArgumentNullException.ThrowIfNull(site);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedKind);

        if (!string.Equals(site.Kind, expectedKind, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Site:Kind is '{site.Kind}' but this host serves the {expectedKind} site, and the shared layout "
                + "and About content branch on it. Set Site:Kind to the value this host actually serves.");
        }

        var roleKey = RoleBaseUrlKey(expectedKind);
        var publicHost = HostOf(site.PublicBaseUrl);
        var roleHost = HostOf(RoleBaseUrl(site, expectedKind));
        if (publicHost is null)
        {
            return;
        }

        if (roleHost is not null
            && !string.Equals(publicHost, roleHost, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Site:PublicBaseUrl is '{site.PublicBaseUrl}' (host '{publicHost}') but Site:{roleKey} is "
                + $"'{RoleBaseUrl(site, expectedKind)}' (host '{roleHost}'). A {expectedKind} host must advertise its "
                + "own canonical URLs; the two settings disagree about which host this is.");
        }
    }

    private static string RoleBaseUrlKey(string kind) =>
        string.Equals(kind, PortfolioKind, StringComparison.OrdinalIgnoreCase) ? "PortfolioBaseUrl" : "BlogBaseUrl";

    private static string RoleBaseUrl(SiteOptions site, string kind) =>
        string.Equals(kind, PortfolioKind, StringComparison.OrdinalIgnoreCase) ? site.PortfolioBaseUrl : site.BlogBaseUrl;

    /// <summary>
    /// Extracts the bare host from an absolute base URL, returning null when the
    /// value is absent or not a parseable absolute URL. A relative or malformed
    /// value yields null rather than an exception, so a bad deployment degrades
    /// to "no canonical host configured" instead of crashing startup twice.
    /// </summary>
    private static string? HostOf(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        return Uri.TryCreate(baseUrl.Trim(), UriKind.Absolute, out var uri) && !string.IsNullOrEmpty(uri.Host)
            ? uri.Host
            : null;
    }
}
