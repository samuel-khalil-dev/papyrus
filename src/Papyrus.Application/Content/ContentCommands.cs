using Microsoft.EntityFrameworkCore;
using Papyrus.Core.Content;
using Papyrus.Infrastructure;

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

public sealed class ContentCommandHandlers(PapyrusDbContext db)
{
    public async Task<CommandResult<Project>> SaveProjectAsync(SaveProjectCommand command, CancellationToken cancellationToken)
    {
        var validation = Validate(command.Slug, command.Title, command.Summary, command.TagIds);
        if (validation.Count > 0) return CommandResult<Project>.Invalid(validation.ToArray());
        if (command.CategoryId is not null && !await db.ContentCategories.AsNoTracking().AnyAsync(x => x.Id == command.CategoryId, cancellationToken))
            return CommandResult<Project>.Invalid(new ValidationError("category.not_found", "Category was not found."));
        if (!await TagsExistAsync(command.TagIds, cancellationToken))
            return CommandResult<Project>.Invalid(new ValidationError("tag.not_found", "One or more tags were not found."));

        var project = command.Id is null
            ? new Project { Id = Guid.NewGuid(), TenantId = db.TenantId }
            : await db.Projects.Include(x => x.ProjectTags).FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (project is null) return CommandResult<Project>.Invalid(new ValidationError("project.not_found", "Project was not found."));
        project.CategoryId = command.CategoryId; project.Slug = command.Slug; project.Title = command.Title;
        project.Summary = command.Summary; project.Description = command.Description; project.RepositoryUrl = command.RepositoryUrl;
        project.LiveUrl = command.LiveUrl; project.CoverImageUrl = command.CoverImageUrl; project.IsFeatured = command.IsFeatured;
        project.IsPublished = command.IsPublished; project.PublishedAt = command.PublishedAt;
        if (command.Id is null) db.Projects.Add(project);
        await ReplaceProjectTagsAsync(project, command.TagIds, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return CommandResult<Project>.Success(project);
    }

    public async Task<CommandResult<BlogPost>> SaveBlogPostAsync(SaveBlogPostCommand command, CancellationToken cancellationToken)
    {
        var validation = Validate(command.Slug, command.Title, command.Summary, command.TagIds);
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
        post.ProjectId = command.ProjectId; post.CategoryId = command.CategoryId; post.Slug = command.Slug;
        post.Title = command.Title; post.Summary = command.Summary; post.ContentMarkdown = command.ContentMarkdown;
        post.CoverImageUrl = command.CoverImageUrl; post.IsFeatured = command.IsFeatured; post.IsPublished = command.IsPublished;
        post.PublishedAt = command.PublishedAt;
        if (command.Id is null) db.BlogPosts.Add(post);
        var current = post.BlogPostTags.ToArray(); db.BlogPostTags.RemoveRange(current);
        foreach (var tagId in command.TagIds.Distinct()) post.BlogPostTags.Add(new BlogPostTag { BlogPostId = post.Id, TagId = tagId, TenantId = db.TenantId });
        await db.SaveChangesAsync(cancellationToken);
        return CommandResult<BlogPost>.Success(post);
    }

    private async Task<bool> TagsExistAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        ids.Count == 0 || await db.ContentTags.AsNoTracking().Where(x => ids.Contains(x.Id)).Select(x => x.Id).CountAsync(cancellationToken) == ids.Distinct().Count();

    private async Task ReplaceProjectTagsAsync(Project project, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken)
    {
        var current = await db.ProjectTags.Where(x => x.ProjectId == project.Id).ToListAsync(cancellationToken);
        db.ProjectTags.RemoveRange(current);
        foreach (var tagId in tagIds.Distinct()) db.ProjectTags.Add(new ProjectTag { ProjectId = project.Id, TagId = tagId, TenantId = db.TenantId });
    }

    private static List<ValidationError> Validate(string slug, string title, string summary, IReadOnlyCollection<Guid> tags)
    {
        var errors = new List<ValidationError>();
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > 200) errors.Add(new("slug.invalid", "Slug is required and must be at most 200 characters."));
        if (string.IsNullOrWhiteSpace(title) || title.Length > 200) errors.Add(new("title.invalid", "Title is required and must be at most 200 characters."));
        if (string.IsNullOrWhiteSpace(summary) || summary.Length > 500) errors.Add(new("summary.invalid", "Summary is required and must be at most 500 characters."));
        if (tags.Any(x => x == Guid.Empty)) errors.Add(new("tag.invalid", "Tag identifiers must be non-empty."));
        return errors;
    }
}
