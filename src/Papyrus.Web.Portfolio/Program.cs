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
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UsePapyrusTelemetry();
app.UseRouting();
app.UseAuthorization();
app.MapDefaultEndpoints();
app.MapStaticAssets();
app.MapGet("/feed.xml", async (PapyrusDbContext db, IOptions<SiteOptions> siteOptions, HttpRequest request, CancellationToken cancellationToken) =>
{
    var site = siteOptions.Value;
    var baseUrl = SiteUrl.ResolvePublicBase(site.PublicBaseUrl, request);
    var now = DateTimeOffset.UtcNow;
    var projects = await db.Projects.AsNoTracking()
        .Where(project => project.IsPublished && (project.PublishedAt == null || project.PublishedAt <= now))
        .OrderByDescending(project => project.PublishedAt ?? project.CreatedAt)
        .Take(100)
        .ToListAsync(cancellationToken);
    var items = projects.Select(project => new SiteFeedItem(
        project.Title,
        SiteUrl.Combine(baseUrl, $"/Project/{project.Slug}"),
        project.Summary,
        project.PublishedAt ?? project.CreatedAt,
        $"project-{project.Id}"))
        .ToList();

    var xml = SiteFeed.CreateRss(site.Name, baseUrl ?? "/", "Latest published projects from Papyrus.", items);
    return Results.Text(xml, "application/rss+xml", System.Text.Encoding.UTF8);
});
app.MapGet("/sitemap.xml", async (PapyrusDbContext db, IOptions<SiteOptions> siteOptions, HttpRequest request, CancellationToken cancellationToken) =>
{
    var site = siteOptions.Value;
    var baseUrl = SiteUrl.ResolvePublicBase(site.PublicBaseUrl, request);
    var now = DateTimeOffset.UtcNow;
    var projects = await db.Projects.AsNoTracking()
        .Where(project => project.IsPublished && (project.PublishedAt == null || project.PublishedAt <= now))
        .Select(project => new { project.Slug, LastModified = project.UpdatedAt ?? project.PublishedAt ?? project.CreatedAt })
        .Take(5000)
        .ToListAsync(cancellationToken);
    var entries = new List<SitemapEntry>
    {
        new(SiteUrl.Combine(baseUrl, "/"), now),
        new(SiteUrl.Combine(baseUrl, "/About"), now)
    };
    entries.AddRange(projects.Select(project => new SitemapEntry(SiteUrl.Combine(baseUrl, $"/Project/{project.Slug}"), project.LastModified)));
    return Results.Text(SiteFeed.CreateSitemap(entries), "application/xml", System.Text.Encoding.UTF8);
});
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
