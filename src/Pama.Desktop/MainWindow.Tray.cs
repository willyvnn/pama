using System.ComponentModel;
using System.Windows;
using Pama.Core.Generation;
using Pama.Desktop.Startup;
using Pama.Desktop.Tray;

namespace Pama.Desktop;

// Notification-area behaviour: close to tray, quick copy and settings from the right-click menu
public partial class MainWindow
{
    private static readonly int[] TrayPasswordLengths = [12, 16, 20, 24, 32, 64];

    /// <summary>Login start: the window stays hidden and only the tray icon appears.</summary>
    public void StartInTray() => _trayIcon.Show();

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_isExiting || CloseToTrayCheckBox.IsChecked != true)
        {
            return;
        }

        e.Cancel = true;
        _trayIcon.Show();
        Hide();
        ShowTrayHintOnce();
    }

    // Without this, a user who clicks X believes Pama has exited while it keeps running
    private void ShowTrayHintOnce()
    {
        if (_hasShownTrayHint)
        {
            return;
        }

        _trayIcon.Notify(Text("Tray.StillRunning"));
        _hasShownTrayHint = true;
        SaveSettings();
    }

    /// <summary>Really exits, bypassing close-to-tray; the clipboard is cleared if a countdown is running.</summary>
    private void ExitApp()
    {
        _isExiting = true;
        Close();
    }

    // Covers every way back: tray click, tray menu, and a second launch via App.BringMainWindowToFront
    private void HideTrayWhenVisible()
    {
        if (IsVisible)
        {
            _trayIcon.Hide();
        }
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    // Goes through the window's own select-generate-copy path, so reopening shows exactly what is on the clipboard
    private void QuickCopy(SecretKind kind)
    {
        SelectKind(kind);
        var result = Generate();
        if (!result.IsSuccess)
        {
            _trayIcon.Notify(Text($"Generator.Error.{result.Error}"));
            return;
        }

        Copy();
        var messageKey = _clipboard.IsCountingDown ? "Tray.CopiedWithCountdown" : "Tray.Copied";
        _trayIcon.Notify(string.Format(Text(messageKey), Text($"Generator.Kind.{kind}"), ClipboardGuard.LifetimeSeconds));
    }

    private List<TrayMenuItem?> CreateTrayMenu() =>
    [
        new(Text("Tray.Open"), RestoreFromTray),
        null,
        .. Enum.GetValues<SecretKind>().Select(kind => new TrayMenuItem(
            string.Format(Text("Tray.CopyNew"), Text($"Generator.Kind.{kind}")), () => QuickCopy(kind))),
        null,
        new(Text("Tray.Settings"), () => { }, Children:
        [
            new(Text("Generator.AutoClear"), ToggleAutoClear, IsChecked: () => AutoClearCheckBox.IsChecked == true),
            new(Text("Tray.MaskValues"), ToggleReveal, IsChecked: () => !_isValueRevealed),
            new(Text("Settings.CloseToTray"), ToggleCloseToTray, IsChecked: () => CloseToTrayCheckBox.IsChecked == true),
            new(Text("Settings.StartWithWindows"), () => SetStartWithWindows(!StartupRegistration.IsEnabled()),
                IsChecked: StartupRegistration.IsEnabled),
            null,
            new(Text("Tray.PasswordOptions"), () => { }, Children: CreatePasswordMenu()),
        ]),
        null,
        new(Text("Tray.Exit"), ExitApp),
    ];

    private void ToggleCloseToTray()
    {
        CloseToTrayCheckBox.IsChecked = CloseToTrayCheckBox.IsChecked != true;
        SaveSettings();
    }

    private void ToggleAutoClear()
    {
        AutoClearCheckBox.IsChecked = AutoClearCheckBox.IsChecked != true;
        OnAutoClearChanged(AutoClearCheckBox, new RoutedEventArgs());
    }

    private List<TrayMenuItem?> CreatePasswordMenu() =>
    [
        PasswordClassItem("Generator.Lowercase", o => o.IncludeLowercase, (o, on) => o with { IncludeLowercase = on }),
        PasswordClassItem("Generator.Uppercase", o => o.IncludeUppercase, (o, on) => o with { IncludeUppercase = on }),
        PasswordClassItem("Generator.Digits", o => o.IncludeDigits, (o, on) => o with { IncludeDigits = on }),
        PasswordClassItem("Generator.Symbols", o => o.IncludeSymbols, (o, on) => o with { IncludeSymbols = on }),
        new(Text("Generator.ExcludeAmbiguous"),
            () => UpdatePasswordOptions(_passwordOptions with { ExcludeAmbiguous = !_passwordOptions.ExcludeAmbiguous }),
            IsChecked: () => _passwordOptions.ExcludeAmbiguous),
        null,
        .. TrayPasswordLengths.Select(length => new TrayMenuItem(
            string.Format(Text("Tray.LengthCharacters"), length),
            () => UpdatePasswordOptions(_passwordOptions with { Length = length }),
            IsChecked: () => _passwordOptions.Length == length)),
    ];

    private TrayMenuItem PasswordClassItem(
        string textKey, Func<PasswordOptions, bool> isIncluded, Func<PasswordOptions, bool, PasswordOptions> toggle) =>
        new(Text(textKey), () => TogglePasswordClass(isIncluded, toggle), IsChecked: () => isIncluded(_passwordOptions));

    // Unlike the window, the tray has nowhere to show the "select at least one" error, so the last class stays on
    private void TogglePasswordClass(
        Func<PasswordOptions, bool> isIncluded, Func<PasswordOptions, bool, PasswordOptions> toggle)
    {
        var updated = toggle(_passwordOptions, !isIncluded(_passwordOptions));
        if (SecretGenerator.PasswordEntropyBits(updated) > 0)
        {
            UpdatePasswordOptions(updated);
        }
    }

}
