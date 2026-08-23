using Microsoft.AspNetCore.Http;

namespace Papyrus.Web.Shared;

public static class SiteUrl
{
    public static string Combine(string? baseUrl, string path)
    {
        var relative = path.StartsWith('/') ? path : $"/{path}";
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return relative;
        }

        return $"{baseUrl.TrimEnd('/')}{relative}";
    }

    public static string? ResolvePublicBase(string? configured, HttpRequest _)
    {
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return configured.TrimEnd('/');
        }

        // Never derive public absolute URLs from Request.Host. The host header is
        // attacker-controlled unless a trusted proxy/host policy has been applied.
        return null;
    }
}
