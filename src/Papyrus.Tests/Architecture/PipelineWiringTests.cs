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
