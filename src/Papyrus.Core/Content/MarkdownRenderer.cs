using Markdig;
using System.Net;
using System.Text.RegularExpressions;

namespace Papyrus.Core.Content;

public static class MarkdownRenderer
{
    private static readonly Regex UrlAttribute = new(
        "(?<prefix>\\b(?<name>href|src)\\s*=\\s*)(?<quote>[\\\"\\'])(?<url>.*?)(?:\\k<quote>)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

    private static readonly Regex EventHandlerAttribute = new(
        "\\s+\\bon[a-z][a-z0-9_:-]*\\s*=\\s*(?:\\\"[^\\\"]*\\\"|'[^']*'|[^\\s>]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex Scheme = new(
        "^[a-z][a-z0-9+.-]*:",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex SafeDataImage = new(
        "^data:image/(?:gif|png|jpe?g|webp|avif);base64,[a-z0-9+/]+=*$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    public static string ToSafeHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var html = Markdown.ToHtml(markdown, Pipeline);

        // HTML is disabled above, but keep this final boundary defensive: the result is
        // intentionally emitted with Html.Raw by the public detail pages.
        html = EventHandlerAttribute.Replace(html, string.Empty);
        return UrlAttribute.Replace(html, SanitizeUrlAttribute);
    }

    private static string SanitizeUrlAttribute(Match match)
    {
        var url = match.Groups["url"].Value;
        var normalizedUrl = WebUtility.HtmlDecode(url);
        normalizedUrl = string.Concat(normalizedUrl.Where(character => !char.IsControl(character) && !char.IsWhiteSpace(character)));
        var attributeName = match.Groups["name"].Value;
        var isImage = attributeName.Equals("src", StringComparison.OrdinalIgnoreCase);

        var isSafe = isImage
            ? IsSafeImageUrl(normalizedUrl)
            : IsSafeLinkUrl(normalizedUrl);

        return $"{match.Groups["prefix"].Value}{match.Groups["quote"].Value}{(isSafe ? url : "#")}{match.Groups["quote"].Value}";
    }

    private static bool IsSafeLinkUrl(string url)
    {
        if (string.IsNullOrEmpty(url) || url.StartsWith('#') || url.StartsWith('/') || url.StartsWith('.') || url.StartsWith('?'))
        {
            return true;
        }

        return url.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)
            || !Scheme.IsMatch(url);
    }

    private static bool IsSafeImageUrl(string url)
    {
        if (url.StartsWith("https:", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("http:", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(url)
            || url.StartsWith('/')
            || url.StartsWith('.')
            || url.StartsWith('?'))
        {
            return true;
        }

        return SafeDataImage.IsMatch(url);
    }
}
