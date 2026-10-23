using System.Windows.Threading;

namespace Pama.Desktop.Startup;

/// <summary>Shows the splash on a dedicated UI thread so it keeps animating while the main thread loads.</summary>
public sealed class SplashController
{
    private readonly Dispatcher _dispatcher;
    private readonly SplashWindow _window;

    private SplashController(Dispatcher dispatcher, SplashWindow window)
    {
        _dispatcher = dispatcher;
        _window = window;
    }

    public static SplashController Show(string version)
    {
        SplashController? controller = null;
        using var ready = new ManualResetEventSlim();
        var thread = new Thread(() =>
        {
            var window = new SplashWindow(version);
            window.Show();
            controller = new SplashController(Dispatcher.CurrentDispatcher, window);
            ready.Set();
            Dispatcher.Run();
        })
        {
            IsBackground = true,
            Name = "Splash",
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        ready.Wait();
        return controller!;
    }

    public void Report(double fraction, string text) =>
        _dispatcher.BeginInvoke(() => _window.ShowStep(fraction, text));

    /// <summary>Closes the splash and returns once it is gone, so the caller can activate its own window after.</summary>
    public void Close()
    {
        _dispatcher.Invoke(_window.Close);
        _dispatcher.InvokeShutdown();
    }
}
