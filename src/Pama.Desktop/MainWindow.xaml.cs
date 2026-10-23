using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Pama.Core.Generation;
using Pama.Core.Settings;
using Pama.Desktop.Startup;
using Pama.Desktop.Tray;

namespace Pama.Desktop;

/// <summary>Secret generator window.</summary>
public partial class MainWindow : Window
{
    private const double FairEntropyBits = 60;
    private const double StrongEntropyBits = 80;
    private const double VeryStrongEntropyBits = 128;
    private const int ShortValueLength = 20;
    private const int MediumValueLength = 36;
    private const double LargeFontSize = 18;
    private const double MediumFontSize = 15;
    private const double SmallFontSize = 12;
    private const string CopyGlyph = "\uE8C8";
    private const string CheckGlyph = "\uE73E";
    private const string ShowGlyph = "\uE890";
    private const string HideGlyph = "\uED1A";
    private const char MaskCharacter = '\u25CF';

    private readonly SettingsStore _settingsStore = SettingsStore.ForCurrentUser();
    private readonly TrayIcon _trayIcon;
    private readonly ClipboardGuard _clipboard = new();
    private string _currentValue = string.Empty;
    private bool _isValueRevealed;
    private bool _isCopyFeedbackVisible;
    private PasswordOptions _passwordOptions = new();
    private bool _hasShownTrayHint;
    private bool _isExiting;

