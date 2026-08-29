using System.Security.Cryptography;
using System.Text;

namespace Papyrus.Api.Admin.Authentication;

public static class AdminApiKey
{
    public const string HeaderName = "X-Admin-Api-Key";
    public const string ConfigurationKey = "AdminApi:ApiKey";
    public const string KeysConfigurationKey = "AdminApi:ApiKeys";

    public static bool Matches(string? expectedApiKey, string? providedApiKey)
    {
        if (string.IsNullOrWhiteSpace(expectedApiKey) || providedApiKey is null)
        {
            return false;
        }

        var expectedBytes = Encoding.UTF8.GetBytes(expectedApiKey);
        var providedBytes = Encoding.UTF8.GetBytes(providedApiKey);

        return expectedBytes.Length == providedBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    public static bool MatchesAny(IEnumerable<string> expectedApiKeys, string? providedApiKey)
    {
        var matches = false;
        foreach (var expectedApiKey in expectedApiKeys)
        {
            // Do not short-circuit: every configured candidate gets the same
            // fixed-time comparison regardless of its position in the set.
            matches |= Matches(expectedApiKey, providedApiKey);
        }

        return matches;
    }

}
