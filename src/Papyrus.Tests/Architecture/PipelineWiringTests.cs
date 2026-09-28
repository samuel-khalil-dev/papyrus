namespace Papyrus.Tests.Architecture;

/// The behavioural tests in CompositionRootTests and ServiceDefaultsPipelineTests
/// prove the shared pipeline extensions still do the right thing. They cannot
/// prove a host still *calls* them, because a host's Program.cs is top-level
/// statements that no test boots. These assertions close that gap: deleting
/// `app.UsePapyrusSecurityHeaders()` from a host would otherwise pass every
/// other test in the repository.
public sealed class PipelineWiringTests
{
    private static readonly string[] Hosts =
    [
        Path.Combine("src", "Papyrus.Api.Admin", "Program.cs"),
        Path.Combine("src", "Papyrus.Web.Blog", "Program.cs"),
        Path.Combine("src", "Papyrus.Web.Portfolio", "Program.cs"),
    ];

    /// <summary>
    /// The M1 routing policy is applied per public host, and each host serves a
    /// different site, so the expected kind is part of what must be declared.
    /// </summary>
    public static TheoryData<string, string> PublicHostPrograms()
    {
        var data = new TheoryData<string, string>();
        data.Add(Path.Combine("src", "Papyrus.Web.Blog", "Program.cs"), "SiteHostValidator.BlogKind");
        data.Add(Path.Combine("src", "Papyrus.Web.Portfolio", "Program.cs"), "SiteHostValidator.PortfolioKind");
        return data;
    }

    public static TheoryData<string> HostPrograms()
    {
        var data = new TheoryData<string>();
        foreach (var host in Hosts)
        {
            data.Add(host);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(HostPrograms))]
    public void Host_registers_the_shared_pipeline(string relativeProgramPath)
    {
        var source = ReadProgram(relativeProgramPath);

        Assert.Contains("builder.AddServiceDefaults()", source, StringComparison.Ordinal);
        Assert.Contains("app.UsePapyrusSecurityHeaders()", source, StringComparison.Ordinal);
        Assert.Contains("app.UsePapyrusTelemetry()", source, StringComparison.Ordinal);
        Assert.Contains("app.MapDefaultEndpoints()", source, StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(HostPrograms))]
    public void Host_never_migrates_the_database(string relativeProgramPath)
    {
        Assert.DoesNotContain("MigrateAsync(", ReadProgram(relativeProgramPath), StringComparison.Ordinal);
    }

