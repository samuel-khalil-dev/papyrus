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

    private static ValidationError? EmptyTag(IReadOnlyCollection<Guid> tags) =>
        tags.Any(tag => tag == Guid.Empty)
            ? new ValidationError("tag.invalid", "Tag identifiers must be non-empty.")
            : null;
}
