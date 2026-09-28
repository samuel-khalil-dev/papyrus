using Microsoft.EntityFrameworkCore;
using Npgsql;
using Papyrus.Application.Content;
using Papyrus.Core.Content;

namespace Papyrus.Infrastructure.Content;

/// <summary>
/// Detects and translates a duplicate-slug write conflict.
/// <para>
/// Every slug-carrying aggregate has a partial unique index on
/// <c>(TenantId, Slug)</c> filtered to <c>"IsDeleted" = false</c>. Without this
/// translation the index violation escaped the command store as an unhandled
/// <see cref="DbUpdateException"/> and the API answered 500 for what is a
/// conflict an operator can act on.
/// </para>
/// <para>
/// Two layers, because one is not enough. The pre-check resolves the ordinary
/// case without a failed write, and the <c>23505</c> translation is the only
/// race-proof backstop: two concurrent creates with the same new slug can both
/// pass any pre-check, and exactly one of them is rejected by the index. Every
/// other <see cref="DbUpdateException"/> is rethrown unchanged so a genuine
/// database fault is still a 500.
/// </para>
/// </summary>
internal static class SlugConflict
{
    /// <summary>PostgreSQL <c>unique_violation</c>.</summary>
    private const string UniqueViolation = "23505";

    /// <summary>
    /// Returns the identifier of a non-deleted row in the same tenant that
    /// already holds <paramref name="slug"/> under a different identifier, or
    /// null when the slug is free.
    /// <para>
    /// The global query filter already excludes soft-deleted rows and other
    /// tenants, so this matches the partial unique index exactly rather than
    /// approximating it. The identifier comparison is what lets a row be
    /// re-saved under its own slug, which a naive "does this slug exist" check
    /// would break on every ordinary edit.
    /// </para>
    /// </summary>
    public static async Task<Guid?> ConflictingIdAsync(
        IQueryable<IContentSlugged> candidates, string slug, Guid? id, CancellationToken cancellationToken) =>
        // The projection must be nullable: a non-nullable Guid would make
        // FirstOrDefault return Guid.Empty for "no match", which is a real
        // identifier the test host can use, and every create would report a
        // conflict against nothing.
        await candidates
            .Where(entity => entity.Slug == slug && (id == null || entity.Id != id))
            .Select(entity => (Guid?)entity.Id)
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>
    /// Runs <paramref name="save"/>, and if it fails on a unique-index
    /// violation returns <paramref name="error"/> instead. Every other
    /// exception propagates, so a genuine database fault is still a 500.
    /// <para>
    /// The caller must not touch its <c>DbContext</c> again after this returns
    /// false: a failed <c>SaveChanges</c> leaves the added entry tracked, so a
    /// retry through the same context throws again. That is why the endpoints
    /// return the conflict without re-reading the entity. It is also why this
    /// path cannot name the row that won the race — only the pre-check knows
    /// it.
    /// </para>
    /// </summary>
    public static async Task<CommandResult<T>> TranslateAsync<T>(
        Func<Task> save, T value, ValidationError error)
    {
        try
        {
            await save().ConfigureAwait(false);
            return CommandResult<T>.Success(value);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return CommandResult<T>.Invalid(error);
        }
    }

    /// <summary>The conflict error naming the row that already holds the slug.</summary>
    public static ValidationError Conflict(ValidationError error, Guid conflictingId) =>
        new(error.Code, $"{error.Message} It is used by {conflictingId}.");

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: UniqueViolation };
}
