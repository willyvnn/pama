using System.Text.Json;

namespace Pama.Core.Settings;

/// <summary>Reads and writes <see cref="AppSettings"/> as a JSON file.</summary>
public sealed class SettingsStore(string filePath)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>Settings file under %AppData%\Pama.</summary>
    public static SettingsStore ForCurrentUser() => new(Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Pama", "settings.json"));

    /// <summary>Saved settings, or defaults when the file is missing or unreadable.</summary>
    public AppSettings Load()
    {
        if (!File.Exists(filePath))
        {
            return new AppSettings();
        }

        try
        {
            return JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(filePath), JsonOptions) ?? new AppSettings();
        }
        catch (JsonException)
        {
            // Preferences only: a corrupt file must not stop the app; the next save overwrites it
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        // Write then rename so a crash mid-write never leaves a truncated file
        var tempPath = filePath + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(settings, JsonOptions));
        File.Move(tempPath, filePath, overwrite: true);
    }
}
