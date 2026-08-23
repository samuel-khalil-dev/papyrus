using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Papyrus.ServiceDefaults.Telemetry;
using Papyrus.ServiceDefaults.Telemetry.Redaction;

namespace Papyrus.Tests;

public sealed class TelemetryTests
{
    [Fact]
    public void Audit_sink_writes_a_structured_event_without_credentials()
    {
        // Arrange
        using var provider = new ListLoggerProvider();
        using var factory = LoggerFactory.Create(logging => logging.AddProvider(provider));
        var sink = new StructuredAuditEventSink(factory.CreateLogger<StructuredAuditEventSink>());
        var auditEvent = new AuditEvent(
            "tenant-a",
            "admin",
            "http.post",
            "/api/v1/projects",
            "correlation-1",
            "success",
            201,
            "trace-1",
            DateTimeOffset.UtcNow);

        // Act
        sink.Record(auditEvent);

        // Assert
        var entry = Assert.Single(provider.Entries);
        Assert.Equal("AuditEvent", entry.EventId.Name);
        Assert.Contains("tenant-a", entry.Message, StringComparison.Ordinal);
        Assert.Contains("correlation-1", entry.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("X-Admin-Api-Key", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", entry.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(entry.StructuredState, pair => pair.Key == "TenantId");
        Assert.Contains(entry.StructuredState, pair => pair.Key == "Action");
    }

    [Fact]
    public void Audit_event_is_logged_when_no_activity_listener_or_otlp_exporter_exists()
    {
        // Arrange
        using var provider = new ListLoggerProvider();
        using var factory = LoggerFactory.Create(logging => logging.AddProvider(provider));
        var sink = new StructuredAuditEventSink(factory.CreateLogger<StructuredAuditEventSink>());

        // Act
        sink.Record(new AuditEvent(
            "main",
            "anonymous",
            "auth.api_key",
            "admin-api",
            "correlation-2",
            "failure",
            401,
            null,
            DateTimeOffset.UtcNow));

        // Assert
        Assert.Single(provider.Entries);
        Assert.Equal(LogLevel.Warning, provider.Entries.ToArray()[0].Level);
    }

    [Fact]
    public void Telemetry_activity_source_uses_w3c_trace_ids()
    {
        // Arrange
        TelemetryModule.ConfigureTracePropagation();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == TelemetryActivitySource.Name,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            SampleUsingParentId = static (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllData
        };
        ActivitySource.AddActivityListener(listener);

        // Act
        using var activity = TelemetryActivitySource.Start("Papyrus.Tests.Telemetry");

        // Assert
        Assert.NotNull(activity);
        Assert.Equal(ActivityIdFormat.W3C, activity!.IdFormat);
        Assert.Equal(TelemetryActivitySource.Name, activity.Source.Name);
    }

    [Fact]
    public void Redactor_removes_untrusted_identity_and_secret_markers()
    {
        // Arrange
        var redactor = new Redactor();
        var auditEvent = new AuditEvent(
            "tenant-a",
            "user@example.com?token=secret-token",
            "http.post",
            "/api/v1/projects?X-Admin-Api-Key=secret-key",
            "correlation-3",
            "success",
            201,
            "trace-3",
            DateTimeOffset.UtcNow);

        // Act
        var redacted = redactor.Redact(auditEvent);

        // Assert
        Assert.Equal("tenant-a", redacted.TenantId);
        Assert.Equal(RedactionPolicy.Redacted, redacted.Actor);
        Assert.Equal(RedactionPolicy.Redacted, redacted.Target);
        Assert.DoesNotContain("secret", string.Join('|', redacted.GetType().GetProperties().Select(property => property.GetValue(redacted))), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", string.Join('|', redacted.GetType().GetProperties().Select(property => property.GetValue(redacted))), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Audit_event_survives_zero_percent_diagnostic_sampling()
    {
        // Arrange
        using var provider = new ListLoggerProvider();
        using var factory = LoggerFactory.Create(logging => logging.AddProvider(provider));
        var sampling = new TelemetrySamplingPolicy(Microsoft.Extensions.Options.Options.Create(new TelemetryOptions { SamplingRate = 0 }));
        var sink = new StructuredAuditEventSink(factory.CreateLogger<StructuredAuditEventSink>());

        // Act
        var diagnosticSampled = sampling.ShouldSampleDiagnostic();
        sink.Record(new AuditEvent(
            "tenant-a", "anonymous", "auth.api_key", "admin-api", "correlation-4",
            "failure", 401, null, DateTimeOffset.UtcNow));

        // Assert
        Assert.False(diagnosticSampled);
        Assert.Single(provider.Entries);
        Assert.Equal("AuditEvent", provider.Entries.ToArray()[0].EventId.Name);
    }

    [Fact]
    public void Cardinality_guard_maps_values_after_configured_limit()
    {
        // Arrange
        var guard = new CardinalityGuard(Microsoft.Extensions.Options.Options.Create(new TelemetryOptions { CardinalityLimit = 2 }));

        // Act
        var first = guard.GetValue("route", "/one");
        var second = guard.GetValue("route", "/two");
        var overflow = guard.GetValue("route", "/three");

        // Assert
        Assert.Equal("/one", first);
        Assert.Equal("/two", second);
        Assert.Equal("[other]", overflow);
    }

    private sealed class ListLoggerProvider : ILoggerProvider
    {
        public ConcurrentQueue<LogEntry> Entries { get; } = new();

        public ILogger CreateLogger(string categoryName) => new ListLogger(Entries);

        public void Dispose()
        {
        }
    }

    private sealed class ListLogger(ConcurrentQueue<LogEntry> entries) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var structuredState = state is IEnumerable<KeyValuePair<string, object?>> pairs
                ? pairs.ToArray()
                : [];
            entries.Enqueue(new LogEntry(logLevel, eventId, formatter(state, exception), structuredState));
        }
    }

    private sealed record LogEntry(
        LogLevel Level,
        EventId EventId,
        string Message,
        IReadOnlyList<KeyValuePair<string, object?>> StructuredState);

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
