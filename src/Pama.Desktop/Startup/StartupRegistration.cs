using Microsoft.Win32;

namespace Pama.Desktop.Startup;

/// <summary>"Start with Windows" via the per-user Run key; the registry, not settings.json, is the source of truth.</summary>
public static class StartupRegistration
{
    /// <summary>Command-line flag the Run entry passes, so a login start goes straight to the tray.</summary>
    public const string MinimizedArgument = "--minimized";

    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "Pama";

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath);
        return key?.GetValue(ValueName) is string;
    }

    public static void SetEnabled(bool isEnabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath);
        if (isEnabled)
        {
            key.SetValue(ValueName, Command());
            return;
        }

        key.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    // The exe may have been moved or replaced by a new version since the entry was written
    public static void RefreshPathIfEnabled()
    {
        if (IsEnabled())
        {
            SetEnabled(true);
        }
    }

    private static string Command() => $"\"{Environment.ProcessPath}\" {MinimizedArgument}";
}
