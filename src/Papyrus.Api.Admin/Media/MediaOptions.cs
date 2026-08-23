namespace Papyrus.Api.Admin.Media;

public sealed class MediaOptions
{
    public const string SectionName = "Media";

    public string RootPath { get; set; } = "wwwroot/media";

    public string RequestPath { get; set; } = "/media";

    public long MaxFileBytes { get; set; } = 5 * 1024 * 1024;

    public int MaxWidth { get; set; } = 10000;

    public int MaxHeight { get; set; } = 10000;

    public long MaxPixels { get; set; } = 40_000_000;
}
