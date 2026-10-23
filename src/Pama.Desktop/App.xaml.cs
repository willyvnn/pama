using System.Windows;
using Pama.Desktop.Startup;

namespace Pama.Desktop;

/// <summary>Application entry; allows one running instance per Windows user session.</summary>
public partial class App : Application
{
    // Local\ scopes both handles to the current logon session, so other users on the same PC are unaffected
    private const string InstanceMutexName = @"Local\Pama.SingleInstance";
    private const string ActivateEventName = @"Local\Pama.Activate";

    private Mutex? _instanceMutex;
    private EventWaitHandle? _activateEvent;
    private RegisteredWaitHandle? _activateWait;

    protected override void OnStartup(StartupEventArgs e)
    {
        ProcessHardening.ApplyToProcess();
        _instanceMutex = new Mutex(initiallyOwned: true, InstanceMutexName, out var isFirstInstance);
        _activateEvent = new EventWaitHandle(false, EventResetMode.AutoReset, ActivateEventName);

        if (!isFirstInstance)
        {
            _activateEvent.Set();
            Shutdown();
            return;
        }

        _activateWait = ThreadPool.RegisterWaitForSingleObject(
            _activateEvent, (_, _) => Dispatcher.BeginInvoke(BringMainWindowToFront), null, Timeout.Infinite, false);
        base.OnStartup(e);
        StartupRegistration.RefreshPathIfEnabled();
        if (e.Args.Contains(StartupRegistration.MinimizedArgument))
        {
            StartInTray();
            return;
        }

        StartWithSplash();
    }

    // Login start: no splash and no window, just the tray icon
    private void StartInTray()
    {
        ApplyTheme();
        var window = new MainWindow();
        MainWindow = window;
        window.StartInTray();
    }

    // Theme and window are loaded here rather than via App.xaml so the splash is already up while they load
    private void StartWithSplash()
    {
        var splash = SplashController.Show($"v{AppVersion.Current}");

        splash.Report(0.3, Text("Startup.Theme"));
        ApplyTheme();

        splash.Report(0.7, Text("Startup.Window"));
        var window = new MainWindow();
        MainWindow = window;

        splash.Report(1.0, Text("Startup.Ready"));
        window.ContentRendered += (_, _) => OnFirstRender(splash);
        window.Show();
    }

    private void OnFirstRender(SplashController splash)
    {
        splash.Close();
        BringMainWindowToFront();
    }

    // Fluent theme that follows the Windows light/dark setting live, title bar included
    // HACK: ThemeMode is still marked experimental (WPF0001) in .NET 10; drop the pragma once it is stable
#pragma warning disable WPF0001
    private void ApplyTheme() => ThemeMode = ThemeMode.System;
#pragma warning restore WPF0001

    private string Text(string key) => (string)FindResource(key);

    protected override void OnExit(ExitEventArgs e)
    {
        _activateWait?.Unregister(null);
        _activateEvent?.Dispose();
        _instanceMutex?.Dispose();
        base.OnExit(e);
    }

    private void BringMainWindowToFront()
    {
        if (MainWindow is not { } window)
        {
            return;
        }

        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }

        window.Show();
        window.Activate();
        // Toggling Topmost is the reliable way past Windows' foreground-lock for a background process
        window.Topmost = true;
        window.Topmost = false;
        window.Focus();
    }
}
