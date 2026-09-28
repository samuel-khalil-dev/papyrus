using Microsoft.EntityFrameworkCore;
using Papyrus.Application.Content;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;

namespace Papyrus.Infrastructure.Content;

/// <summary>
/// EF Core adapter for <see cref="IContentCommandStore"/>.
/// </summary>
public sealed class EfContentCommandStore(PapyrusDbContext db) : IContentCommandStore
{
    // The pre-check is skipped on update when the row already holds this slug:
    // the query would exclude the row being edited anyway, so the round trip
    // would buy nothing on the most common write.
    public async Task<CommandResult<Project>> SaveProjectAsync(SaveProjectCommand command, CancellationToken cancellationToken)
    {
        var validation = ContentValidation.ForProjectOrPost(command.Slug, command.Title, command.Summary, command.TagIds);
        if (validation.Count > 0) return CommandResult<Project>.Invalid(validation.ToArray());
        if (command.CategoryId is not null && !await db.ContentCategories.AsNoTracking().AnyAsync(x => x.Id == command.CategoryId, cancellationToken))
            return CommandResult<Project>.Invalid(new ValidationError("category.not_found", "Category was not found."));
        if (!await TagsExistAsync(command.TagIds, cancellationToken))
            return CommandResult<Project>.Invalid(new ValidationError("tag.not_found", "One or more tags were not found."));

        var project = command.Id is null
            ? new Project { Id = Guid.NewGuid(), TenantId = db.TenantId }
            : await db.Projects.Include(x => x.ProjectTags).FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (project is null) return CommandResult<Project>.Invalid(new ValidationError("project.not_found", "Project was not found."));
        if (project.Slug != command.Slug
            && await SlugConflict.ConflictingIdAsync(db.Projects, command.Slug, command.Id, cancellationToken) is { } conflictingProjectId)
            return CommandResult<Project>.Invalid(SlugConflict.Conflict(ContentValidation.ProjectSlugTaken, conflictingProjectId));
        project.CategoryId = command.CategoryId; project.Slug = command.Slug; project.Title = command.Title;
        project.Summary = command.Summary; project.Description = command.Description; project.RepositoryUrl = command.RepositoryUrl;
        project.LiveUrl = command.LiveUrl; project.CoverImageUrl = command.CoverImageUrl; project.IsFeatured = command.IsFeatured;
        project.IsPublished = command.IsPublished; project.PublishedAt = command.PublishedAt;
        if (command.Id is null) db.Projects.Add(project);
        await ReplaceProjectTagsAsync(project, command.TagIds, cancellationToken);
        return await SlugConflict.TranslateAsync(
            () => db.SaveChangesAsync(cancellationToken), project, ContentValidation.ProjectSlugTaken);
    }

    public async Task<CommandResult<BlogPost>> SaveBlogPostAsync(SaveBlogPostCommand command, CancellationToken cancellationToken)
    {
        var validation = ContentValidation.ForProjectOrPost(command.Slug, command.Title, command.Summary, command.TagIds);
        if (validation.Count > 0 || string.IsNullOrWhiteSpace(command.ContentMarkdown))
            return CommandResult<BlogPost>.Invalid([.. validation, new ValidationError("post.content_required", "Content is required.")]);
        if (command.ProjectId is not null && !await db.Projects.AsNoTracking().AnyAsync(x => x.Id == command.ProjectId, cancellationToken))
            return CommandResult<BlogPost>.Invalid(new ValidationError("project.not_found", "Project was not found."));
        if (command.CategoryId is not null && !await db.ContentCategories.AsNoTracking().AnyAsync(x => x.Id == command.CategoryId, cancellationToken))
            return CommandResult<BlogPost>.Invalid(new ValidationError("category.not_found", "Category was not found."));
        if (!await TagsExistAsync(command.TagIds, cancellationToken))
            return CommandResult<BlogPost>.Invalid(new ValidationError("tag.not_found", "One or more tags were not found."));

        var post = command.Id is null
            ? new BlogPost { Id = Guid.NewGuid(), TenantId = db.TenantId }
            : await db.BlogPosts.Include(x => x.BlogPostTags).FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (post is null) return CommandResult<BlogPost>.Invalid(new ValidationError("post.not_found", "Blog post was not found."));
        if (post.Slug != command.Slug
            && await SlugConflict.ConflictingIdAsync(db.BlogPosts, command.Slug, command.Id, cancellationToken) is { } conflictingPostId)
            return CommandResult<BlogPost>.Invalid(SlugConflict.Conflict(ContentValidation.BlogPostSlugTaken, conflictingPostId));
        post.ProjectId = command.ProjectId; post.CategoryId = command.CategoryId; post.Slug = command.Slug;
        post.Title = command.Title; post.Summary = command.Summary; post.ContentMarkdown = command.ContentMarkdown;
        post.CoverImageUrl = command.CoverImageUrl; post.IsFeatured = command.IsFeatured; post.IsPublished = command.IsPublished;
        post.PublishedAt = command.PublishedAt;
        if (command.Id is null) db.BlogPosts.Add(post);
        var current = post.BlogPostTags.ToArray(); db.BlogPostTags.RemoveRange(current);
        foreach (var tagId in command.TagIds.Distinct()) post.BlogPostTags.Add(new BlogPostTag { BlogPostId = post.Id, TagId = tagId, TenantId = db.TenantId });
        return await SlugConflict.TranslateAsync(
            () => db.SaveChangesAsync(cancellationToken), post, ContentValidation.BlogPostSlugTaken);
    }

    private async Task<bool> TagsExistAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        ids.Count == 0 || await db.ContentTags.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => x.Id).CountAsync(cancellationToken) == ids.Distinct().Count();

    private async Task ReplaceProjectTagsAsync(Project project, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken)
    {
        var current = await db.ProjectTags.Where(x => x.ProjectId == project.Id).ToListAsync(cancellationToken);
        db.ProjectTags.RemoveRange(current);
        foreach (var tagId in tagIds.Distinct()) db.ProjectTags.Add(new ProjectTag { ProjectId = project.Id, TagId = tagId, TenantId = db.TenantId });
    }
}
