namespace Papyrus.Api.Admin.Authentication;

/// <summary>
/// Configuration for the Admin API key set.
/// </summary>
/// <remarks>
/// <see cref="ApiKey"/> is retained as a compatibility alias for existing
/// deployments. New deployments should use <see cref="ApiKeys"/> so a new key
/// can be added before the previous key is removed.
/// </remarks>
public sealed class AdminApiKeyOptions
{
    public const string SectionName = "AdminApi";

    public string? ApiKey { get; set; }

    public string[] ApiKeys { get; set; } = [];

    public IEnumerable<string> GetConfiguredKeys()
    {
        if (!string.IsNullOrWhiteSpace(ApiKey))
        {
            yield return ApiKey;
        }

        foreach (var apiKey in ApiKeys)
        {
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                yield return apiKey;
            }
        }
    }
}
