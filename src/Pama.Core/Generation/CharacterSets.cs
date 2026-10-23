namespace Pama.Core.Generation;

internal static class CharacterSets
{
    public const string Lowercase = "abcdefghijklmnopqrstuvwxyz";
    public const string Uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public const string Digits = "0123456789";
    public const string Symbols = "!@#$%^&*()-_=+[]{};:,.<>?/~";

    // Glyphs easily confused when a password is read aloud or retyped from paper
    public const string Ambiguous = "Il1O0o|`'\"";
}
