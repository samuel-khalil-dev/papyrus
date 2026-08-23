namespace Papyrus.Contracts.Content;

public sealed record PublicCategory(string Slug, string Name);

public sealed record PublicTag(string Slug, string Name);

public sealed record PublicProjectCard(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string? CoverImageUrl,
    bool IsFeatured,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt,
    PublicCategory? Category,
    IReadOnlyList<PublicTag> Tags);

public sealed record PublicPostCard(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string? CoverImageUrl,
    bool IsFeatured,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt,
    PublicCategory? Category,
    PublicProjectCard? Project,
    IReadOnlyList<PublicTag> Tags);

public sealed record PublicProjectDetail(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string? Description,
    string? RepositoryUrl,
    string? LiveUrl,
    string? CoverImageUrl,
    bool IsFeatured,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt,
    PublicCategory? Category,
    IReadOnlyList<PublicTag> Tags,
    IReadOnlyList<PublicPostCard> BlogPosts);

public sealed record PublicPostDetail(
    Guid Id,
    string Slug,
    string Title,
    string Summary,
    string ContentMarkdown,
    string? CoverImageUrl,
    bool IsFeatured,
    DateTimeOffset? PublishedAt,
    DateTimeOffset CreatedAt,
    PublicCategory? Category,
    PublicProjectCard? Project,
    IReadOnlyList<PublicTag> Tags);

public sealed record PublicBlogIndexResult(
    PublicPostCard? FeaturedPost,
    IReadOnlyList<PublicPostCard> Posts,
    IReadOnlyList<PublicTag> Tags,
    int TotalPosts,
    int TotalPages);

public sealed record PublicPortfolioIndexResult(
    PublicProjectCard? FeaturedProject,
    IReadOnlyList<PublicProjectCard> Projects,
    IReadOnlyList<PublicTag> Tags,
    int TotalProjects,
    int TotalPages);

public sealed record PublicPostPageResult(
    PublicPostDetail Post,
    IReadOnlyList<PublicPostCard> RelatedPosts,
    IReadOnlyList<PublicTag> AllTags);

public sealed record PublicProjectPageResult(
    PublicProjectDetail Project,
    IReadOnlyList<PublicProjectCard> RelatedProjects,
    IReadOnlyList<PublicPostCard> RelatedPosts,
    IReadOnlyList<PublicTag> AllTags);
