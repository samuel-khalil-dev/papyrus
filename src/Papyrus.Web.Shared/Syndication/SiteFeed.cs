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
            writer.WriteElementString("title", title);
            writer.WriteElementString("link", link);
            writer.WriteElementString("description", description);
            writer.WriteElementString("language", "en");

            foreach (var item in items)
            {
                writer.WriteStartElement("item");
                writer.WriteElementString("title", item.Title);
                writer.WriteElementString("link", item.Link);
                writer.WriteElementString("description", item.Description);
                writer.WriteElementString("pubDate", item.PublishedAt.ToUniversalTime().ToString("R", CultureInfo.InvariantCulture));
                writer.WriteStartElement("guid");
                writer.WriteAttributeString("isPermaLink", "false");
                writer.WriteString(item.Guid);
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
                writer.WriteElementString("loc", entry.Location);
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
}
