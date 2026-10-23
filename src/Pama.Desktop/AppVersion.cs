using System.Reflection;

namespace Pama.Desktop;

/// <summary>Version stamped at build time from CHANGELOG.md (see Directory.Build.props).</summary>
public static class AppVersion
{
    public static string Current { get; } =
        typeof(AppVersion).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            .Split('+')[0] ?? "0.0.0";
}
