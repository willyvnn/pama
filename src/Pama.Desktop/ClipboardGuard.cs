using System.Windows;
using System.Windows.Threading;

namespace Pama.Desktop;

/// <summary>Owns what Pama put on the clipboard and clears it after a countdown.</summary>
public sealed class ClipboardGuard
{
    /// <summary>Seconds a copied value stays on the clipboard when auto-clear is on.</summary>
    public const int LifetimeSeconds = 30;

    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromSeconds(1) };
    private string? _copiedValue;

    public ClipboardGuard() => _timer.Tick += OnTick;

    /// <summary>Raised every second with the seconds left; 0 means the clipboard was just cleared.</summary>
    public event EventHandler<int>? CountdownChanged;

    public bool IsCountingDown => _timer.IsEnabled;

    public int SecondsLeft { get; private set; }

    public void Copy(string value, bool autoClear)
    {
        _copiedValue = value;
        SensitiveClipboard.SetText(value);
        _timer.Stop();
        if (!autoClear)
        {
            return;
        }

        SecondsLeft = LifetimeSeconds;
        _timer.Start();
        CountdownChanged?.Invoke(this, SecondsLeft);
    }

    /// <summary>Stops the countdown and leaves the value on the clipboard.</summary>
    public void CancelCountdown() => _timer.Stop();

    /// <summary>Clears now, but only a value still counting down; a copy made with auto-clear off is the user's to keep.</summary>
    public void ClearIfCountingDown()
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
            ClearIfOwned();
        }
    }

    private void OnTick(object? sender, EventArgs e)
    {
        SecondsLeft--;
        if (SecondsLeft <= 0)
        {
            _timer.Stop();
            ClearIfOwned();
        }

        CountdownChanged?.Invoke(this, SecondsLeft);
    }

    // Leave the clipboard alone if the user has since copied something else
    private void ClearIfOwned()
    {
        if (_copiedValue is not null && Clipboard.ContainsText() && Clipboard.GetText() == _copiedValue)
        {
            Clipboard.Clear();
        }

        _copiedValue = null;
    }
}
