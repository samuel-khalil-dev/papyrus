using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Papyrus.Api.Admin.Authentication;
using Papyrus.Api.Admin.Media;
using Papyrus.Application;
using Papyrus.Application.Content;
using Papyrus.Infrastructure;
using Papyrus.Infrastructure.Content;
using Papyrus.Infrastructure.Interceptors;
using Papyrus.Infrastructure.MultiTenancy;
using Papyrus.ServiceDefaults;
using Papyrus.ServiceDefaults.Telemetry;

namespace Papyrus.Tests;

/// Builds a host that runs the real cross-host pipeline — service defaults,
/// security headers, the real API-key authentication scheme, and the real
/// endpoint mapping — while substituting only the three things a test cannot
/// supply: the database provider, the audit sink, and the media directory.
/// Nothing here re-implements production behaviour; every assertion in
/// CompositionRootTests and ServiceDefaultsPipelineTests runs against these
/// registrations.
internal static class CompositionTestHost
{
    public const string ValidApiKey = "composition-test-key";

    public static WebApplication Build(string databaseName, string? tenantId = "main")
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Environment.EnvironmentName = "Test";
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["AdminApi:ApiKeys:0"] = ValidApiKey,
            ["Site:PublicBaseUrl"] = "https://papyrus.test",
        });

        builder.AddServiceDefaults();

        builder.Services.AddDbContext<PapyrusDbContext>(options =>
            options.UseInMemoryDatabase(databaseName)
                .AddInterceptors(new PapyrusSaveChangesInterceptor(new DefaultAuditActor(), TimeProvider.System)));
        builder.Services.AddPapyrusApplication();
        builder.Services.AddScoped<IPublicContentStore, EfPublicContentStore>();
        builder.Services.AddScoped<IContentCommandStore, EfContentCommandStore>();
        builder.Services.AddScoped<IContentQueryStore, EfContentQueryStore>();
        builder.Services.AddScoped<ITaxonomyCommandStore, EfTaxonomyCommandStore>();
        builder.Services.AddScoped<IContentLifecycleStore, EfContentLifecycleStore>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<ITenantContext>(_ => new TenantContext(tenantId!));
        // Singleton, matching the production registration lifetime, so the handler and
        // the test assertions resolve the same instance.
        builder.Services.AddSingleton<IAuditEventSink, RecordingAuditEventSink>();
        builder.Services.Configure<MediaOptions>(options =>
        {
            options.RootPath = Path.Combine(Path.GetTempPath(), "papyrus-composition-tests", Guid.NewGuid().ToString("N"));
            options.RequestPath = "/media";
        });
        builder.Services.AddScoped<LocalMediaService>();

        // The production scheme, not a stand-in: the point of these tests is to
        // exercise ApiKeyAuthenticationHandler itself.
        builder.Services.AddAdminApiAuthentication(builder.Configuration);
        builder.Services.AddAdminApiRateLimiting();

        return builder.Build();
    }

    public static RecordingAuditEventSink AuditSink(IServiceProvider services) =>
        (RecordingAuditEventSink)services.GetRequiredService<IAuditEventSink>();

    public sealed class RecordingAuditEventSink : IAuditEventSink
    {
        public ConcurrentQueue<AuditEvent> Events { get; } = new();

        public void Record(AuditEvent auditEvent) => Events.Enqueue(auditEvent);
    }
}

/// Serialises the tests that build a host, because AddServiceDefaults installs a
/// process-wide OpenTelemetry provider and ActivitySource listeners. Running them
/// concurrently would let one host's provider observe another test's activities.
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class HostCompositionCollection
{
    public const string Name = "host-composition";
}
