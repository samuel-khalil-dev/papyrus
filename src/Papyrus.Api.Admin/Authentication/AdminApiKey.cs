using System.Security.Cryptography;
using System.Text;

namespace Papyrus.Api.Admin.Authentication;

public static class AdminApiKey
{
    public const string HeaderName = "X-Admin-Api-Key";
    public const string ConfigurationKey = "AdminApi:ApiKey";

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

}
