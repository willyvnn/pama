using Pama.Core.Generation;

namespace Pama.Core.Settings;

/// <summary>User preferences kept between runs; contains no secrets.</summary>
public sealed record AppSettings
{
    public bool AutoClearClipboard { get; init; } = true;

    public bool IsValueMasked { get; init; }

    /// <summary>The close button hides the window to the notification area instead of exiting.</summary>
    public bool CloseToTray { get; init; } = true;

    /// <summary>The one-time "still running in the tray" notification has been shown.</summary>
    public bool HasShownTrayHint { get; init; }

    /// <summary>Used by the Password tab and by the tray's "Copy new Password".</summary>
    public PasswordOptions Password { get; init; } = new();
}
