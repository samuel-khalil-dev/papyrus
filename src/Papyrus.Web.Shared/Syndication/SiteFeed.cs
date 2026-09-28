using System.Globalization;
using System.Text;
using System.Xml;

namespace Papyrus.Web.Shared.Syndication;

public sealed record SiteFeedItem(string Title, string Link, string Description, DateTimeOffset PublishedAt, string Guid);

public sealed record SitemapEntry(string Location, DateTimeOffset LastModified);

public static class SiteFeed
{
    public static string CreateRss(string title, string link, string description, IReadOnlyList<SiteFeedItem> items)
    {
        var settings = CreateWriterSettings();
        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("rss");
            writer.WriteAttributeString("version", "2.0");
            writer.WriteStartElement("channel");
            writer.WriteElementString("title", Sanitize(title));
            writer.WriteElementString("link", Sanitize(link));
            writer.WriteElementString("description", Sanitize(description));
            writer.WriteElementString("language", "en");

            foreach (var item in items)
            {
                writer.WriteStartElement("item");
                writer.WriteElementString("title", Sanitize(item.Title));
                writer.WriteElementString("link", Sanitize(item.Link));
                writer.WriteElementString("description", Sanitize(item.Description));
                writer.WriteElementString("pubDate", item.PublishedAt.ToUniversalTime().ToString("R", CultureInfo.InvariantCulture));
                writer.WriteStartElement("guid");
                writer.WriteAttributeString("isPermaLink", "false");
                writer.WriteString(Sanitize(item.Guid));
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static string CreateSitemap(IReadOnlyList<SitemapEntry> entries)
    {
        var settings = CreateWriterSettings();
        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

            foreach (var entry in entries)
            {
                writer.WriteStartElement("url");
                writer.WriteElementString("loc", Sanitize(entry.Location));
                writer.WriteElementString("lastmod", entry.LastModified.ToUniversalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static XmlWriterSettings CreateWriterSettings() => new()
    {
        Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
        Indent = true,
        OmitXmlDeclaration = false
    };

    /// Every value written here originates in operator-authored content, and
    /// XmlWriter throws on characters XML forbids. A stray control character in a
    /// post title would otherwise turn both syndication endpoints into 500s, so
    /// illegal characters are dropped rather than allowed to break the document.
    /// Valid surrogate pairs are preserved so astral characters survive intact.
    private static string Sanitize(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];
            if (XmlConvert.IsXmlChar(character))
            {
                builder.Append(character);
            }
            else if (index + 1 < value.Length && XmlConvert.IsXmlSurrogatePair(value[index + 1], character))
            {
                builder.Append(character).Append(value[++index]);
            }
        }

        return builder.ToString();
    }
}
