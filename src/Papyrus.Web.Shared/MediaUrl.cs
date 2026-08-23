namespace Papyrus.Web.Shared;

public static class MediaUrl
{
    public static string? Resolve(string? baseUrl, string? coverImageUrl, string? fallbackBaseUrl = null)
    {
        if (string.IsNullOrWhiteSpace(coverImageUrl))
        {
            return null;
        }

        var value = coverImageUrl.Trim();
        if (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return value;
        }

        var absoluteBaseUrl = string.IsNullOrWhiteSpace(baseUrl) ? fallbackBaseUrl : baseUrl;
        if (!string.IsNullOrWhiteSpace(absoluteBaseUrl))
        {
            var path = value.StartsWith('/') ? value : $"/{value}";
            return $"{absoluteBaseUrl.TrimEnd('/')}{path}";
        }

        return value;
    }
}
