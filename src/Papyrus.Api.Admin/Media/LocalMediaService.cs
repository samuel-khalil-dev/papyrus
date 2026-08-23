using Microsoft.Extensions.Options;
using Papyrus.Infrastructure.MultiTenancy;
using System.Buffers.Binary;
using System.Text.RegularExpressions;

namespace Papyrus.Api.Admin.Media;

public sealed class LocalMediaService(IWebHostEnvironment environment, IOptions<MediaOptions> options, ITenantContext tenantContext)
{
    private static readonly Regex GeneratedFileName = new(@"^[0-9a-f]{32}\.(jpg|jpeg|png|gif|webp)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public async Task<MediaUploadResult> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        var settings = options.Value;
        if (file.Length <= 0 || file.Length > settings.MaxFileBytes)
        {
            throw new InvalidOperationException($"The uploaded file must be between 1 and {settings.MaxFileBytes} bytes.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Only jpeg, png, gif, and webp images are allowed. SVG is not accepted.");
        }

        await using var input = file.OpenReadStream();
        using var buffer = new MemoryStream((int)Math.Min(file.Length, settings.MaxFileBytes));
        await input.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length != file.Length)
        {
            throw new InvalidOperationException("The uploaded file changed while it was being read.");
        }

        var detected = Detect(buffer.GetBuffer().AsSpan(0, checked((int)buffer.Length)), settings, extension);
        var root = GetMediaRoot();
        var tenantRoot = Path.Combine(root, tenantContext.TenantId);
        Directory.CreateDirectory(tenantRoot);
        var fileName = $"{Guid.NewGuid():N}{detected.Extension}";
        var destination = Path.Combine(tenantRoot, fileName);
        var temporary = Path.Combine(tenantRoot, $".{Guid.NewGuid():N}.upload");
        try
        {
            await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 64 * 1024, useAsync: true))
            {
                buffer.Position = 0;
                await buffer.CopyToAsync(output, cancellationToken);
            }

            File.Move(temporary, destination, overwrite: false);
        }
        catch
        {
            if (File.Exists(temporary)) File.Delete(temporary);
            throw;
        }

        return new MediaUploadResult($"{GetRequestPath()}/{tenantContext.TenantId}/{fileName}", fileName, detected.ContentType, buffer.Length);
    }

    public async Task<IReadOnlyList<MediaFileResult>> ListAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tenantRoot = Path.Combine(GetMediaRoot(), tenantContext.TenantId);
        if (!Directory.Exists(tenantRoot)) return [];
        // System.IO has no async directory enumerator on the target Windows/.NET
        // surface. Offload the potentially slow directory walk and preserve request
        // cancellation rather than blocking the request thread.
        var paths = await Task.Run(() => Directory.EnumerateFiles(tenantRoot).ToArray(), cancellationToken);
        var files = new List<FileInfo>(paths.Length);
        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var file = new FileInfo(path);
            if (GeneratedFileName.IsMatch(file.Name)) files.Add(file);
        }

        return files
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .Select(file => new MediaFileResult(file.Name, $"{GetRequestPath()}/{tenantContext.TenantId}/{file.Name}", file.Length, new DateTimeOffset(file.LastWriteTimeUtc, TimeSpan.Zero)))
            .ToList();
    }

    public Task<bool> DeleteAsync(string fileName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!GeneratedFileName.IsMatch(fileName)) return Task.FromResult(false);
        var path = Path.Combine(GetMediaRoot(), tenantContext.TenantId, fileName);
        try
        {
            File.Delete(path);
            return Task.FromResult(true);
        }
        catch (FileNotFoundException) { return Task.FromResult(false); }
        catch (DirectoryNotFoundException) { return Task.FromResult(false); }
    }

    private static DetectedMedia Detect(ReadOnlySpan<byte> data, MediaOptions settings, string extension)
    {
        DetectedMedia detected;
        if (data.Length >= 24 && data[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a }) && data[^12..].SequenceEqual(new byte[] { 0, 0, 0, 0, 0x49, 0x45, 0x4e, 0x44, 0xae, 0x42, 0x60, 0x82 }))
        {
            detected = new(".png", "image/png", BinaryPrimitives.ReadInt32BigEndian(data[16..]), BinaryPrimitives.ReadInt32BigEndian(data[20..]));
        }
        else if (data.Length >= 10 && (data[..6].SequenceEqual("GIF87a"u8) || data[..6].SequenceEqual("GIF89a"u8)))
        {
            detected = new(".gif", "image/gif", BinaryPrimitives.ReadUInt16LittleEndian(data[6..]), BinaryPrimitives.ReadUInt16LittleEndian(data[8..]));
        }
        else if (data.Length >= 12 && data[..4].SequenceEqual("RIFF"u8) && data[8..12].SequenceEqual("WEBP"u8))
        {
            detected = DetectWebp(data);
        }
        else if (data.Length >= 4 && data[0] == 0xff && data[1] == 0xd8 && data[^2] == 0xff && data[^1] == 0xd9)
        {
            detected = DetectJpeg(data);
        }
        else
        {
            throw new InvalidOperationException("The file signature does not match an allowed image format.");
        }

        if (!string.Equals(extension, detected.Extension, StringComparison.OrdinalIgnoreCase)
            && !(extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) && detected.Extension == ".jpeg"))
        {
            throw new InvalidOperationException("The file extension does not match its content.");
        }
        if (detected.Width <= 0 || detected.Height <= 0 || detected.Width > settings.MaxWidth || detected.Height > settings.MaxHeight
            || (long)detected.Width * detected.Height > settings.MaxPixels)
        {
            throw new InvalidOperationException("The image dimensions exceed the configured safety limits.");
        }
        return detected;
    }

    private static DetectedMedia DetectWebp(ReadOnlySpan<byte> data)
    {
        if (data.Length < 30 || !data[12..16].SequenceEqual("VP8X"u8)) throw new InvalidOperationException("Only validated WebP images are accepted.");
        var width = 1 + data[24] + (data[25] << 8) + (data[26] << 16);
        var height = 1 + data[27] + (data[28] << 8) + (data[29] << 16);
        return new(".webp", "image/webp", width, height);
    }

    private static DetectedMedia DetectJpeg(ReadOnlySpan<byte> data)
    {
        var index = 2;
        while (index + 9 < data.Length)
        {
            if (data[index++] != 0xff) continue;
            while (index < data.Length && data[index] == 0xff) index++;
            var marker = data[index++];
            if (marker is 0xd8 or 0xd9) continue;
            if (index + 2 > data.Length) break;
            var length = BinaryPrimitives.ReadUInt16BigEndian(data[index..]);
            if (length < 2 || index + length > data.Length) break;
            if (marker is >= 0xc0 and <= 0xc3 or >= 0xc5 and <= 0xc7 or >= 0xc9 and <= 0xcb or >= 0xcd and <= 0xcf)
            {
                return new(".jpeg", "image/jpeg", BinaryPrimitives.ReadUInt16BigEndian(data[(index + 5)..]), BinaryPrimitives.ReadUInt16BigEndian(data[(index + 3)..]));
            }
            index += length;
        }
        throw new InvalidOperationException("The JPEG dimensions could not be validated.");
    }

    private string GetMediaRoot() => Path.IsPathRooted(options.Value.RootPath) ? options.Value.RootPath : Path.Combine(environment.ContentRootPath, options.Value.RootPath);
    private string GetRequestPath() => string.IsNullOrWhiteSpace(options.Value.RequestPath) ? "/media" : options.Value.RequestPath.TrimEnd('/');
    private sealed record DetectedMedia(string Extension, string ContentType, int Width, int Height);
}
