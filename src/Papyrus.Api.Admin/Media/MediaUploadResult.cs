namespace Papyrus.Api.Admin.Media;

public sealed record MediaUploadResult(string Url, string FileName, string ContentType, long Size);
