namespace Papyrus.Api.Admin.Media;

public sealed record MediaFileResult(string FileName, string Url, long Size, DateTimeOffset LastModified);
