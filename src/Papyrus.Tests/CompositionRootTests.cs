using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Papyrus.Api.Admin;
using Papyrus.ServiceDefaults;
using Papyrus.ServiceDefaults.Telemetry;

namespace Papyrus.Tests;

/// Covers the Admin API composition root: the real API-key scheme, the security
/// headers every host sets, and the health endpoints staying anonymous. The
/// endpoint-level behaviour of the API itself is covered by AdminApiTests, which
/// deliberately substitutes the authentication scheme; these tests are the
/// counterweight, because they do not.
[Collection(HostCompositionCollection.Name)]
public sealed class CompositionRootTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _app = CompositionTestHost.Build(databaseName: Guid.NewGuid().ToString());
        _app.UsePapyrusSecurityHeaders();
        _app.UsePapyrusTelemetry();
        _app.UseRouting();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapDefaultEndpoints();
        _app.MapAdminApi();
        _app.MapGet("/probe", () => "ok");
        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    [Fact]
    public async Task Missing_api_key_is_rejected_and_audited_as_an_anonymous_failure()
    {
        var response = await _client.GetAsync("/api/v1/blog-posts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var audit = Assert.Single(CompositionTestHost.AuditSink(_app.Services).Events);
        Assert.Equal("auth.api_key", audit.Action);
        Assert.Equal("anonymous", audit.Actor);
        Assert.Equal("failure", audit.Outcome);
        Assert.Equal(401, audit.StatusCode);
    }

    [Fact]
    public async Task Wrong_api_key_is_rejected_and_audited_as_an_invalid_key()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/blog-posts");
        request.Headers.Add("X-Admin-Api-Key", "not-the-configured-key");

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var audit = Assert.Single(CompositionTestHost.AuditSink(_app.Services).Events);
        Assert.Equal("anonymous", audit.Actor);
        Assert.Equal("failure", audit.Outcome);
    }

    [Fact]
    public async Task Configured_api_key_is_accepted_and_audited_as_an_admin_identity()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/blog-posts");
        request.Headers.Add("X-Admin-Api-Key", CompositionTestHost.ValidApiKey);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var audit = Assert.Single(CompositionTestHost.AuditSink(_app.Services).Events);
        Assert.Equal("admin", audit.Actor);
        Assert.Equal("success", audit.Outcome);
        Assert.Equal(200, audit.StatusCode);
    }

    [Fact]
    public async Task Health_endpoints_stay_anonymous_for_orchestrators()
    {
        using var health = await _client.GetAsync("/health");
        using var alive = await _client.GetAsync("/alive");

        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
        Assert.Equal("Healthy", await health.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, alive.StatusCode);

        // Probes are unauthenticated, so they must not appear in the audit trail
        // as authentication attempts either.
        Assert.Empty(CompositionTestHost.AuditSink(_app.Services).Events);
    }

    [Fact]
    public async Task Security_headers_are_applied_to_authenticated_and_anonymous_responses()
    {
        using var anonymous = await _client.GetAsync("/health");
        using var authorized = await _client.SendAsync(AuthorizedRequest("/api/v1/blog-posts"));

        Assert.Equal(HttpStatusCode.OK, authorized.StatusCode);

        foreach (var response in new[] { anonymous, authorized })
        {
            var csp = Header(response, "Content-Security-Policy");
            Assert.Contains("default-src 'self'", csp, StringComparison.Ordinal);
            Assert.Contains("base-uri 'self'", csp, StringComparison.Ordinal);
            Assert.Contains("frame-ancestors 'none'", csp, StringComparison.Ordinal);
            Assert.Contains("form-action 'self'", csp, StringComparison.Ordinal);
            Assert.Contains("script-src 'self'", csp, StringComparison.Ordinal);
            Assert.Contains("style-src 'self' https://fonts.googleapis.com", csp, StringComparison.Ordinal);
            Assert.Contains("font-src 'self' https://fonts.gstatic.com", csp, StringComparison.Ordinal);

            Assert.Equal("nosniff", Header(response, "X-Content-Type-Options"));
            Assert.Equal("DENY", Header(response, "X-Frame-Options"));
            Assert.Equal("strict-origin-when-cross-origin", Header(response, "Referrer-Policy"));
            Assert.Contains("camera=()", Header(response, "Permissions-Policy"), StringComparison.Ordinal);
            Assert.Contains("geolocation=()", Header(response, "Permissions-Policy"), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Security_headers_are_applied_even_when_a_request_is_rejected()
    {
        var response = await _client.GetAsync("/api/v1/blog-posts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("frame-ancestors 'none'", Header(response, "Content-Security-Policy"), StringComparison.Ordinal);
    }

    private static HttpRequestMessage AuthorizedRequest(string path)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("X-Admin-Api-Key", CompositionTestHost.ValidApiKey);
        return request;
    }

    private static string Header(HttpResponseMessage response, string name)
    {
        Assert.True(response.Headers.TryGetValues(name, out var values), $"Expected header '{name}' to be present.");
        return Assert.Single(values);
    }
}
