using Microsoft.EntityFrameworkCore;
using Papyrus.Application.Content;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;

namespace Papyrus.Infrastructure.Content;

/// <summary>
/// EF Core adapter for <see cref="ITaxonomyCommandStore"/>.
/// </summary>
public sealed class EfTaxonomyCommandStore(PapyrusDbContext db) : ITaxonomyCommandStore
{
    public async Task<CommandResult<ContentCategory>> SaveCategoryAsync(SaveCategoryCommand command, CancellationToken cancellationToken)
    {
        var validation = ContentValidation.ForCategoryOrTag("category", command.Slug, command.Name);
        if (validation.Count > 0) return CommandResult<ContentCategory>.Invalid(validation.ToArray());
        var category = command.Id is null ? new ContentCategory { Id = Guid.NewGuid(), TenantId = db.TenantId } : await db.ContentCategories.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (category is null) return CommandResult<ContentCategory>.Invalid(new ValidationError("category.not_found", "Category was not found."));
        category.Slug = command.Slug; category.Name = command.Name; category.Description = command.Description; category.IsPublished = command.IsPublished;
        if (command.Id is null) db.ContentCategories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return CommandResult<ContentCategory>.Success(category);
    }

    public async Task<CommandResult<ContentTag>> SaveTagAsync(SaveTagCommand command, CancellationToken cancellationToken)
    {
        var validation = ContentValidation.ForCategoryOrTag("tag", command.Slug, command.Name);
        if (validation.Count > 0) return CommandResult<ContentTag>.Invalid(validation.ToArray());
        var tag = command.Id is null ? new ContentTag { Id = Guid.NewGuid(), TenantId = db.TenantId } : await db.ContentTags.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (tag is null) return CommandResult<ContentTag>.Invalid(new ValidationError("tag.not_found", "Tag was not found."));
        tag.Slug = command.Slug; tag.Name = command.Name;
        if (command.Id is null) db.ContentTags.Add(tag);
        await db.SaveChangesAsync(cancellationToken);
        return CommandResult<ContentTag>.Success(tag);
    }
}
