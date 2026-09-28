using System.Security.Claims;
using System.Diagnostics;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Papyrus.ServiceDefaults.Telemetry;

namespace Papyrus.Api.Admin.Authentication;

internal sealed class ApiKeyAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IAuditEventSink auditEvents)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "ApiKey";

    // Clients must send the configured key in this header.
    public const string HeaderName = AdminApiKey.HeaderName;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Endpoints that are not subject to authorization — the health and
        // aliveness probes the orchestrator polls, which are mapped without a
        // policy — are not authentication attempts. Returning NoResult lets the
        // authorization middleware short-circuit them, and skipping the audit
        // record keeps a probe every few seconds from filling the security trail
        // with 401 failures that read like attacks.
        if (IsOutsideAuthorization(Context.GetEndpoint()))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var configuredKeys = Context.RequestServices
            .GetRequiredService<IOptionsMonitor<AdminApiKeyOptions>>()
            .CurrentValue
            .GetConfiguredKeys()
            .ToArray();
        if (configuredKeys.Length == 0)
        {
            RecordAuthEvent("configuration-missing");
            return Task.FromResult(AuthenticateResult.Fail("Admin API key is not configured."));
        }

        if (!Request.Headers.TryGetValue(HeaderName, out var providedApiKey))
        {
            RecordAuthEvent("missing-key");
            return Task.FromResult(AuthenticateResult.Fail("Missing admin API key."));
        }

        if (!AdminApiKey.MatchesAny(configuredKeys, providedApiKey.ToString()))
        {
            RecordAuthEvent("invalid-key");
            return Task.FromResult(AuthenticateResult.Fail("Invalid admin API key."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "admin"),
            new Claim(ClaimTypes.Name, "admin")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        RecordAuthEvent("success");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    // An endpoint is outside authorization when it opts out with AllowAnonymous, or
    // when no authorization requirement was ever applied to it.
    private static bool IsOutsideAuthorization(Endpoint? endpoint)
    {
        if (endpoint is null)
        {
            return false;
        }

        return endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null
            || endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>().Count == 0;
    }

    private void RecordAuthEvent(string outcome) => auditEvents.Record(new AuditEvent(
        Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? "main",
        outcome == "success" ? "admin" : "anonymous",
        "auth.api_key",
        "admin-api",
        Context.TraceIdentifier,
        outcome == "success" ? "success" : "failure",
        outcome == "success" ? StatusCodes.Status200OK : StatusCodes.Status401Unauthorized,
        Activity.Current?.TraceId.ToString(),
        DateTimeOffset.UtcNow));
}
