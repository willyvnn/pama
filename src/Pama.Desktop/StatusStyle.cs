namespace Pama.Desktop;

/// <summary>Maps a status level to its Fluent theme brush key and icon glyph.</summary>
public static class StatusStyle
{
    public static string BrushKey(StatusLevel level) => level switch
    {
        StatusLevel.Success => "SystemFillColorSuccessBrush",
        StatusLevel.Warning => "SystemFillColorCautionBrush",
        StatusLevel.Error => "SystemFillColorCriticalBrush",
        _ => "TextFillColorSecondaryBrush",
    };

    public static string Glyph(StatusLevel level) => level switch
    {
        StatusLevel.Success => "",
        StatusLevel.Warning => "",
        StatusLevel.Error => "",
        _ => "",
    };
}
