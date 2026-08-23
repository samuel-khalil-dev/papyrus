using Papyrus.Core.Content;

namespace Papyrus.Tests;

public sealed class MarkdownRendererTests
{
    [Fact]
    public void Empty_markdown_returns_empty_html() => Assert.Equal(string.Empty, MarkdownRenderer.ToSafeHtml(" "));

    [Fact]
    public void Markdown_is_rendered_but_raw_html_is_escaped()
    {
        var html = MarkdownRenderer.ToSafeHtml("# Heading\n\n<script>alert('x')</script>");
        Assert.Contains("<h1", html);
        Assert.DoesNotContain("<script", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;script&gt;", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Links_are_rendered_without_allowing_html_injection()
    {
        var html = MarkdownRenderer.ToSafeHtml("[safe](https://example.com)\n\n[javascript](javascript:alert(1))");
        Assert.Contains("href=\"https://example.com\"", html);
        Assert.DoesNotContain("javascript:", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Dangerous_protocols_are_neutralized_in_links_and_images()
    {
        var html = MarkdownRenderer.ToSafeHtml(
            "[js](java\nscript:alert(1)) [vb](vbscript:msgbox(1)) [data](data:text/html,<script>alert(1)</script>)\n\n![js](javascript:alert(1)) ![data](data:text/html;base64,PHNjcmlwdD4=) ![safe](data:image/png;base64,AAAA)");

        Assert.DoesNotContain("javascript", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("vbscript", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("data:text", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("src=\"data:image/png;base64,AAAA\"", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<script", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Raw_html_and_event_handlers_are_not_emitted_as_active_markup()
    {
        var html = MarkdownRenderer.ToSafeHtml(
            "<img src=\"https://example.com/x\" onerror=\"alert(1)\"><div onclick=\"alert(2)\">text</div><svg><script>alert(3)</script></svg>");

        Assert.DoesNotContain("<img", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<div", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<svg", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onerror", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onclick", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("alert(", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("&lt;img", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Encoded_and_unknown_schemes_are_neutralized()
    {
        var html = MarkdownRenderer.ToSafeHtml(
            "[encoded](java&#x73;cript:alert(1)) [unknown](file:///etc/passwd) [custom](foo:bar)");

        Assert.DoesNotContain("javascript", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("file:", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("foo:bar", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("href=\"#\"", html, StringComparison.OrdinalIgnoreCase);
    }
}