    [Fact]
    public void Only_the_admin_host_initialises_the_database()
    {
        var callers = Hosts
            .Where(host => ReadProgram(host).Contains("InitializeDatabaseAsync(", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal([Path.Combine("src", "Papyrus.Api.Admin", "Program.cs")], callers);
    }

    [Fact]
    public void The_admin_host_still_registers_its_rate_limit()
    {
        // A missing or renamed registration would leave the API unmetered while
        // every rate-limit test still passed against its own host.
        Assert.Contains("builder.Services.AddAdminApiRateLimiting();", ReadProgram(Path.Combine("src", "Papyrus.Api.Admin", "Program.cs")), StringComparison.Ordinal);
    }

    [Theory]
    [MemberData(nameof(HostPrograms))]
    public void Routing_is_declared_before_the_telemetry_middleware(string relativeProgramPath)
    {
        // The telemetry middleware tags the selected endpoint, which is only
        // available once routing has run. Declaring the two in the wrong order
        // makes papyrus.endpoint a constant "unmatched-endpoint" on every request
        // while every behavioural test still passes.
        var source = ReadProgram(relativeProgramPath);
        var routing = source.IndexOf("app.UseRouting()", StringComparison.Ordinal);
        var telemetry = source.IndexOf("app.UsePapyrusTelemetry()", StringComparison.Ordinal);

        Assert.True(routing >= 0, $"{relativeProgramPath} does not declare UseRouting().");
        Assert.True(telemetry >= 0, $"{relativeProgramPath} does not declare UsePapyrusTelemetry().");
        Assert.True(
            routing < telemetry,
            $"{relativeProgramPath} declares UsePapyrusTelemetry() at offset {telemetry} and UseRouting() at {routing}; "
            + "routing must come first or the endpoint tag is always the unmatched fallback.");
    }

    [Theory]
    [MemberData(nameof(PublicHostPrograms))]
    public void Public_host_still_applies_the_routing_truth_policy(string relativeProgramPath, string expectedKind)
    {
        // A deleted or mis-kinded call would leave AllowedHosts at its default
        // and serve every Host header, while every SiteHostValidatorTests case
        // still passed against the shared helper.
        var source = ReadProgram(relativeProgramPath);

        Assert.Contains($"builder.UsePapyrusSiteHostPolicy({expectedKind});", source, StringComparison.Ordinal);
    }

    /// <summary>
    /// The OTLP exporter is gated on this variable in
    /// Papyrus.ServiceDefaults. If a host stops receiving it from compose, the
    /// exporter silently switches off and the collector records nothing, while
    /// every behavioural telemetry test still passes against an in-process host
    /// that sets the variable itself.
    /// </summary>
    [Theory]
    [MemberData(nameof(HostPrograms))]
    public void Compose_publishes_the_otlp_endpoint_to_every_host(string relativeProgramPath)
    {
        var compose = File.ReadAllText(Path.Combine(FindRepositoryRoot(), "docker-compose.yml"));
        var service = ServiceBlockFor(compose, relativeProgramPath);

        Assert.Contains("OTEL_EXPORTER_OTLP_ENDPOINT", service, StringComparison.Ordinal);
    }

    /// <summary>
    /// The release workflow is what actually starts the collector, Prometheus,
    /// Grafana, and blackbox-exporter in production. A removed step would leave
    /// the stack hand-placed on the host or absent, which no test in the
    /// application projects can observe.
    /// </summary>
    [Fact]
    public void Release_workflow_deploys_the_tracked_observability_stack()
    {
        var workflow = File.ReadAllText(Path.Combine(
            FindRepositoryRoot(), ".github", "workflows", "release.yml"));

        Assert.Contains("ops/observability/docker-compose.observability.yml", workflow, StringComparison.Ordinal);
        // The endpoint the applications export to, and the collector's own OTLP
        // receiver port, must agree or telemetry is silently dropped.
        Assert.Contains("OTEL_EXPORTER_OTLP_ENDPOINT: http://otel-collector:4317", workflow, StringComparison.Ordinal);
    }

    /// <summary>
    /// The documented collector-boundary contract names the metrics the
    /// dashboard and alert rules query. A rename in the collector config that
    /// is not applied to the rules leaves every alert permanently unevaluated.
    /// </summary>
    [Fact]
    public void Alert_rules_only_query_metrics_the_collector_derives()
    {
        var root = FindRepositoryRoot();
        var alerts = File.ReadAllText(Path.Combine(
            root, "ops", "observability", "prometheus", "papyrus-alerts.yml"));
        var collector = File.ReadAllText(Path.Combine(
            root, "ops", "observability", "collector", "config.yaml"));

        var queried = System.Text.RegularExpressions.Regex
            .Matches(alerts, @"\b(http_server_request_duration_seconds[a-z_]*|papyrus_[a-z_]+)\b")
            .Select(match => match.Value)
            .Distinct()
            .ToArray();

        Assert.NotEmpty(queried);
        foreach (var metric in queried)
        {
            // http_server_request_duration_seconds is produced by ASP.NET Core
            // instrumentation, not by the collector config; the rest must be
            // derived there.
            if (metric.StartsWith("http_server", StringComparison.Ordinal))
            {
                continue;
            }

            Assert.Contains(metric, collector, StringComparison.Ordinal);
        }
    }

    private static readonly Dictionary<string, string> HostComposeServices = new(StringComparer.Ordinal)
    {
        [Path.Combine("src", "Papyrus.Api.Admin", "Program.cs")] = "admin-api",
        [Path.Combine("src", "Papyrus.Web.Blog", "Program.cs")] = "blog",
        [Path.Combine("src", "Papyrus.Web.Portfolio", "Program.cs")] = "portfolio"
    };

    private static string ServiceBlockFor(string compose, string relativeProgramPath)
    {
        var service = HostComposeServices[relativeProgramPath];

        var start = compose.IndexOf($"\n  {service}:", StringComparison.Ordinal);
        Assert.True(start >= 0, $"docker-compose.yml has no {service} service.");

        // The block ends at the next sibling service, which is a line indented
        // by exactly two spaces. A plain "\n  " search would also match the
        // deeper-indented keys inside this service.
        var next = compose.IndexOf("\n  ", start + 3, StringComparison.Ordinal);
        while (next >= 0 && next + 3 < compose.Length && char.IsWhiteSpace(compose[next + 2]))
        {
            next = compose.IndexOf("\n  ", next + 1, StringComparison.Ordinal);
        }

        var block = next < 0 ? compose[start..] : compose[start..next];
        Assert.NotEqual(0, block.Length);
        return block;
    }

    private static string ReadProgram(string relativeProgramPath)
    {
        var path = Path.Combine(FindRepositoryRoot(), relativeProgramPath);
        Assert.True(File.Exists(path), $"Expected {relativeProgramPath} to exist.");
        return File.ReadAllText(path);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Papyrus.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate the repository root.");
    }
}
