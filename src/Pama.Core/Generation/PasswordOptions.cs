namespace Pama.Core.Generation;

/// <summary>Character classes and length of a generated password.</summary>
public sealed record PasswordOptions(
    int Length = 20,
    bool IncludeLowercase = true,
    bool IncludeUppercase = true,
    bool IncludeDigits = true,
    bool IncludeSymbols = true,
    bool ExcludeAmbiguous = false);
