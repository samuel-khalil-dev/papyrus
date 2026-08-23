using Npgsql;

namespace Papyrus.Tests.Integration;

[Collection(PostgresCollection.Name)]
public sealed class IndexTests(PostgresFixture fixture)
{
    [Fact]
    public async Task Trigram_extension_and_partial_search_indexes_exist()
    {
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();

        await using var extensionCommand = new NpgsqlCommand(
            "SELECT EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'pg_trgm')", connection);
        Assert.True((bool)(await extensionCommand.ExecuteScalarAsync() ?? false));

        await using var indexCommand = new NpgsqlCommand(
            "SELECT indexname, indexdef FROM pg_indexes WHERE schemaname = 'public' AND indexname LIKE '%Search%' ORDER BY indexname",
            connection);
        await using var reader = await indexCommand.ExecuteReaderAsync();
        var definitions = new List<string>();
        while (await reader.ReadAsync())
        {
            definitions.Add($"{reader.GetString(0)} {reader.GetString(1)}");
        }

        Assert.Contains(definitions, value => value.Contains("gin_trgm_ops", StringComparison.Ordinal));
        Assert.Contains(definitions, value => value.Contains("WHERE (\"IsDeleted\" = false)", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Trigram_index_is_available_to_case_insensitive_search_plan()
    {
        await using var connection = new NpgsqlConnection(fixture.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SET enable_seqscan = off; SET enable_indexscan = off; EXPLAIN (COSTS OFF) SELECT \"Id\" FROM \"BlogPosts\" WHERE \"IsDeleted\" = false AND \"Title\" ILIKE '%fixture%';",
            connection);
        await using var reader = await command.ExecuteReaderAsync();
        var plan = new List<string>();
        while (await reader.ReadAsync())
        {
            plan.Add(reader.GetString(0));
        }

        Assert.DoesNotContain(plan, line => line.Contains("Seq Scan", StringComparison.Ordinal));
        Assert.Contains(plan, line => line.Contains("Index", StringComparison.Ordinal));
    }
}
