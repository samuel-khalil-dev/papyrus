using Microsoft.EntityFrameworkCore;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;

namespace Papyrus.Application.Content;

public sealed record SaveCategoryCommand(Guid? Id, string Slug, string Name, string? Description, bool IsPublished);
public sealed record SaveTagCommand(Guid? Id, string Slug, string Name);

public sealed class TaxonomyCommandHandlers(PapyrusDbContext db)
{
    public async Task<CommandResult<ContentCategory>> SaveCategoryAsync(SaveCategoryCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Slug) || command.Slug.Length > 200 || string.IsNullOrWhiteSpace(command.Name) || command.Name.Length > 200)
            return CommandResult<ContentCategory>.Invalid(new ValidationError("category.invalid", "Category slug and name are required and limited to 200 characters."));
        var category = command.Id is null ? new ContentCategory { Id = Guid.NewGuid(), TenantId = db.TenantId } : await db.ContentCategories.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (category is null) return CommandResult<ContentCategory>.Invalid(new ValidationError("category.not_found", "Category was not found."));
        category.Slug = command.Slug; category.Name = command.Name; category.Description = command.Description; category.IsPublished = command.IsPublished;
        if (command.Id is null) db.ContentCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return CommandResult<ContentCategory>.Success(category);
    }

    public async Task<CommandResult<ContentTag>> SaveTagAsync(SaveTagCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Slug) || command.Slug.Length > 200 || string.IsNullOrWhiteSpace(command.Name) || command.Name.Length > 200)
            return CommandResult<ContentTag>.Invalid(new ValidationError("tag.invalid", "Tag slug and name are required and limited to 200 characters."));
        var tag = command.Id is null ? new ContentTag { Id = Guid.NewGuid(), TenantId = db.TenantId } : await db.ContentTags.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (tag is null) return CommandResult<ContentTag>.Invalid(new ValidationError("tag.not_found", "Tag was not found."));
        tag.Slug = command.Slug; tag.Name = command.Name;
        if (command.Id is null) db.ContentTags.Add(tag);
        await db.SaveChangesAsync(cancellationToken);
        return CommandResult<ContentTag>.Success(tag);
    }
}

public sealed class ContentLifecycleHandlers(PapyrusDbContext db)
{
    public async Task<bool> SoftDeleteProjectAsync(Guid id, string reason, CancellationToken cancellationToken) => await SetProjectStateAsync(id, true, reason, cancellationToken);
    public async Task<bool> RestoreProjectAsync(Guid id, CancellationToken cancellationToken) => await SetProjectStateAsync(id, false, null, cancellationToken);
    public async Task<bool> SoftDeleteBlogPostAsync(Guid id, string reason, CancellationToken cancellationToken) => await SetPostStateAsync(id, true, reason, cancellationToken);
    public async Task<bool> RestoreBlogPostAsync(Guid id, CancellationToken cancellationToken) => await SetPostStateAsync(id, false, null, cancellationToken);

    private async Task<bool> SetProjectStateAsync(Guid id, bool deleted, string? reason, CancellationToken cancellationToken)
    {
        var project = await db.Projects.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (project is null) return false;
        project.IsDeleted = deleted; project.DeletedAt = deleted ? DateTimeOffset.UtcNow : null; project.DeleteReason = deleted ? reason : null;
        await db.SaveChangesAsync(cancellationToken); return true;
    }

    private async Task<bool> SetPostStateAsync(Guid id, bool deleted, string? reason, CancellationToken cancellationToken)
    {
        var post = await db.BlogPosts.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (post is null) return false;
        post.IsDeleted = deleted; post.DeletedAt = deleted ? DateTimeOffset.UtcNow : null; post.DeleteReason = deleted ? reason : null;
        await db.SaveChangesAsync(cancellationToken); return true;
    }
}
