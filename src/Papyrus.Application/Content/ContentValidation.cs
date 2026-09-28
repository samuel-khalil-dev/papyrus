namespace Papyrus.Application.Content;

/// <summary>
/// Centralized field-rule validation shared by the content and taxonomy handlers.
/// </summary>
public static class ContentValidation
{
    public const int SlugMaxLength = 200;
    public const int NameMaxLength = 200;
    public const int TitleMaxLength = 200;
    public const int SummaryMaxLength = 500;

    /// <summary>
    /// Conflict codes for a slug already held by another row in the same tenant.
    /// <para>
    /// These are not field-validation failures: the request is well formed, and
    /// the request itself is what conflicts. The endpoints key on them to return
    /// 409 rather than 400, in the same way they key on the
    /// <c>*.not_found</c> codes to return 404.
    /// </para>
    /// </summary>
    public static readonly ValidationError BlogPostSlugTaken = new(
        "post.slug_taken", "Another blog post already uses this slug.");

    public static readonly ValidationError ProjectSlugTaken = new(
        "project.slug_taken", "Another project already uses this slug.");

    public static readonly ValidationError CategorySlugTaken = new(
        "category.slug_taken", "Another category already uses this slug.");

    public static readonly ValidationError TagSlugTaken = new(
        "tag.slug_taken", "Another tag already uses this slug.");

    /// <summary>True when a validation error reports a slug conflict.</summary>
    public static bool IsSlugConflict(ValidationError error) =>
        error.Code.EndsWith(".slug_taken", StringComparison.Ordinal);

    /// <summary>True when any of the errors reports a slug conflict.</summary>
    public static bool HasSlugConflict(IEnumerable<ValidationError> errors) => errors.Any(IsSlugConflict);

    public static IReadOnlyList<ValidationError> ForProjectOrPost(string slug, string title, string summary, IReadOnlyCollection<Guid> tags)
    {
        var errors = new List<ValidationError?>
        {
            StringField("slug", "Slug", slug, SlugMaxLength),
            StringField("title", "Title", title, TitleMaxLength),
            StringField("summary", "Summary", summary, SummaryMaxLength),
            EmptyTag(tags)
        };
        return errors.Where(static error => error != null).Cast<ValidationError>().ToList();
    }

    public static IReadOnlyList<ValidationError> ForCategoryOrTag(string codePrefix, string slug, string name)
    {
        // The taxonomy contract exposes a single aggregate error code for slug/name
        // validation, so the category and tag handlers report "{entity}.invalid".
        if (string.IsNullOrWhiteSpace(slug) || slug.Length > SlugMaxLength || string.IsNullOrWhiteSpace(name) || name.Length > NameMaxLength)
        {
            return [new ValidationError($"{codePrefix}.invalid", "Slug and name are required and limited to 200 characters.")];
        }

        return [];
    }

    private static ValidationError? StringField(string code, string displayName, string value, int maxLength)
    {
        if (!string.IsNullOrWhiteSpace(value) && value.Length <= maxLength)
        {
            return null;
        }

        return new ValidationError(
            $"{code}.invalid",
            $"{displayName} is required and must be at most {maxLength} characters.");
    }

    private static ValidationError? EmptyTag(IReadOnlyCollection<Guid>? tags) =>
        // A null collection is the JSON binder's result for an omitted optional
        // tagIds property, and it means "no tags" exactly as an empty array does.
        // Treating it as empty here keeps an unannotated optional field from
        // becoming an unhandled NullReferenceException, and keeps the
        // non-nullable command signature honest about what callers must pass.
        tags?.Any(tag => tag == Guid.Empty) == true
            ? new ValidationError("tag.invalid", "Tag identifiers must be non-empty.")
            : null;
}