    public MainWindow()
    {
        InitializeComponent();
        _clipboard.CountdownChanged += (_, secondsLeft) => OnCountdownChanged(secondsLeft);
        SourceInitialized += (_, _) => ProcessHardening.ExcludeFromScreenCapture(this);
        Closed += (_, _) => _clipboard.ClearIfCountingDown();
        _trayIcon = new TrayIcon(this, Text("Generator.Title"), CreateTrayMenu());
        _trayIcon.Clicked += (_, _) => RestoreFromTray();
        Closing += OnClosing;
        // Logoff and shutdown must really exit, not hide to the tray
        Application.Current.SessionEnding += (_, _) => _isExiting = true;
        IsVisibleChanged += (_, _) => HideTrayWhenVisible();
        Closed += (_, _) => _trayIcon.Dispose();
        InputBindings.Add(new KeyBinding(new RelayCommand(() => Generate()), Key.F5, ModifierKeys.None));
        InputBindings.Add(new KeyBinding(new RelayCommand(Copy), Key.C, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(ToggleReveal), Key.H, ModifierKeys.Control));
        InputBindings.Add(new KeyBinding(new RelayCommand(ExitApp), Key.Q, ModifierKeys.Control));
        // The box's own Copy would put the mask (or the bare value) on the clipboard, bypassing SensitiveClipboard
        OutputTextBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, (_, _) => Copy()));
        OutputTextBox.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, (_, _) => Copy()));
        VersionButton.Content = string.Format(Text("App.Version"), AppVersion.Current);
        CountdownBar.Maximum = ClipboardGuard.LifetimeSeconds;

        var settings = _settingsStore.Load();
        AutoClearCheckBox.IsChecked = settings.AutoClearClipboard;
        _isValueRevealed = !settings.IsValueMasked;
        _passwordOptions = settings.Password;
        CloseToTrayCheckBox.IsChecked = settings.CloseToTray;
        _hasShownTrayHint = settings.HasShownTrayHint;
        ShowPasswordOptions();
        StartWithWindowsCheckBox.IsChecked = StartupRegistration.IsEnabled();
        UpdateRevealButton();
        HideCopyFeedback();
        ApplyKind();
    }

    private SecretKind SelectedKind => Enum.Parse<SecretKind>(
        (string)KindPanel.Children.OfType<RadioButton>().Single(r => r.IsChecked == true).Tag);

    // Checking the radio raises OnKindChanged, which applies the kind's length range
    private void SelectKind(SecretKind kind) =>
        KindPanel.Children.OfType<RadioButton>().Single(r => (string)r.Tag == kind.ToString()).IsChecked = true;

    private void OnKindChanged(object sender, RoutedEventArgs e)
    {
        // Fires during InitializeComponent before the named controls below exist
        if (LengthSlider is null)
        {
            return;
        }

        ApplyKind();
    }

    private void OnLengthChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateLengthLabel();
        if (!IsLoaded)
        {
            return;
        }

        if (SelectedKind == SecretKind.Password)
        {
            UpdatePasswordOptions(_passwordOptions with { Length = (int)LengthSlider.Value });
            return;
        }

        Generate();
    }

    private void OnPasswordOptionChanged(object sender, RoutedEventArgs e) =>
        UpdatePasswordOptions(_passwordOptions with
        {
            IncludeLowercase = LowercaseToggle.IsChecked == true,
            IncludeUppercase = UppercaseToggle.IsChecked == true,
            IncludeDigits = DigitsToggle.IsChecked == true,
            IncludeSymbols = SymbolsToggle.IsChecked == true,
            ExcludeAmbiguous = ExcludeAmbiguousCheckBox.IsChecked == true,
        });

    // Single entry point for window and tray edits: keeps the controls, settings.json and the value in step
    private void UpdatePasswordOptions(PasswordOptions options)
    {
        _passwordOptions = options;
        ShowPasswordOptions();
        SaveSettings();
        if (SelectedKind == SecretKind.Password)
        {
            Generate();
        }
    }

    private void ShowPasswordOptions()
    {
        LowercaseToggle.IsChecked = _passwordOptions.IncludeLowercase;
        UppercaseToggle.IsChecked = _passwordOptions.IncludeUppercase;
        DigitsToggle.IsChecked = _passwordOptions.IncludeDigits;
        SymbolsToggle.IsChecked = _passwordOptions.IncludeSymbols;
        ExcludeAmbiguousCheckBox.IsChecked = _passwordOptions.ExcludeAmbiguous;
        if (SelectedKind == SecretKind.Password && (int)LengthSlider.Value != _passwordOptions.Length)
        {
            LengthSlider.Value = _passwordOptions.Length;
        }
    }

    private void OnCloseToTrayChanged(object sender, RoutedEventArgs e) => SaveSettings();

    private void OnStartWithWindowsChanged(object sender, RoutedEventArgs e) =>
        SetStartWithWindows(StartWithWindowsCheckBox.IsChecked == true);

    private void SetStartWithWindows(bool isEnabled)
    {
        try
        {
            StartupRegistration.SetEnabled(isEnabled);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or System.Security.SecurityException or IOException)
        {
            ShowStatus(StatusLevel.Warning, Text("Settings.StartupFailed"));
        }

        StartWithWindowsCheckBox.IsChecked = StartupRegistration.IsEnabled();
    }

    private void OnGenerateClick(object sender, RoutedEventArgs e) => Generate();

    private void OnCopyClick(object sender, RoutedEventArgs e) => Copy();

    private void OnRevealClick(object sender, RoutedEventArgs e) => ToggleReveal();

    private void ToggleReveal()
    {
        _isValueRevealed = !_isValueRevealed;
        UpdateRevealButton();
        RenderValue();
        SaveSettings();
    }

    private void UpdateRevealButton()
    {
        RevealButton.Content = _isValueRevealed ? HideGlyph : ShowGlyph;
        RevealButton.SetResourceReference(ToolTipProperty, _isValueRevealed ? "Generator.Hide" : "Generator.Show");
    }

    private void SaveSettings()
    {
        var settings = new AppSettings
        {
            AutoClearClipboard = AutoClearCheckBox.IsChecked == true,
            IsValueMasked = !_isValueRevealed,
            CloseToTray = CloseToTrayCheckBox.IsChecked == true,
            HasShownTrayHint = _hasShownTrayHint,
            Password = _passwordOptions,
        };
        try
        {
            _settingsStore.Save(settings);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The choice still applies for this session; only persistence failed
            ShowStatus(StatusLevel.Warning, Text("Settings.SaveFailed"));
        }
    }

    private void RenderValue()
    {
        OutputTextBox.Text = _isValueRevealed ? _currentValue : new string(MaskCharacter, _currentValue.Length);
        OutputTextBox.FontSize = OutputFontSize(_currentValue.Length);
    }

    private void OnVersionClick(object sender, RoutedEventArgs e) =>
        new ChangelogWindow { Owner = this }.ShowDialog();

    private void Copy()
    {
        if (_currentValue.Length == 0)
        {
            return;
        }

        // Feedback first: Copy raises the first countdown tick synchronously
        _isCopyFeedbackVisible = true;
        CopyIcon.Text = CheckGlyph;
        CopyLabel.SetResourceReference(TextBlock.TextProperty, "Generator.CopyAgain");
        _clipboard.Copy(_currentValue, AutoClearCheckBox.IsChecked == true);
        if (!_clipboard.IsCountingDown)
        {
            ShowStatus(StatusLevel.Success, Text("Generator.CopiedNoAutoClear"));
        }
    }

    // Turning auto-clear off mid-countdown keeps the value; turning it on later does not start a countdown retroactively
    private void OnAutoClearChanged(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        if (AutoClearCheckBox.IsChecked == true || !_clipboard.IsCountingDown)
        {
            return;
        }

        _clipboard.CancelCountdown();
        CountdownBar.Visibility = Visibility.Hidden;
        if (_isCopyFeedbackVisible)
        {
            ShowStatus(StatusLevel.Success, Text("Generator.CopiedNoAutoClear"));
        }
    }

    private void OnCountdownChanged(int secondsLeft)
    {
        if (!_isCopyFeedbackVisible)
        {
            return;
        }

        if (secondsLeft > 0)
        {
            CountdownBar.Visibility = Visibility.Visible;
            CountdownBar.Value = secondsLeft;
            ShowStatus(StatusLevel.Success, string.Format(Text("Generator.Countdown"), secondsLeft));
            return;
        }

        HideCopyFeedback();
        ShowStatus(StatusLevel.Info, Text("Generator.ClipboardCleared"));
    }

    // The clipboard timer keeps running; only the on-screen "Copied" state is reset
    private void HideCopyFeedback()
    {
        _isCopyFeedbackVisible = false;
        CountdownBar.Visibility = Visibility.Hidden;
        CopyIcon.Text = CopyGlyph;
        CopyLabel.SetResourceReference(TextBlock.TextProperty, "Generator.Copy");
    }

    private void ApplyKind()
    {
        var kind = SelectedKind;
        PasswordPanel.Visibility = kind == SecretKind.Password ? Visibility.Visible : Visibility.Collapsed;

        var (min, max, value) = kind switch
        {
            SecretKind.Password => (SecretGenerator.MinPasswordLength, SecretGenerator.MaxPasswordLength,
                _passwordOptions.Length),
            SecretKind.Pin => (SecretGenerator.MinPinLength, SecretGenerator.MaxPinLength, 6),
            _ => (SecretGenerator.MinByteCount, SecretGenerator.MaxByteCount, 32),
        };
        LengthSlider.Minimum = min;
        LengthSlider.Maximum = max;
        LengthSlider.Value = value;
        UpdateLengthLabel();

        Generate();
    }

    private void UpdateLengthLabel()
    {
        var key = SelectedKind switch
        {
            SecretKind.Password => "Generator.Length.Characters",
            SecretKind.Pin => "Generator.Length.Digits",
            _ => "Generator.Length.Bytes",
        };
        LengthLabel.Text = Text(key);
        LengthValue.Text = ((int)LengthSlider.Value).ToString();
    }

    private GenerationResult Generate()
    {
        var length = (int)LengthSlider.Value;
        var result = SelectedKind switch
        {
            SecretKind.Password => SecretGenerator.GeneratePassword(_passwordOptions),
            SecretKind.Hex => SecretGenerator.GenerateHex(length),
            SecretKind.Base64Url => SecretGenerator.GenerateBase64Url(length),
            _ => SecretGenerator.GeneratePin(length),
        };

        HideCopyFeedback();
        _currentValue = result.Value ?? string.Empty;
        RenderValue();
        ShowStrength(result.IsSuccess ? EntropyBits(length) : 0);
        var status = result.IsSuccess ? string.Empty : Text($"Generator.Error.{result.Error}");
        ShowStatus(result.IsSuccess ? StatusLevel.Info : StatusLevel.Error, status);
        return result;
    }

    // Shrink long values so most fit without wrapping; the box height stays fixed either way
    private static double OutputFontSize(int length) => length switch
    {
        <= ShortValueLength => LargeFontSize,
        <= MediumValueLength => MediumFontSize,
        _ => SmallFontSize,
    };

    private double EntropyBits(int length) => SelectedKind switch
    {
        SecretKind.Password => SecretGenerator.PasswordEntropyBits(_passwordOptions),
        SecretKind.Pin => length * Math.Log2(10),
        _ => length * 8,
    };

    private void ShowStrength(double bits)
    {
        var (key, level) = bits switch
        {
            < FairEntropyBits => ("Generator.Strength.Weak", StatusLevel.Error),
            < StrongEntropyBits => ("Generator.Strength.Fair", StatusLevel.Warning),
            < VeryStrongEntropyBits => ("Generator.Strength.Strong", StatusLevel.Success),
            _ => ("Generator.Strength.VeryStrong", StatusLevel.Success),
        };
        StrengthBar.Value = Math.Min(bits, StrengthBar.Maximum);
        StrengthBar.SetResourceReference(ForegroundProperty, StatusStyle.BrushKey(level));
        StrengthText.SetResourceReference(TextBlock.ForegroundProperty, StatusStyle.BrushKey(level));
        StrengthText.Text = string.Format(Text(key), (int)bits);
    }

    private void ShowStatus(StatusLevel level, string message)
    {
        StatusIcon.Text = message.Length == 0 ? string.Empty : StatusStyle.Glyph(level);
        StatusText.Text = message;
        StatusIcon.SetResourceReference(TextBlock.ForegroundProperty, StatusStyle.BrushKey(level));
        StatusText.SetResourceReference(TextBlock.ForegroundProperty, StatusStyle.BrushKey(level));
    }

    private string Text(string key) => (string)FindResource(key);
}
