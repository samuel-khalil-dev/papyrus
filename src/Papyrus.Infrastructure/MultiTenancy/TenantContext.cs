using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Papyrus.Infrastructure.MultiTenancy;

public sealed class TenantContext : ITenantContext
{
    public const string HeaderName = "X-Tenant-Id";
    private static readonly Regex TenantIdPattern = new("^[a-z0-9][a-z0-9-]{0,62}$", RegexOptions.Compiled);

    public TenantContext(IHttpContextAccessor httpContextAccessor, IOptions<TenantOptions> options)
    {
        var requestedTenant = httpContextAccessor.HttpContext?.Request.Headers[HeaderName].FirstOrDefault();
        var configuredTenant = string.IsNullOrWhiteSpace(options.Value.DefaultTenantId) ? "main" : options.Value.DefaultTenantId;
        TenantId = NormalizeAndValidate(string.IsNullOrWhiteSpace(requestedTenant) ? configuredTenant : requestedTenant);
    }

    public TenantContext(string tenantId) => TenantId = NormalizeAndValidate(tenantId);

    public string TenantId { get; }

    private static string NormalizeAndValidate(string tenantId)
    {
        var normalized = tenantId.Trim().ToLowerInvariant();
        if (!TenantIdPattern.IsMatch(normalized))
        {
            throw new InvalidOperationException($"Tenant id '{tenantId}' is invalid.");
        }

        return normalized;
    }
}
