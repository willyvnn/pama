using Pama.Core.Changelog;

namespace Pama.Core.Tests.Changelog;

public sealed class ReleaseNotesTests
{
    private const string Changelog = """
        # Changelog

        All notable changes to this project are documented here.

        ## [Unreleased]
        ### Added
        - Work in progress

        ## [0.2.0] - 2026-11-01

        ### Added
        - Passphrase

        ## [0.1.0] - 2026-10-23
        - Generator
        """;

    [Fact]
    public void should_start_at_first_released_version_when_changelog_has_header_and_unreleased()
    {
        var lines = ReleaseNotes.ReleasedLines(Changelog);

        Assert.Equal(
            ["## [0.2.0] - 2026-11-01", "### Added", "- Passphrase", "## [0.1.0] - 2026-10-23", "- Generator"],
            lines);
    }

    [Fact]
    public void should_return_empty_when_nothing_is_released()
    {
        var lines = ReleaseNotes.ReleasedLines("# Changelog\r\n\r\n## [Unreleased]\r\n- Draft\r\n");

        Assert.Empty(lines);
    }
}
