using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Papyrus.Api.Admin.Media;

namespace Papyrus.Tests;

public sealed class MediaRootResolverTests
{
    [Fact]
    public void Resolve_returns_the_combined_path_when_the_content_root_is_the_application_directory()
    {
        var contentRoot = CreateRoot();
        try
        {
            System.IO.File.WriteAllText(Path.Combine(contentRoot, MediaRootResolver.ProjectFileName), string.Empty);

            var options = new MediaOptions();

            var resolved = MediaRootResolver.Resolve(Environment(contentRoot), options);

            Assert.Equal(Path.Combine(contentRoot, options.RootPath), resolved);
        }
        finally { Delete(contentRoot); }
    }

    [Fact]
    public void Resolve_accepts_a_published_application_directory_without_a_project_file()
    {
        var contentRoot = CreateRoot();
        try
        {
            System.IO.File.WriteAllText(Path.Combine(contentRoot, MediaRootResolver.ApplicationAssemblyName), string.Empty);

            var options = new MediaOptions();

            var resolved = MediaRootResolver.Resolve(Environment(contentRoot), options);

            Assert.Equal(Path.Combine(contentRoot, options.RootPath), resolved);
        }
        finally { Delete(contentRoot); }
    }

    [Fact]
    public void Resolve_rejects_a_relative_root_when_the_content_root_is_not_the_application_directory()
    {
        var contentRoot = CreateRoot();
        try
        {
            var error = Assert.Throws<InvalidOperationException>(
                () => MediaRootResolver.Resolve(Environment(contentRoot), new MediaOptions()));

            Assert.Contains(contentRoot, error.Message, StringComparison.Ordinal);
            Assert.Contains("wwwroot/media", error.Message, StringComparison.Ordinal);
            Assert.Contains("dotnet run --project src/Papyrus.Api.Admin", error.Message, StringComparison.Ordinal);
        }
        finally { Delete(contentRoot); }
    }

    [Fact]
    public void Resolve_honours_an_absolute_root_regardless_of_the_content_root()
    {
        var contentRoot = CreateRoot();
        var absolute = Path.Combine(Path.GetTempPath(), "papyrus-media-absolute");
        try
        {
            var resolved = MediaRootResolver.Resolve(
                Environment(contentRoot),
                new MediaOptions { RootPath = absolute });

            Assert.Equal(absolute, resolved);
        }
        finally { Delete(contentRoot); }
    }

    [Fact]
    public void Resolve_rejects_a_null_environment_or_options()
    {
        Assert.Throws<ArgumentNullException>(() => MediaRootResolver.Resolve(null!, new MediaOptions()));
        Assert.Throws<ArgumentNullException>(() => MediaRootResolver.Resolve(Environment(CreateRoot()), null!));
    }

    private static IWebHostEnvironment Environment(string contentRoot) => new TestEnvironment(contentRoot);

    private static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "papyrus-media-root-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static void Delete(string root)
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private sealed class TestEnvironment(string contentRoot) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Papyrus.Tests";
        public string EnvironmentName { get; set; } = "Test";
        public string WebRootPath { get; set; } = Path.Combine(contentRoot, "wwwroot");
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
