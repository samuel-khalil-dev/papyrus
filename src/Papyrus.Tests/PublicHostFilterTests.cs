using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Papyrus.Web.Shared;

namespace Papyrus.Tests;

/// <summary>
/// Proves the M1 host filter through a real request, because the 400 on a
/// foreign Host header is the whole security deliverable of the routing-truth
/// work and a source-text wiring assertion cannot prove it.
/// <para>
/// The Blog host is reconstructed here rather than launched, because the tests
/// must run without a database and without a fixed port.
/// </para>
/// </summary>
public sealed class PublicHostFilterTests
{
    private static WebApplication BuildBlog(string publicBaseUrl)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Site:Name"] = "Papyrus Blog",
            ["Site:Kind"] = "blog",
            ["Site:PublicBaseUrl"] = publicBaseUrl,
            ["Site:BlogBaseUrl"] = "https://blog.example.com",
            ["Site:PortfolioBaseUrl"] = "https://example.com"
        });
        builder.Services.AddPapyrusWeb(builder.Configuration);
        builder.UsePapyrusSiteHostPolicy(SiteHostValidator.BlogKind);
        return builder.Build();
    }

    [Theory]
    [InlineData("evil.example.com")]
    [InlineData("example.com")]
    [InlineData("blog.example.com.evil.test")]
    [InlineData("notblog.example.com")]
    public async Task A_foreign_host_header_is_refused_with_400(string host)
    {
        await using var app = BuildBlog("https://blog.example.com");
        await app.StartAsync();
        var client = app.GetTestClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Host = host;

        var response = await client.SendAsync(request);

        // 400 is what HostFilteringMiddleware returns; a 404 would mean the host
        // was allowed through and only the route was missing, which would not
        // prove the filter works.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("blog.example.com")]
    [InlineData("BLOG.EXAMPLE.COM")]
    [InlineData("127.0.0.1")]
    [InlineData("localhost")]
    public async Task The_canonical_and_loopback_hosts_are_not_rejected_by_the_filter(string host)
    {
        await using var app = BuildBlog("https://blog.example.com");
        await app.StartAsync();
        var client = app.GetTestClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Host = host;

        var response = await client.SendAsync(request);

        // The route is not mapped in this harness, so anything other than 400
        // proves the host passed the filter. A 500 would still indicate the
        // request reached the application, but the registered health check makes
        // 503 the expected outcome and keeps the assertion unambiguous.
        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task An_unconfigured_public_base_url_keeps_serving_every_host()
    {
        // The documented fallback: with no canonical host there is nothing to
        // enforce, and a wide-open allowlist is preferred over refusing every
        // request. This asserts the current behaviour so a future tightening is a
        // deliberate change rather than an accident.
        await using var app = BuildBlog(string.Empty);
        await app.StartAsync();
        var client = app.GetTestClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Host = "anything.example.test";

        var response = await client.SendAsync(request);

        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public void A_malformed_public_base_url_disables_enforcement_silently()
    {
        // Documented limitation rather than intended safety valve: the most likely
        // real trigger is a mistyped BLOG_PUBLIC_BASE_URL, which Compose's
        // non-empty check does not catch. Asserted so the behaviour is explicit.
        var site = new SiteOptions { PublicBaseUrl = "https//blog.example.com" };

        Assert.Equal(["*"], SiteHostValidator.ResolveAllowedHosts(site));
    }

    [Fact]
    public void A_startup_mismatch_between_kind_and_site_fails_loudly()
    {
        // The contrast with the case above: a wrong Kind refuses to start, while a
        // malformed URL silently disables enforcement. Both behaviours are current.
        var site = new SiteOptions
        {
            Kind = "portfolio",
            PublicBaseUrl = "https://blog.example.com",
            BlogBaseUrl = "https://blog.example.com"
        };

        Assert.Throws<InvalidOperationException>(
            () => SiteHostValidator.Validate(site, SiteHostValidator.BlogKind));
    }
}
