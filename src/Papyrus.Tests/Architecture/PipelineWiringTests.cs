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

    /// <summary>
    /// The Grafana dashboard is mounted by FILE provisioning, so Grafana never
    /// resolves the <c>${DS_PROMETHEUS}</c> placeholder that it substitutes when
    /// a dashboard is imported through the UI, and the datasource provisioning
    /// declares no <c>uid</c> for Grafana to be asked about. Every panel then
    /// binds to a datasource that does not exist and renders empty — while
    /// Prometheus reports healthy, the collector is up, and the dashboard is
    /// served, so every other signal in the stack stays green.
    /// <para>
    /// Both files are read because either half can drift on its own: reading only
    /// the dashboard would pass while the provisioning stopped declaring a uid,
    /// and reading only the provisioning would pass while the dashboard went back
    /// to the placeholder.
    /// </para>
    /// </summary>
    [Fact]
    public void Dashboard_panels_bind_to_the_provisioned_prometheus_datasource()
    {
        var root = FindRepositoryRoot();
        var provisioning = File.ReadAllText(Path.Combine(
            root, "ops", "observability", "grafana", "provisioning", "datasources", "prometheus.yml"));
        var dashboard = File.ReadAllText(Path.Combine(
            root, "docs", "observability", "dashboards", "papyrus-dashboard.json"));

        // The provisioning side: a generated uid does not survive a restart or a
        // fresh volume, so it has to be pinned and the dashboard has to name it.
        var declared = System.Text.RegularExpressions.Regex.Match(
            provisioning, @"(?m)^\s*uid:\s*(?<uid>\S+)\s*$");
        Assert.True(
            declared.Success,
            "prometheus.yml declares no uid, so Grafana generates one and no file-provisioned dashboard can bind to it.");
        var provisionedUid = declared.Groups["uid"].Value;

        // The dashboard side: every Prometheus datasource block must carry the
        // provisioned uid literally. The built-in grafana annotation datasource is
        // not Prometheus and is deliberately left out.
        using var document = System.Text.Json.JsonDocument.Parse(dashboard);
        var bindings = DatasourceBindings(document.RootElement)
            .Where(binding => binding.Type == "prometheus")
            .ToArray();
        Assert.True(
            bindings.Length > 0,
            "papyrus-dashboard.json declares no Prometheus datasource, so no panel can resolve one.");

        foreach (var binding in bindings)
        {
            // ${DS_PROMETHEUS} is only substituted at UI-import time, and there is
            // no __inputs block to drive the substitution for file provisioning.
            Assert.True(
                !binding.Uid.Contains('$', StringComparison.Ordinal),
                $"papyrus-dashboard.json binds a datasource to '{binding.Uid}'. File provisioning does not substitute a Grafana import-time variable, so that block can never resolve.");
            Assert.True(
                binding.Uid == provisionedUid,
                $"papyrus-dashboard.json binds a datasource to uid '{binding.Uid}' but prometheus.yml provisions '{provisionedUid}'; every panel would render empty.");
        }

        // Finally the literal both files are required to share, so that renaming
        // the datasource is a deliberate edit to the test rather than a rename
        // that silently changes what a shipped dashboard binds to.
        Assert.True(
            provisionedUid == "papyrus-prometheus",
            $"prometheus.yml provisions uid '{provisionedUid}'; the dashboard must be shipped naming 'papyrus-prometheus'.");
    }

    /// <summary>
    /// The public hosts reject any Host header that is not their configured
    /// public base URL or loopback, so blackbox has to send the right one or the
    /// probe returns 400 and <c>up</c> reads 0 forever — which looks exactly
    /// like a host that is down, and cannot be told apart from a real outage.
    /// <para>
    /// The Host headers are literals in the exporter config because
    /// blackbox-exporter v0.27.0 does not expand <c>${VAR}</c> there. That makes
    /// them driftable, so this asserts they still name the same host the
    /// application is configured to answer to. A mismatch fails the build
    /// instead of silently disabling the availability alert.
    /// </para>
    /// </summary>
    [Theory]
    [InlineData("http_2xx_blog_host", "BLOG_PUBLIC_BASE_URL", "blog", "http://blog:8080/health")]
    [InlineData("http_2xx_portfolio_host", "PORTFOLIO_PUBLIC_BASE_URL", "portfolio", "http://portfolio:8080/health")]
    public void Availability_probe_sends_the_host_the_public_site_answers_to(
        string module, string baseUrlVariable, string serviceName, string expectedTarget)
    {
        var root = FindRepositoryRoot();
        var blackbox = File.ReadAllText(Path.Combine(
            root, "ops", "observability", "blackbox-exporter", "config.yml"));
        var workflow = File.ReadAllText(Path.Combine(root, ".github", "workflows", "release.yml"));
        var compose = File.ReadAllText(Path.Combine(root, "docker-compose.yml"));

        // The module must exist and carry a Host header, or the probe falls back
        // to the target's own hostname, which these hosts refuse with 400.
        var moduleStart = blackbox.IndexOf(module + ":", StringComparison.Ordinal);
        Assert.True(moduleStart >= 0, $"blackbox config has no module named {module}.");
        var hostLine = System.Text.RegularExpressions.Regex.Match(
            blackbox[moduleStart..], @"Host:\s*(\S+)");
        Assert.True(hostLine.Success, $"module {module} sets no Host header.");

        // The header must be the same host the application container is
        // configured to answer to, so it cannot go stale.
        var expectedHost = ExpectedPublicHost(baseUrlVariable);
        Assert.Equal(expectedHost, hostLine.Groups[1].Value);

        // The variable must actually reach the container that serves the public
        // host, or the site answers on a different domain and the header is
        // wrong regardless of what the exporter sends. The compose form is
        // ${VAR:?message}, so match the variable name rather than the whole
        // interpolation expression.
        Assert.Contains(
            $"Site__PublicBaseUrl: ${{{baseUrlVariable}:?",
            compose,
            StringComparison.Ordinal);
        Assert.Contains(
            $"{baseUrlVariable}: ${{{{ vars.{baseUrlVariable} }}}}",
            workflow,
            StringComparison.Ordinal);

        // The module must not substitute a variable: blackbox-exporter v0.27.0
        // does not expand ${VAR} in its config, so that form silently sends no
        // Host header and the probe is blind again.
        Assert.DoesNotContain("$", hostLine.Groups[1].Value, StringComparison.Ordinal);

        // And Prometheus must actually probe that service with that module.
        var prometheus = File.ReadAllText(Path.Combine(
            root, "ops", "observability", "prometheus", "prometheus.yml"));
        Assert.Contains($"module: [{module}]", prometheus, StringComparison.Ordinal);
        Assert.Contains($"'{expectedTarget}'", prometheus, StringComparison.Ordinal);
        Assert.Contains($"job: {serviceName}", prometheus, StringComparison.Ordinal);
    }

    /// <summary>
    /// The host a public base URL variable is expected to name. Held literally
    /// here so that changing a configured domain is a deliberate, visible edit
    /// to this test rather than a silent drift that blinds the probe.
    /// </summary>
    private static string ExpectedPublicHost(string baseUrlVariable) => baseUrlVariable switch
    {
        "BLOG_PUBLIC_BASE_URL" => "blog.papyrus.quest",
        "PORTFOLIO_PUBLIC_BASE_URL" => "papyrus.quest",
        _ => throw new ArgumentOutOfRangeException(
            nameof(baseUrlVariable), baseUrlVariable, "Unknown base URL variable."),
    };

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

    /// <summary>
    /// Every <c>datasource</c> object in the dashboard, at any depth, so a
    /// binding added to a panel, a target, or a template variable is covered
    /// without this having to know the dashboard's shape.
    /// </summary>
    private static IEnumerable<(string Type, string Uid)> DatasourceBindings(
        System.Text.Json.JsonElement element)
    {
        if (element.ValueKind == System.Text.Json.JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("datasource")
                    && property.Value.ValueKind == System.Text.Json.JsonValueKind.Object
                    && property.Value.TryGetProperty("type", out var type)
                    && type.ValueKind == System.Text.Json.JsonValueKind.String
                    && type.GetString() is { } declaredType)
                {
                    // A block without a uid binds to the default datasource by
                    // Grafana's own rule, which the caller reports as an empty uid
                    // rather than silently passing it.
                    var uid = property.Value.TryGetProperty("uid", out var uidElement)
                        && uidElement.ValueKind == System.Text.Json.JsonValueKind.String
                            ? uidElement.GetString() ?? string.Empty
                            : string.Empty;
                    yield return (declaredType, uid);
                }

                foreach (var nested in DatasourceBindings(property.Value))
                {
                    yield return nested;
                }
            }

            yield break;
        }

        if (element.ValueKind == System.Text.Json.JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                foreach (var nested in DatasourceBindings(item))
                {
                    yield return nested;
                }
            }
        }
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
