using System.IO;
using System.Windows;
using System.Windows.Controls;
using Pama.Core.Changelog;

namespace Pama.Desktop;

/// <summary>Shows the CHANGELOG.md embedded at build time.</summary>
public partial class ChangelogWindow : Window
{
    private const string ResourceName = "CHANGELOG.md";

    public ChangelogWindow()
    {
        InitializeComponent();
        foreach (var line in ReleaseNotes.ReleasedLines(ReadChangelog()))
        {
            ContentPanel.Children.Add(Render(line));
        }
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private static string ReadChangelog()
    {
        using var stream = typeof(ChangelogWindow).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded resource {ResourceName} is missing from the build");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    // Only the subset of Markdown the changelog uses: ##, ### headings and "- " bullets
    private static TextBlock Render(string line) => line switch
    {
        _ when line.StartsWith("### ") => Block(line[4..], 14, FontWeights.SemiBold, new Thickness(0, 8, 0, 4)),
        _ when line.StartsWith("## ") => Block(line[3..].Replace("[", "").Replace("]", ""), 18, FontWeights.SemiBold,
            new Thickness(0, 16, 0, 0)),
        _ when line.StartsWith("- ") => Block("•  " + line[2..].Replace("`", ""), 13, FontWeights.Normal,
            new Thickness(8, 2, 0, 2)),
        _ => Block(line, 12, FontWeights.Normal, new Thickness(0, 4, 0, 0)),
    };

    private static TextBlock Block(string text, double size, FontWeight weight, Thickness margin) => new()
    {
        Text = text,
        FontSize = size,
        FontWeight = weight,
        Margin = margin,
        TextWrapping = TextWrapping.Wrap,
    };
}
