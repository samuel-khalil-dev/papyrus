using System.Globalization;
using System.Xml.Linq;
using Papyrus.Web.Shared.Syndication;

namespace Papyrus.Tests;

/// Covers the syndication generators behind /feed.xml and /sitemap.xml. Both are
/// pure functions over user-authored content, which is what makes them worth
/// asserting: titles, descriptions, and URLs are operator-supplied text, and the
/// feed is the only public surface that serialises them as XML.
public sealed class SiteFeedTests
{
    private static readonly XNamespace Sitemap = "http://www.sitemaps.org/schemas/sitemap/0.9";

    [Fact]
    public void Rss_serialises_the_channel_and_every_item()
    {
        var items = new[]
        {
            new SiteFeedItem("First post", "https://papyrus.test/Post/first", "First summary", new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero), "guid-1"),
            new SiteFeedItem("Second post", "https://papyrus.test/Post/second", "Second summary", new DateTimeOffset(2026, 2, 3, 4, 5, 6, TimeSpan.Zero), "guid-2"),
        };

        var document = XDocument.Parse(SiteFeed.CreateRss("Papyrus", "https://papyrus.test", "Latest writing", items));

        Assert.Equal("rss", document.Root!.Name.LocalName);
        var channel = document.Root.Element("channel")!;
        Assert.Equal("Papyrus", channel.Element("title")!.Value);
        Assert.Equal("https://papyrus.test", channel.Element("link")!.Value);
        var itemElements = channel.Elements("item").ToArray();
        Assert.Equal(2, itemElements.Length);
        Assert.Equal("First post", itemElements[0].Element("title")!.Value);
        Assert.Equal("https://papyrus.test/Post/first", itemElements[0].Element("link")!.Value);
        Assert.Equal("First summary", itemElements[0].Element("description")!.Value);
        Assert.Equal("guid-1", itemElements[0].Element("guid")!.Value);
    }

    [Fact]
    public void Rss_escapes_markup_in_user_authored_text()
    {
        var items = new[]
        {
            new SiteFeedItem("Tom & Jerry <b>bold</b>", "https://papyrus.test/Post/x?a=1&b=2", "A & B", DateTimeOffset.UtcNow, "guid-1"),
        };

        var document = XDocument.Parse(SiteFeed.CreateRss("Papyrus", "https://papyrus.test", "Latest", items));
        var item = document.Root!.Element("channel")!.Element("item")!;

        Assert.Equal("Tom & Jerry <b>bold</b>", item.Element("title")!.Value);
        Assert.Equal("A & B", item.Element("description")!.Value);
        Assert.Equal("https://papyrus.test/Post/x?a=1&b=2", item.Element("link")!.Value);
    }

    [Fact]
    public void Rss_writes_pub_date_as_rfc_1123_in_utc()
    {
        var published = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.FromHours(2));
        var items = new[] { new SiteFeedItem("T", "https://papyrus.test/Post/t", "D", published, "guid-1") };

        var document = XDocument.Parse(SiteFeed.CreateRss("Papyrus", "https://papyrus.test", "Latest", items));
        var value = document.Root!.Element("channel")!.Element("item")!.Element("pubDate")!.Value;

        Assert.EndsWith("GMT", value, StringComparison.Ordinal);
        var parsed = DateTimeOffset.ParseExact(value, "R", CultureInfo.InvariantCulture, DateTimeStyles.None);
        Assert.Equal(published.UtcDateTime, parsed.UtcDateTime);
    }

    [Fact]
    public void Rss_produces_a_valid_document_with_no_items()
    {
        var document = XDocument.Parse(SiteFeed.CreateRss("Papyrus", "https://papyrus.test", "Latest", []));

        Assert.Empty(document.Root!.Element("channel")!.Elements("item"));
    }

    [Fact]
    public void Rss_tolerates_xml_illegal_characters_in_a_title()
    {
        // Operators paste content; a stray control character must not take the
        // feed down. XmlWriter throws on these by default.
        var items = new[]
        {
            new SiteFeedItem("Bad\u0001title", "https://papyrus.test/Post/t", "D", DateTimeOffset.UtcNow, "guid-1"),
        };

        var document = XDocument.Parse(SiteFeed.CreateRss("Papyrus", "https://papyrus.test", "Latest", items));

        Assert.Equal("Badtitle", document.Root!.Element("channel")!.Element("item")!.Element("title")!.Value);
    }

    [Fact]
    public void Sitemap_lists_locations_with_date_only_last_modified()
    {
        var entries = new[]
        {
            new SitemapEntry("https://papyrus.test/", new DateTimeOffset(2026, 3, 4, 5, 6, 7, TimeSpan.Zero)),
            new SitemapEntry("https://papyrus.test/Post/live?x=1&y=2", new DateTimeOffset(2026, 3, 5, 0, 0, 0, TimeSpan.FromHours(-5))),
        };

        var document = XDocument.Parse(SiteFeed.CreateSitemap(entries));
        var urls = document.Root!.Elements(Sitemap + "url").ToArray();

        Assert.Equal(2, urls.Length);
        Assert.Equal("https://papyrus.test/", urls[0].Element(Sitemap + "loc")!.Value);
        Assert.Equal("2026-03-04", urls[0].Element(Sitemap + "lastmod")!.Value);
        Assert.Equal("https://papyrus.test/Post/live?x=1&y=2", urls[1].Element(Sitemap + "loc")!.Value);
        Assert.Equal("2026-03-05", urls[1].Element(Sitemap + "lastmod")!.Value);
    }

    [Fact]
    public void Sitemap_produces_a_valid_document_with_no_entries()
    {
        var document = XDocument.Parse(SiteFeed.CreateSitemap([]));

        Assert.Empty(document.Root!.Elements(Sitemap + "url"));
    }

    [Fact]
    public void Sitemap_tolerates_xml_illegal_characters_in_a_location()
    {
        var entries = new[] { new SitemapEntry("https://papyrus.test/Posts/\u0001bad", DateTimeOffset.UtcNow) };

        var document = XDocument.Parse(SiteFeed.CreateSitemap(entries));

        Assert.Equal("https://papyrus.test/Posts/bad", document.Root!.Element(Sitemap + "url")!.Element(Sitemap + "loc")!.Value);
    }
}
