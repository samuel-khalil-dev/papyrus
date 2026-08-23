using System.ComponentModel.DataAnnotations;

namespace Papyrus.Contracts.Content;

public sealed record ContentCategoryResponse(Guid Id, string Slug, string Name, string? Description, bool IsPublished, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

public sealed record ContentTagResponse(Guid Id, string Slug, string Name, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

public sealed record ProjectSummaryResponse(Guid Id, string Slug, string Title, string Summary, string? CoverImageUrl, bool IsFeatured, bool IsPublished, DateTimeOffset? PublishedAt);

public sealed record BlogPostSummaryResponse(Guid Id, string Slug, string Title, string Summary, bool IsFeatured, bool IsPublished, DateTimeOffset? PublishedAt);

public sealed record ProjectResponse(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string? Description,
    string? RepositoryUrl,
    string? LiveUrl,
    string? CoverImageUrl,
    bool IsFeatured,
    bool IsPublished,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    ContentCategoryResponse? Category,
    IReadOnlyList<ContentTagResponse> Tags,
    IReadOnlyList<BlogPostSummaryResponse> BlogPosts);

public sealed record BlogPostResponse(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string ContentMarkdown,
    string? CoverImageUrl,
    bool IsFeatured,
    bool IsPublished,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    ContentCategoryResponse? Category,
    ProjectSummaryResponse? Project,
    IReadOnlyList<ContentTagResponse> Tags);

public sealed record CategoryUpsertRequest(
    [property: Required, MaxLength(200)] string Slug,
    [property: Required, MaxLength(200)] string Name,
    [property: MaxLength(1000)] string? Description,
    bool IsPublished);

public sealed record TagUpsertRequest(
    [property: Required, MaxLength(200)] string Slug,
    [property: Required, MaxLength(200)] string Name);

public sealed record ProjectUpsertRequest(
    [property: Required, MaxLength(200)] string Slug,
    [property: Required, MaxLength(200)] string Title,
    [property: Required, MaxLength(500)] string Summary,
    [property: MaxLength(20000)] string? Description,
    [property: Url, MaxLength(2000)] string? RepositoryUrl,
    [property: Url, MaxLength(2000)] string? LiveUrl,
    [property: MaxLength(500)] string? CoverImageUrl,
    Guid? CategoryId,
    bool IsFeatured,
    bool IsPublished,
    DateTimeOffset? PublishedAt,
    Guid[] TagIds);

public sealed record BlogPostUpsertRequest(
    [property: Required, MaxLength(200)] string Slug,
    [property: Required, MaxLength(200)] string Title,
    [property: Required, MaxLength(500)] string Summary,
    [property: Required] string ContentMarkdown,
    [property: MaxLength(500)] string? CoverImageUrl,
    Guid? ProjectId,
    Guid? CategoryId,
    bool IsFeatured,
    bool IsPublished,
    DateTimeOffset? PublishedAt,
    Guid[] TagIds);
