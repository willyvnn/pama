using System.IO;
using System.Windows;

namespace Pama.Desktop;

/// <summary>Puts text on the clipboard while keeping it out of Win+V history and cloud clipboard sync.</summary>
public static class SensitiveClipboard
{
    // Documented opt-out formats: https://learn.microsoft.com/windows/win32/dataxchg/clipboard-formats#cloud-clipboard-and-clipboard-history-formats
    private const string ExcludeFromMonitoringFormat = "ExcludeClipboardContentFromMonitorProcessing";
    private const string CanIncludeInHistoryFormat = "CanIncludeInClipboardHistory";
    private const string CanUploadToCloudFormat = "CanUploadToCloudClipboard";

    public static void SetText(string text)
    {
        var data = new DataObject();
        data.SetText(text);
        data.SetData(ExcludeFromMonitoringFormat, DwordStream(0));
        data.SetData(CanIncludeInHistoryFormat, DwordStream(0));
        data.SetData(CanUploadToCloudFormat, DwordStream(0));
        Clipboard.SetDataObject(data, copy: true);
    }

    private static MemoryStream DwordStream(int value) => new(BitConverter.GetBytes(value));
}
