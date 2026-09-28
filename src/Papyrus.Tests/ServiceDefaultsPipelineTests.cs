using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Trace;
using Papyrus.ServiceDefaults;
using Papyrus.ServiceDefaults.Telemetry;
using Papyrus.ServiceDefaults.Telemetry.Redaction;

namespace Papyrus.Tests;

/// Covers the OpenTelemetry host wiring itself. TelemetryTests covers the sink,
/// redactor, sampling policy, and cardinality guard as units, all constructed by
/// hand with their own ActivityListener — which is precisely why a dropped
/// AddSource in the host wiring would still leave that suite green while Papyrus
/// spans stopped being produced in every deployed host. These tests go through
/// the real provider and the real exporter, so they fail on that regression.
[Collection(HostCompositionCollection.Name)]
public sealed class ServiceDefaultsPipelineTests
{
    [Fact]
    public async Task Papyrus_activity_source_spans_are_exported_by_the_configured_pipeline()
    {
        var exporter = new CollectingExporter();
        await using var host = await BuildHostAsync(exporter);

        using (var activity = TelemetryActivitySource.Start("Papyrus.Tests.Pipeline"))
        {
            activity?.SetTag("papyrus.probe", true);
        }

        ForceFlush(host);

        var exported = Assert.Single(exporter.ExportedItems);
        Assert.Equal("Papyrus.Tests.Pipeline", exported.DisplayName);
        Assert.Equal(TelemetryActivitySource.Name, exported.Source.Name);
        Assert.Equal(true, exported.GetTagItem("papyrus.probe"));
    }

    [Fact]
    public async Task Exported_spans_use_w3c_trace_ids()
    {
        var exporter = new CollectingExporter();
        await using var host = await BuildHostAsync(exporter);

        using (var activity = TelemetryActivitySource.Start("Papyrus.Tests.W3C"))
        {
            Assert.NotNull(activity);
        }

        ForceFlush(host);

        var exported = Assert.Single(exporter.ExportedItems);
        Assert.Equal(ActivityIdFormat.W3C, exported.IdFormat);
    }

    [Fact]
    public async Task Service_defaults_registers_the_papyrus_telemetry_services()
    {
        await using var host = await BuildHostAsync(new CollectingExporter());

        using var scope = host.Services.CreateScope();

        Assert.IsType<Redactor>(scope.ServiceProvider.GetRequiredService<Redactor>());
        Assert.IsType<CardinalityGuard>(scope.ServiceProvider.GetRequiredService<CardinalityGuard>());
        Assert.IsType<TelemetrySamplingPolicy>(scope.ServiceProvider.GetRequiredService<TelemetrySamplingPolicy>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<IAuditEventSink>());
    }

    [Fact]
    public async Task Request_middleware_emits_a_server_span_tagged_with_service_and_status()
    {
        var exporter = new CollectingExporter();
        await using var host = await BuildHostAsync(exporter, wireTelemetryMiddleware: true);
        using var client = host.GetTestClient();

        using var response = await client.GetAsync("/probe");

        Assert.Equal(200, (int)response.StatusCode);
        ForceFlush(host);

        var request = Assert.Single(exporter.ExportedItems, item => item.DisplayName == "Papyrus.Request");
        Assert.Equal(ActivityKind.Server, request.Kind);
        Assert.Equal(host.Environment.ApplicationName, request.GetTagItem("papyrus.service"));
        Assert.Equal(200, Assert.IsType<int>(request.GetTagItem("http.status_code")));
    }

    [Fact]
    public async Task Health_and_aliveness_requests_are_excluded_from_tracing()
    {
        // The filter is asserted directly rather than through a live request: the
        // ASP.NET Core instrumentation subscribes process-wide, so with several
        // hosts created in one test process a per-host exporter cannot reliably be
        // the one that receives the instrumentation span.
        Assert.True(HealthEndpointFilter.IsExcluded(new PathString("/health")));
        Assert.True(HealthEndpointFilter.IsExcluded(new PathString("/alive")));
        Assert.False(HealthEndpointFilter.IsExcluded(new PathString("/")));
        Assert.False(HealthEndpointFilter.IsExcluded(new PathString("/api/v1/blog-posts")));
        Assert.False(HealthEndpointFilter.IsExcluded(new PathString("/healthcheck")));
        Assert.False(HealthEndpointFilter.IsExcluded(new PathString("/aliveness-report")));

        await Task.CompletedTask;
    }

    private static async Task<WebApplication> BuildHostAsync(CollectingExporter exporter, bool wireTelemetryMiddleware = false)
    {
        var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Environment.EnvironmentName = "Test";
        builder.AddServiceDefaults();

        // AddOpenTelemetry is idempotent, so this attaches the test processor to the
        // same provider the production wiring just configured. A hand-rolled listener
        // could not prove the SDK is subscribed to the Papyrus source: Start would
        // succeed on the test's own listener even with AddSource removed.
        builder.Services.AddOpenTelemetry().WithTracing(tracing =>
            tracing.AddProcessor(new SimpleActivityExportProcessor(exporter)));

        var app = builder.Build();
        app.MapDefaultEndpoints();
        app.MapGet("/probe", () => "ok");
        if (wireTelemetryMiddleware)
        {
            app.UsePapyrusTelemetry();
        }

        await app.StartAsync();
        return app;
    }

    private static void ForceFlush(WebApplication host) =>
        host.Services.GetRequiredService<TracerProvider>().ForceFlush();

    private sealed class CollectingExporter : BaseExporter<Activity>
    {
        private readonly List<Activity> _items = [];

        public IReadOnlyList<Activity> ExportedItems
        {
            get
            {
                lock (_items)
                {
                    return _items.ToArray();
                }
            }
        }

        public override ExportResult Export(in Batch<Activity> batch)
        {
            lock (_items)
            {
                foreach (var item in batch)
                {
                    _items.Add(item);
                }
            }

            return ExportResult.Success;
        }
    }
}
