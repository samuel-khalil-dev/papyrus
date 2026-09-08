using Papyrus.Core.Content;

namespace Papyrus.Application.Content;

public sealed record ValidationError(string Code, string Message);

public sealed record CommandResult<T>(T? Value, IReadOnlyList<ValidationError> Errors)
{
    public bool IsSuccess => Errors.Count == 0;
    public static CommandResult<T> Invalid(params ValidationError[] errors) => new(default, errors);
    public static CommandResult<T> Success(T value) => new(value, []);
}

public sealed record SaveProjectCommand(
    Guid? Id, string Slug, string Title, string Summary, string? Description,
    string? RepositoryUrl, string? LiveUrl, string? CoverImageUrl, Guid? CategoryId,
    bool IsFeatured, bool IsPublished, DateTimeOffset? PublishedAt, IReadOnlyCollection<Guid> TagIds);

public sealed record SaveBlogPostCommand(
    Guid? Id, string Slug, string Title, string Summary, string ContentMarkdown,
    string? CoverImageUrl, Guid? ProjectId, Guid? CategoryId, bool IsFeatured,
    bool IsPublished, DateTimeOffset? PublishedAt, IReadOnlyCollection<Guid> TagIds);

/// <summary>
/// Application-facing facade over <see cref="IContentCommandStore"/>.
/// </summary>
public sealed class ContentCommandHandlers(IContentCommandStore store)
{
    public Task<CommandResult<Project>> SaveProjectAsync(SaveProjectCommand command, CancellationToken cancellationToken) =>
        store.SaveProjectAsync(command, cancellationToken);

    public Task<CommandResult<BlogPost>> SaveBlogPostAsync(SaveBlogPostCommand command, CancellationToken cancellationToken) =>
        store.SaveBlogPostAsync(command, cancellationToken);
}
