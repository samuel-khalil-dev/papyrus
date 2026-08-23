using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Papyrus.ServiceDefaults.Telemetry.Redaction;

namespace Papyrus.ServiceDefaults.Telemetry;

public static class TelemetryModule
{
    public static IServiceCollection AddPapyrusTelemetry(this IServiceCollection services)
    {
        services.AddOptions<TelemetryOptions>().BindConfiguration(TelemetryOptions.SectionName);
        services.AddSingleton<Redactor>();
        services.AddSingleton<CardinalityGuard>();
        services.AddSingleton<TelemetrySamplingPolicy>();
        services.AddSingleton<IAuditEventSink, StructuredAuditEventSink>();
        return services;
    }

    public static IApplicationBuilder UsePapyrusTelemetry(this IApplicationBuilder app) =>
        app.UseMiddleware<TelemetryRequestMiddleware>();

    public static void ConfigureTracePropagation()
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        Activity.ForceDefaultIdFormat = true;
    }
}
