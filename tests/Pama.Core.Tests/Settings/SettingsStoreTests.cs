using Pama.Core.Generation;
using Pama.Core.Settings;

namespace Pama.Core.Tests.Settings;

public sealed class SettingsStoreTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "pama-tests", Guid.NewGuid().ToString());

    private string FilePath => Path.Combine(_directory, "settings.json");

    public void Dispose()
    {
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    [Fact]
    public void should_return_defaults_when_file_does_not_exist()
    {
        var store = new SettingsStore(FilePath);

        var settings = store.Load();

        Assert.Equal(
            new AppSettings
            {
                AutoClearClipboard = true,
                IsValueMasked = false,
                CloseToTray = true,
                HasShownTrayHint = false,
                Password = new PasswordOptions(),
            },
            settings);
    }

    [Fact]
    public void should_load_saved_values_when_settings_were_saved()
    {
        var store = new SettingsStore(FilePath);
        var saved = new AppSettings
        {
            AutoClearClipboard = false,
            IsValueMasked = true,
            CloseToTray = false,
            HasShownTrayHint = true,
            Password = new PasswordOptions(Length: 32, IncludeSymbols: false, ExcludeAmbiguous: true),
        };

        store.Save(saved);
        var loaded = store.Load();

        Assert.Equal(saved, loaded);
    }

    [Fact]
    public void should_return_defaults_when_file_is_corrupt()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(FilePath, "{ not json");
        var store = new SettingsStore(FilePath);

        var settings = store.Load();

        Assert.Equal(new AppSettings(), settings);
    }

    [Fact]
    public void should_use_default_password_options_when_file_predates_them()
    {
        Directory.CreateDirectory(_directory);
        File.WriteAllText(FilePath, """{ "AutoClearClipboard": false, "IsValueMasked": false }""");
        var store = new SettingsStore(FilePath);

        var settings = store.Load();

        Assert.Equal(new PasswordOptions(), settings.Password);
    }
}
