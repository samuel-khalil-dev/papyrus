using Papyrus.Core.Content;

namespace Papyrus.Application.Content;

/// <summary>
/// Application-owned port for content write operations. The application layer
/// depends on this interface rather than on a persistence implementation.
/// </summary>
public interface IContentCommandStore
{
    Task<CommandResult<Project>> SaveProjectAsync(SaveProjectCommand command, CancellationToken cancellationToken);

    Task<CommandResult<BlogPost>> SaveBlogPostAsync(SaveBlogPostCommand command, CancellationToken cancellationToken);
}
