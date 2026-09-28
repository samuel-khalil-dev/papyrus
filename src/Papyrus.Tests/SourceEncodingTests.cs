using System.Text;

namespace Papyrus.Tests;

/// <summary>
/// Guards source files against an encoding corruption that already happened
/// once in this repository.
/// <para>
/// A PowerShell <c>git show &gt; file</c> redirect wrote UTF-16LE with a BOM
/// into nine files. The C# still parsed on the developer's machine only because
/// the working copy had been repaired independently of the commit, so the
/// corruption shipped and CI failed with 365 CS1056 errors. Nothing in the .NET
/// build catches this locally, because the same file in the working tree is
/// fine; only the committed bytes are wrong.
/// </para>
/// <para>
/// The guard therefore reads the <em>working tree</em>, not the committed blob,
/// and is paired with the CI OpenAPI-style byte check. Reading committed blobs
/// from a test would require a git dependency in the test host and would not
/// survive a shallow clone, so the working-tree check is what keeps a stray
/// redirect from being committed in the first place - it fails on the machine
/// that made the mistake, which is where the mistake is still visible.
/// </para>
/// </summary>
public sealed class SourceEncodingTests
{
    private static readonly string[] TrackedExtensions = [".cs", ".csproj", ".json", ".props", ".targets", ".md"];

    public static TheoryData<string> SourceFiles()
    {
        var data = new TheoryData<string>();
        foreach (var path in TrackedSourceFiles().Where(path =>
                     TrackedExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)))
        {
            data.Add(Path.GetRelativePath(RepositoryRoot(), path).Replace('\\', '/'));
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(SourceFiles))]
    public void A_tracked_file_is_plain_utf8_text(string relativePath)
    {
        var bytes = File.ReadAllBytes(Path.Combine(RepositoryRoot(), relativePath));

        // A UTF-8 BOM is tolerated deliberately: Visual Studio and EF Core both
        // emit one, and eleven tracked files in this repository already have
        // one. The defect this guards is not the BOM - it is the UTF-16 file
        // that a shell redirect produced, which the compiler rejects and which
        // no tool in the build flags locally.
        var isUtf16 = bytes.Length >= 2
            && ((bytes[0] == 0xFF && bytes[1] == 0xFE) || (bytes[0] == 0xFE && bytes[1] == 0xFF));

        Assert.False(isUtf16, $"{relativePath} is UTF-16; it must be UTF-8.");
        Assert.False(ContainsNul(bytes), $"{relativePath} contains a NUL byte, which means it is not plain UTF-8 text.");

        // Throws on genuinely invalid UTF-8. C# source with a stray continuation
        // byte fails the build, but markdown does not, so assert it here.
        var exception = Record.Exception(() => new UTF8Encoding(false, true).GetString(bytes));
        Assert.Null(exception);
    }

    private static bool ContainsNul(byte[] bytes) =>
        Array.IndexOf(bytes, (byte)0) >= 0;

    private static IEnumerable<string> TrackedSourceFiles() =>
        Directory.EnumerateFiles(RepositoryRoot(), "*", SearchOption.AllDirectories)
            .Where(path => !IsExcluded(path));

    private static bool IsExcluded(string path)
    {
        var normalized = path.Replace('\\', '/');
        string[] excluded =
        [
            "/.git/", "/bin/", "/obj/", "/node_modules/", "/dist/", "/.claude/", "/.kilo/"
        ];
        return excluded.Any(segment => normalized.Contains(segment, StringComparison.OrdinalIgnoreCase));
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Papyrus.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate the repository root.");
    }
}
