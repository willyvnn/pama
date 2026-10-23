using System.Runtime.InteropServices;

namespace Pama.Desktop.Tray;

internal static partial class TrayNativeMethods
{
    public const uint NimAdd = 0x0;
    public const uint NimModify = 0x1;
    public const uint NimDelete = 0x2;
    public const uint NifMessage = 0x1;
    public const uint NifIcon = 0x2;
    public const uint NifTip = 0x4;
    public const uint NifInfo = 0x10;
    public const uint NiifNoSound = 0x10;
    public const int WmApp = 0x8000;
    public const int WmLButtonUp = 0x0202;
    public const int WmRButtonUp = 0x0205;
    public const uint MfString = 0x0;
    public const uint MfSeparator = 0x800;
    public const uint MfChecked = 0x8;
    public const uint MfPopup = 0x10;
    public const uint TpmReturnCmd = 0x100;
    public const uint TpmRightButton = 0x2;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NotifyIconData
    {
        public int CbSize;
        public IntPtr HWnd;
        public uint UId;
        public uint UFlags;
        public uint UCallbackMessage;
        public IntPtr HIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string SzTip;
        public uint DwState;
        public uint DwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string SzInfo;
        public uint UVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string SzInfoTitle;
        public uint DwInfoFlags;
        public Guid GuidItem;
        public IntPtr HBalloonIcon;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public int X;
        public int Y;
    }

    // DllImport, not LibraryImport: the generator cannot marshal ByValTStr fields of NotifyIconData
    [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool Shell_NotifyIcon(uint message, ref NotifyIconData data);

    [LibraryImport("shell32.dll", EntryPoint = "ExtractIconExW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial uint ExtractIconEx(string file, int index, out IntPtr largeIcon, out IntPtr smallIcon,
        uint icons);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyIcon(IntPtr icon);

    [LibraryImport("user32.dll", EntryPoint = "RegisterWindowMessageW", StringMarshalling = StringMarshalling.Utf16)]
    public static partial uint RegisterWindowMessage(string message);

    [LibraryImport("user32.dll")]
    public static partial IntPtr CreatePopupMenu();

    [LibraryImport("user32.dll", EntryPoint = "AppendMenuW", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool AppendMenu(IntPtr menu, uint flags, nuint id, string? text);

    [LibraryImport("user32.dll")]
    public static partial int TrackPopupMenu(IntPtr menu, uint flags, int x, int y, int reserved, IntPtr hWnd,
        IntPtr rect);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DestroyMenu(IntPtr menu);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool GetCursorPos(out Point point);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetForegroundWindow(IntPtr hWnd);
}
