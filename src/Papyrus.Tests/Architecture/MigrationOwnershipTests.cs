using System.Reflection;

namespace Papyrus.Tests.Architecture;

public sealed class MigrationOwnershipTests
{
    [Fact]
    public void Infrastructure_owns_the_db_context_and_migrations()
    {
        var infrastructure = typeof(Papyrus.Infrastructure.PapyrusDbContext).Assembly;
        var migrationTypes = infrastructure.GetTypes()
            .Where(type => type.Namespace?.Contains("Migrations", StringComparison.Ordinal) == true)
            .ToArray();

        Assert.NotEmpty(migrationTypes);
        Assert.All(migrationTypes, type => Assert.Equal(infrastructure, type.Assembly));
        Assert.Contains("PapyrusDbContextModelSnapshot", migrationTypes.Select(type => type.Name));
    }

    [Fact]
    public void Only_admin_startup_authorizes_schema_migration()
    {
        var migrationCallSites = FindRepositoryFiles("src", "*.cs")
            .Where(path => !path.Contains("Migrations", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains(Path.Combine("Papyrus.Tests", "Architecture"), StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains(Path.Combine("Papyrus.Tests", "Integration"), StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadLines(path)
                .Select((line, lineNumber) => (path, lineNumber: lineNumber + 1, line)))
            .Where(entry => entry.line.Contains(".MigrateAsync(", StringComparison.Ordinal))
            .ToArray();

        var callSite = Assert.Single(migrationCallSites);
        Assert.Contains(Path.Combine("src", "Papyrus.Infrastructure"), callSite.path,
            StringComparison.OrdinalIgnoreCase);

        var adminStartupCalls = FindRepositoryFiles("src", "*.cs")
            .Where(path => !path.Contains("Migrations", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains(Path.Combine("Papyrus.Tests", "Architecture"), StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains(Path.Combine("Papyrus.Tests", "Integration"), StringComparison.OrdinalIgnoreCase))
            .SelectMany(path => File.ReadLines(path)
                .Select((line, lineNumber) => (path, lineNumber: lineNumber + 1, line)))
            .Where(entry => entry.line.Contains("await app.InitializeDatabaseAsync(", StringComparison.Ordinal))
            .ToArray();

        var startupCall = Assert.Single(adminStartupCalls);
        Assert.Contains(Path.Combine("src", "Papyrus.Api.Admin"), startupCall.path,
            StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> FindRepositoryFiles(string directory, string pattern)
    {
        var repositoryRoot = FindRepositoryRoot();
        return Directory.EnumerateFiles(Path.Combine(repositoryRoot, directory), pattern, SearchOption.AllDirectories);
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
