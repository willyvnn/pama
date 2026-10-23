using System.Windows;
using System.Windows.Interop;

namespace Pama.Desktop;

/// <summary>Runtime protections that keep generated secrets inside this process and off the screen.</summary>
public static class ProcessHardening
{
    // Empty string drops the current directory from the DLL search path, so a planted DLL next to a
    // downloaded exe or in the working directory is never loaded
    public static void ApplyToProcess() => NativeMethods.SetDllDirectory(string.Empty);

    public static void ExcludeFromScreenCapture(Window window)
    {
        var handle = new WindowInteropHelper(window).Handle;
        if (!NativeMethods.SetWindowDisplayAffinity(handle, NativeMethods.WdaExcludeFromCapture))
        {
            NativeMethods.SetWindowDisplayAffinity(handle, NativeMethods.WdaMonitor);
        }
    }
}
