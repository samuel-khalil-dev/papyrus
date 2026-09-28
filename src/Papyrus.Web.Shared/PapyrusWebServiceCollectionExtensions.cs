using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Papyrus.Web.Shared;

public static class PapyrusWebServiceCollectionExtensions
{
    public static IServiceCollection AddPapyrusWeb(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SiteOptions>(configuration.GetSection(SiteOptions.SectionName));
        return services;
    }

    /// <summary>
    /// Enforces the M1 routing truth for a public host: validates that the
    /// configured <see cref="SiteOptions"/> match the site this process actually
    /// serves, and derives the <c>AllowedHosts</c> list from them.
    /// <para>
    /// <c>AddHostFiltering</c> is already called by
    /// <c>ConfigureWebHostDefaults</c>, so the middleware is present in every
    /// pipeline; the only reason a foreign Host header was served is that the
    /// list was <c>"*"</c>. Overriding it here is what makes the routing
    /// decision real.
    /// </para>
    /// </summary>
    /// <param name="builder">The host builder, before it is built.</param>
    /// <param name="expectedKind">The site this process serves: <c>blog</c> or <c>portfolio</c>.</param>
    public static void UsePapyrusSiteHostPolicy(this WebApplicationBuilder builder, string expectedKind)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var site = builder.Configuration
            .GetSection(SiteOptions.SectionName)
            .Get<SiteOptions>() ?? new SiteOptions();

        // Fail fast: a host that advertises the other site's canonical URLs, or
        // that runs under the wrong Site:Kind, would silently publish the wrong
        // site. This mirrors the Admin API key check and MediaRootResolver.
        SiteHostValidator.Validate(site, expectedKind);

        var allowedHosts = SiteHostValidator.ResolveAllowedHosts(site);
        if (allowedHosts.Length == 1 && allowedHosts[0] == "*")
        {
            // No canonical host is configured, so there is nothing to enforce
            // and the existing wide-open behaviour is preserved.
            return;
        }

        builder.Services.Configure<HostFilteringOptions>(options => options.AllowedHosts = allowedHosts);
    }
}
