using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Features;
using System.Threading.RateLimiting;
using Papyrus.Api.Admin;
using Papyrus.Api.Admin.Authentication;
using Papyrus.Api.Admin.Media;
using Papyrus.Infrastructure;
using Papyrus.Application;
using Papyrus.ServiceDefaults;
using Papyrus.ServiceDefaults.Telemetry;

var builder = WebApplication.CreateBuilder(args);

var configuredAdminApiKeys = builder.Configuration
    .GetSection(AdminApiKeyOptions.SectionName)
    .Get<AdminApiKeyOptions>()?
    .GetConfiguredKeys()
    .ToArray() ?? [];
if (!builder.Environment.IsDevelopment()
    && (configuredAdminApiKeys.Length == 0
        || configuredAdminApiKeys.Any(key => string.Equals(key, "change-me", StringComparison.OrdinalIgnoreCase))))
{
    throw new InvalidOperationException(
        $"{AdminApiKeyOptions.SectionName}:ApiKeys must be supplied in production and must not use the default placeholder.");
}

builder.AddServiceDefaults();
builder.AddPapyrusInfrastructure();
builder.Services.AddPapyrusApplication();
builder.Services.AddAdminApiAuthentication(builder.Configuration);
builder.Services.Configure<MediaOptions>(builder.Configuration.GetSection(MediaOptions.SectionName));
builder.Services.AddScoped<LocalMediaService>();
builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 5 * 1024 * 1024);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("admin-api", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;
});
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<AdminOpenApiDocumentTransformer>());

var app = builder.Build();

app.MapOpenApi();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            title: "An unexpected error occurred.",
            extensions: new Dictionary<string, object?>
            {
                ["correlationId"] = context.TraceIdentifier
            }).ExecuteAsync(context);
    }));
}

if (!app.Configuration.GetValue<bool>("Papyrus:SkipDatabaseInitialization"))
{
    await app.InitializeDatabaseAsync();
}

app.UsePapyrusSecurityHeaders();
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}
app.UsePapyrusTelemetry();
var mediaOptions = app.Services.GetRequiredService<IOptions<MediaOptions>>().Value;
var mediaRoot = MediaRootResolver.Resolve(app.Environment, mediaOptions);
Directory.CreateDirectory(mediaRoot);
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments(mediaOptions.RequestPath, StringComparison.OrdinalIgnoreCase))
    {
        var segments = context.Request.Path.Value!.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var tenantContext = context.RequestServices.GetRequiredService<Papyrus.Infrastructure.MultiTenancy.ITenantContext>();
        if (segments.Length != 3 || !string.Equals(segments[1], tenantContext.TenantId, StringComparison.Ordinal)
            || !System.Text.RegularExpressions.Regex.IsMatch(segments[2], "^[0-9a-f]{32}\\.(jpg|jpeg|png|gif|webp)$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
    }

    await next();
});
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(mediaRoot),
    RequestPath = mediaOptions.RequestPath,
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.ContentDisposition = "inline";
        context.Context.Response.Headers["X-Download-Options"] = "noopen";
    }
});

// Routing is declared explicitly so endpoint selection happens before
// authentication: the API-key scheme uses endpoint metadata to recognise
// anonymous endpoints such as the health probes.
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AdminAuditMiddleware>();
app.UseRateLimiter();
app.MapDefaultEndpoints();
app.MapAdminApi();

var adminSpaRoot = new[]
{
    Path.Combine(app.Environment.ContentRootPath, "wwwroot", "admin"),
    Path.Combine(app.Environment.ContentRootPath, "wwwroot"),
    Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "Papyrus.Admin.UI", "dist"))
}.FirstOrDefault(path => File.Exists(Path.Combine(path, "index.html")));
if (adminSpaRoot is not null)
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(adminSpaRoot)
    });

    app.MapGet("/", () => Results.File(
        File.OpenRead(Path.Combine(adminSpaRoot, "index.html")),
        "text/html; charset=utf-8")).ExcludeFromDescription();
    app.MapGet("/{**path:nonfile}", () => Results.File(
        File.OpenRead(Path.Combine(adminSpaRoot, "index.html")),
        "text/html; charset=utf-8")).ExcludeFromDescription();
}

app.Run();
