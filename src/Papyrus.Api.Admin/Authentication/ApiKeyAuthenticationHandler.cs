using System.Security.Claims;
using System.Diagnostics;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
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
