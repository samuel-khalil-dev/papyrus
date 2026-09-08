using Papyrus.Core.Content;

namespace Papyrus.Application.Content;

/// <summary>
/// Application-owned port for taxonomy (category/tag) write operations.
/// </summary>
public interface ITaxonomyCommandStore
{
    Task<CommandResult<ContentCategory>> SaveCategoryAsync(SaveCategoryCommand command, CancellationToken cancellationToken);

    Task<CommandResult<ContentTag>> SaveTagAsync(SaveTagCommand command, CancellationToken cancellationToken);
}
