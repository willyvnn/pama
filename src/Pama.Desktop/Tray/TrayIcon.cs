using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using static Pama.Desktop.Tray.TrayNativeMethods;

namespace Pama.Desktop.Tray;

/// <summary>Notification-area icon with a click event, a right-click menu and toast-style notifications.</summary>
public sealed class TrayIcon : IDisposable
{
    private const int CallbackMessage = WmApp + 1;
    private const uint IconId = 1;
    // TrackPopupMenu returns 0 for "dismissed", so menu ids start at 1
    private const int FirstCommandId = 1;

    private readonly IntPtr _windowHandle;
    private readonly HwndSource _source;
    private readonly IntPtr _icon;
    private readonly string _tooltip;
    private readonly IReadOnlyList<TrayMenuItem?> _menuItems;
    // Explorer broadcasts this after a restart; the icon has to be added again or it silently disappears
    private readonly uint _taskbarCreatedMessage = RegisterWindowMessage("TaskbarCreated");
    private bool _isVisible;

    public event EventHandler? Clicked;

    public TrayIcon(Window window, string tooltip, IReadOnlyList<TrayMenuItem?> menuItems)
    {
        _windowHandle = new WindowInteropHelper(window).EnsureHandle();
        _source = HwndSource.FromHwnd(_windowHandle);
        _source.AddHook(WndProc);
        ExtractIconEx(Environment.ProcessPath!, 0, out _, out _icon, 1);
        _tooltip = tooltip;
        _menuItems = menuItems;
    }

    public void Show()
    {
        if (_isVisible)
        {
            return;
        }

        var data = CreateData();
        data.UFlags = NifMessage | NifIcon | NifTip;
        _isVisible = Shell_NotifyIcon(NimAdd, ref data);
    }

    /// <summary>Shows a Windows notification from the tray icon, without sound.</summary>
    public void Notify(string message)
    {
        if (!_isVisible)
        {
            return;
        }

        var data = CreateData();
        data.UFlags = NifInfo;
        data.SzInfoTitle = _tooltip;
        data.SzInfo = message;
        data.DwInfoFlags = NiifNoSound;
        Shell_NotifyIcon(NimModify, ref data);
    }

    public void Hide()
    {
        if (!_isVisible)
        {
            return;
        }

        var data = CreateData();
        Shell_NotifyIcon(NimDelete, ref data);
        _isVisible = false;
    }

    public void Dispose()
    {
        Hide();
        _source.RemoveHook(WndProc);
        if (_icon != IntPtr.Zero)
        {
            DestroyIcon(_icon);
        }
    }

    private NotifyIconData CreateData() => new()
    {
        CbSize = Marshal.SizeOf<NotifyIconData>(),
        HWnd = _windowHandle,
        UId = IconId,
        UCallbackMessage = CallbackMessage,
        HIcon = _icon,
        SzTip = _tooltip,
        SzInfo = string.Empty,
        SzInfoTitle = string.Empty,
    };

    private IntPtr WndProc(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (message == _taskbarCreatedMessage && _isVisible)
        {
            _isVisible = false;
            Show();
        }

        if (message != CallbackMessage)
        {
            return IntPtr.Zero;
        }

        switch ((int)lParam)
        {
            case WmLButtonUp:
                Clicked?.Invoke(this, EventArgs.Empty);
                break;
            case WmRButtonUp:
                ShowMenu();
                break;
        }

        handled = true;
        return IntPtr.Zero;
    }

    private void ShowMenu()
    {
        var commands = new List<TrayMenuItem>();
        var menu = BuildMenu(_menuItems, commands);
        GetCursorPos(out var cursor);

        // Without this the menu does not close when the user clicks elsewhere (documented TrackPopupMenu quirk)
        SetForegroundWindow(_windowHandle);
        var command = TrackPopupMenu(menu, TpmReturnCmd | TpmRightButton, cursor.X, cursor.Y, 0, _windowHandle,
            IntPtr.Zero);

        // Destroying the root menu also destroys its submenus
        DestroyMenu(menu);

        if (command >= FirstCommandId)
        {
            commands[command - FirstCommandId].Execute();
        }
    }

    // Flattens leaf items into commands so the id returned by TrackPopupMenu indexes straight into the list
    private static IntPtr BuildMenu(IReadOnlyList<TrayMenuItem?> items, List<TrayMenuItem> commands)
    {
        var menu = CreatePopupMenu();
        foreach (var item in items)
        {
            if (item is null)
            {
                AppendMenu(menu, MfSeparator, 0, null);
            }
            else if (item.Children is not null)
            {
                AppendMenu(menu, MfPopup, (nuint)BuildMenu(item.Children, commands), item.Text);
            }
            else
            {
                commands.Add(item);
                var flags = item.IsChecked?.Invoke() == true ? MfString | MfChecked : MfString;
                AppendMenu(menu, flags, (nuint)(commands.Count - 1 + FirstCommandId), item.Text);
            }
        }

        return menu;
    }
}
