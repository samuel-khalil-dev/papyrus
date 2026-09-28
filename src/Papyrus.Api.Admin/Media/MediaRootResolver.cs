namespace Papyrus.Api.Admin.Media;

/// Resolves the media storage root and rejects a launch that would store media
/// outside the application.
/// <para>
/// A relative <see cref="MediaOptions.RootPath"/> is resolved against the
/// content root, which is the working directory the host was launched from. When
/// that is not the application's own directory the media root lands outside the
/// project, where it is neither published nor versioned: an upload would be
/// accepted with a success response and then be absent from every container
/// image, build, and fresh clone, because <c>wwwroot/media</c> is ignored by
/// design. That is a startup failure rather than a silent one.
/// </para>
internal static class MediaRootResolver
{
    internal const string ProjectFileName = "Papyrus.Api.Admin.csproj";

    internal const string ApplicationAssemblyName = "Papyrus.Api.Admin.dll";

    internal static string Resolve(IWebHostEnvironment environment, MediaOptions options)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(options);

        // An absolute root is unambiguous operator intent, so it is honoured
        // whatever the content root happens to be.
        if (Path.IsPathRooted(options.RootPath))
        {
            return options.RootPath;
        }

        var contentRoot = environment.ContentRootPath;
        if (!IsApplicationDirectory(contentRoot))
        {
            throw new InvalidOperationException(
                $"Media:RootPath '{options.RootPath}' is relative, but the content root '{contentRoot}' is not the "
                + "Papyrus.Api.Admin application directory, so uploaded media would be stored outside the project, "
                + "where it is neither published nor versioned. Start the API from its project directory with "
                + "'dotnet run --project src/Papyrus.Api.Admin', or set Media:RootPath to an absolute path.");
        }

        return Path.Combine(contentRoot, options.RootPath);
    }

    private static bool IsApplicationDirectory(string contentRoot) =>
        File.Exists(Path.Combine(contentRoot, ProjectFileName))
        || File.Exists(Path.Combine(contentRoot, ApplicationAssemblyName));
}
