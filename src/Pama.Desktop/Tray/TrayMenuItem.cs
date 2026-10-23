namespace Pama.Desktop.Tray;

/// <summary>One entry of the tray right-click menu; a null entry in a list renders as a separator.</summary>
/// <param name="IsChecked">Read each time the menu opens, so the check mark always reflects current state.</param>
/// <param name="Children">When set, the entry opens a submenu and <paramref name="Execute"/> is not used.</param>
public sealed record TrayMenuItem(
    string Text,
    Action Execute,
    Func<bool>? IsChecked = null,
    IReadOnlyList<TrayMenuItem?>? Children = null);
