using Papyrus.Application;
using Papyrus.Infrastructure;
using Papyrus.ServiceDefaults;
using Papyrus.Web.Shared;
using Papyrus.Web.Shared.Syndication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Papyrus.ServiceDefaults.Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddPapyrusInfrastructure();
builder.Services.AddPapyrusApplication();
builder.Services.AddPapyrusWeb(builder.Configuration);

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UsePapyrusSecurityHeaders();
app.UseHttpsRedirection();
app.UsePapyrusTelemetry();
app.MapDefaultEndpoints();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();
app.MapGet("/feed.xml", async (PapyrusDbContext db, IOptions<SiteOptions> siteOptions, HttpRequest request, CancellationToken cancellationToken) =>
{
    var site = siteOptions.Value;
    var baseUrl = SiteUrl.ResolvePublicBase(site.PublicBaseUrl, request);
    var now = DateTimeOffset.UtcNow;
    var posts = await db.BlogPosts.AsNoTracking()
        .Where(post => post.IsPublished && (post.PublishedAt == null || post.PublishedAt <= now))
        .OrderByDescending(post => post.PublishedAt ?? post.CreatedAt)
        .Take(100)
        .ToListAsync(cancellationToken);
    var items = posts.Select(post => new SiteFeedItem(
        post.Title,
        SiteUrl.Combine(baseUrl, $"/Post/{post.Slug}"),
        post.Summary,
        post.PublishedAt ?? post.CreatedAt,
        $"blog-post-{post.Id}"))
        .ToList();

    var xml = SiteFeed.CreateRss(site.Name, baseUrl ?? "/", "Latest published writing from Papyrus.", items);
    return Results.Text(xml, "application/rss+xml", System.Text.Encoding.UTF8);
});
app.MapGet("/sitemap.xml", async (PapyrusDbContext db, IOptions<SiteOptions> siteOptions, HttpRequest request, CancellationToken cancellationToken) =>
{
    var site = siteOptions.Value;
    var baseUrl = SiteUrl.ResolvePublicBase(site.PublicBaseUrl, request);
    var now = DateTimeOffset.UtcNow;
    var posts = await db.BlogPosts.AsNoTracking()
        .Where(post => post.IsPublished && (post.PublishedAt == null || post.PublishedAt <= now))
        .Select(post => new { post.Slug, LastModified = post.UpdatedAt ?? post.PublishedAt ?? post.CreatedAt })
        .Take(5000)
        .ToListAsync(cancellationToken);
    var entries = new List<SitemapEntry>
    {
        new(SiteUrl.Combine(baseUrl, "/"), now),
        new(SiteUrl.Combine(baseUrl, "/About"), now)
    };
    entries.AddRange(posts.Select(post => new SitemapEntry(SiteUrl.Combine(baseUrl, $"/Post/{post.Slug}"), post.LastModified)));
    return Results.Text(SiteFeed.CreateSitemap(entries), "application/xml", System.Text.Encoding.UTF8);
});
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
