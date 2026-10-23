using System.Runtime.InteropServices;

namespace Pama.Desktop;

internal static partial class NativeMethods
{
    // Window shows as blank in screenshots, screen recording and screen sharing (Windows 10 2004+)
    public const uint WdaExcludeFromCapture = 0x11;

    // Pre-2004 fallback: window shows as black in captures
    public const uint WdaMonitor = 0x01;

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetWindowDisplayAffinity(IntPtr hWnd, uint affinity);

    [LibraryImport("kernel32.dll", EntryPoint = "SetDllDirectoryW", StringMarshalling = StringMarshalling.Utf16,
        SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetDllDirectory(string path);
}
