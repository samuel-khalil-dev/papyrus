using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Papyrus.Api.Admin;
using Papyrus.Api.Admin.Authentication;

namespace Papyrus.Tests;

/// Covers the Admin API rate limit as behaviour. Until now the policy was only
/// reachable from the k6 load script, which asserts thresholds over a long run;
/// nothing proved that an over-budget request is actually rejected, or that the
/// first 120 requests in a window succeed.
///
/// The test drives the real registration, so a change to the permit limit or the
/// window is a change to what is asserted here rather than a silent divergence.
[Collection(HostCompositionCollection.Name)]
public sealed class AdminRateLimitingTests : IAsyncLifetime
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        _app = CompositionTestHost.Build(databaseName: Guid.NewGuid().ToString());
        _app.UseRouting();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.UseRateLimiter();
        _app.MapAdminApi();
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
    public async Task Requests_within_the_permit_limit_are_served()
    {
        for (var index = 0; index < 10; index++)
        {
            var response = await GetAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            response.Dispose();
        }
    }

    [Fact]
    public async Task The_full_permit_limit_is_served_and_the_next_request_is_rejected()
    {
        var statuses = new List<HttpStatusCode>();
        // One request past the limit, so a reset window mid-run can only make the
        // rejection later, never earlier.
        for (var index = 0; index <= AdminRateLimitingExtensions.PermitLimit; index++)
        {
            var response = await GetAsync();
            statuses.Add(response.StatusCode);
            response.Dispose();
        }

        var firstRejection = statuses.FindIndex(status => status == HttpStatusCode.TooManyRequests);

        Assert.True(
            firstRejection >= AdminRateLimitingExtensions.PermitLimit,
            $"Expected the first rejection at or after request {AdminRateLimitingExtensions.PermitLimit + 1}, "
            + $"but got {firstRejection}. Sequence: {string.Join(',', statuses)}");

        // The rejection is the tail of the run: the limit is not being hit early.
        Assert.All(statuses.Take(AdminRateLimitingExtensions.PermitLimit), status =>
            Assert.Equal(HttpStatusCode.OK, status));
    }

    [Fact]
    public async Task A_rejected_request_does_not_leak_the_request_detail()
    {
        for (var index = 0; index <= AdminRateLimitingExtensions.PermitLimit; index++)
        {
            (await GetAsync()).Dispose();
        }

        var body = await (await GetAsync()).Content.ReadAsStringAsync();

        Assert.DoesNotContain("Exception", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("   at ", body, StringComparison.Ordinal);
    }

    private HttpRequestMessage AuthorizedRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/blog-posts");
        request.Headers.Add("X-Admin-Api-Key", CompositionTestHost.ValidApiKey);
        return request;
    }

    private Task<HttpResponseMessage> GetAsync() => _client.SendAsync(AuthorizedRequest());
}
