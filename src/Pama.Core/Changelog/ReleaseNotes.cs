using System.Text.RegularExpressions;

namespace Pama.Core.Changelog;

/// <summary>Extracts the user-facing part of CHANGELOG.md: released versions only.</summary>
public static partial class ReleaseNotes
{
    /// <summary>Returns the non-blank lines from the first released version heading onwards.</summary>
    public static IReadOnlyList<string> ReleasedLines(string markdown) =>
        markdown.Split('\n')
            .Select(line => line.TrimEnd())
            .SkipWhile(line => !VersionHeading().IsMatch(line))
            .Where(line => line.Length > 0)
            .ToList();

    // Same pattern Directory.Build.props uses to read the app version
    [GeneratedRegex(@"^## \[\d+\.\d+\.\d+\]")]
    private static partial Regex VersionHeading();
}
