using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Papyrus.Api.Admin.Media;
using Papyrus.Infrastructure.MultiTenancy;

namespace Papyrus.Tests;

public sealed class MediaValidationTests
{
    private const string OnePixelPng = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=";

    [Fact]
    public async Task Save_rejects_svg_and_extension_signature_mismatches()
    {
        var root = CreateRoot();
        try
        {
            var service = CreateService(root);
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveAsync(CreateFile("<svg></svg>", "image/svg+xml", "x.svg")));
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveAsync(CreateFile(Convert.FromBase64String(OnePixelPng), "image/jpeg", "x.jpg")));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task Save_uses_detected_raster_type_and_tenant_isolated_path()
    {
        var root = CreateRoot();
        try
        {
            var result = await CreateService(root).SaveAsync(CreateFile(Convert.FromBase64String(OnePixelPng), "application/octet-stream", "cover.png"));
            Assert.Equal("image/png", result.ContentType);
            Assert.Matches("^/media/main/[0-9a-f]{32}\\.png$", result.Url);
            Assert.True(System.IO.File.Exists(Path.Combine(root, "main", result.FileName)));
        }
        finally { if (Directory.Exists(root)) Directory.Delete(root, recursive: true); }
    }

    private static LocalMediaService CreateService(string root) => new(
        new TestEnvironment(root),
        Options.Create(new MediaOptions { RootPath = root, RequestPath = "/media", MaxFileBytes = 1024 * 1024 }),
        new TenantContext("main"));

    private static FormFile CreateFile(byte[] bytes, string contentType, string name)
    {
        var file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", name)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
        return file;
    }

    private static FormFile CreateFile(string text, string contentType, string name) => CreateFile(System.Text.Encoding.UTF8.GetBytes(text), contentType, name);
    private static string CreateRoot() => Path.Combine(Path.GetTempPath(), "papyrus-media-tests", Guid.NewGuid().ToString("N"));

    private sealed class TestEnvironment(string root) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Papyrus.Tests";
        public string EnvironmentName { get; set; } = "Test";
        public string WebRootPath { get; set; } = root;
        public string ContentRootPath { get; set; } = root;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
