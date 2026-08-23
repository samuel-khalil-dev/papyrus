using Papyrus.Api.Admin.AdminUi;

namespace Papyrus.Tests;

public sealed class SlugGenerationTests
{
    [Theory]
    [InlineData("  Hello, World!  ", "hello-world")]
    [InlineData("C# & .NET: A Guide", "c-net-a-guide")]
    [InlineData("---already--slug---", "already-slug")]
    public void Slugify_normalizes_titles(string input, string expected) =>
        Assert.Equal(expected, AdminContentSupport.Slugify(input));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("中文 בלבד")]
    public void Slugify_uses_item_for_empty_or_non_ascii_input(string input) =>
        Assert.Equal("item", AdminContentSupport.Slugify(input));

    [Fact]
    public void Slugify_limits_output_to_two_hundred_characters()
    {
        var slug = AdminContentSupport.Slugify(new string('a', 250) + " title");
        Assert.Equal(200, slug.Length);
        Assert.DoesNotContain("-", slug);
    }

    [Fact]
    public void Different_normalized_titles_produce_distinct_slugs()
    {
        var slugs = new[] { "Blog Post", "Portfolio Project", "Release Notes" }
            .Select(AdminContentSupport.Slugify)
            .ToArray();
        Assert.Equal(slugs.Length, slugs.Distinct().Count());
    }
}
