using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;

namespace Papyrus.Tests.Integration;

public sealed class PostgresFixture : IAsyncLifetime
{
    public const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=papyrus_test;Username=postgres;Password=postgres";

    public string ConnectionString { get; } =
        Environment.GetEnvironmentVariable("PAPYRUS_TEST_CONNECTION_STRING") ?? DefaultConnectionString;

    public string TenantId { get; } = "integration";

    public async Task InitializeAsync()
    {
        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
        await EnsureSearchExtensionAsync();

        if (!await db.Projects.IgnoreQueryFilters().AnyAsync(project => project.TenantId == TenantId))
        {
            var now = DateTimeOffset.UtcNow;
            db.Projects.Add(new Project
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                Slug = "fixture-seed",
                Title = "Integration fixture seed",
                Summary = "Known PostgreSQL integration fixture data.",
                IsPublished = true,
                PublishedAt = now,
                CreatedAt = now,
                CreatedBy = "integration-fixture"
            });
            await db.SaveChangesAsync();
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task EnsureSearchExtensionAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "CREATE EXTENSION IF NOT EXISTS pg_trgm; " +
            "CREATE INDEX IF NOT EXISTS \"IX_BlogPosts_Search_Title\" ON \"BlogPosts\" USING gin (\"Title\" gin_trgm_ops) WHERE \"IsDeleted\" = false;",
            connection);
        await command.ExecuteNonQueryAsync();
    }

    public PapyrusDbContext CreateDbContext(string? tenantId = null, params IInterceptor[] interceptors) =>
        TestDb.CreatePostgres(ConnectionString, tenantId ?? TenantId, interceptors);
}

public static class PostgresCollection
{
    public const string Name = "PostgreSQL integration collection";
}

[CollectionDefinition(PostgresCollection.Name, DisableParallelization = true)]
public sealed class PostgresCollectionDefinition : ICollectionFixture<PostgresFixture>;
