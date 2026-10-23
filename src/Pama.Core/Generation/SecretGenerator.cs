using System.Security.Cryptography;

namespace Pama.Core.Generation;

/// <summary>Generates passwords, API keys and tokens from the OS CSPRNG.</summary>
public static class SecretGenerator
{
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 256;
    public const int MinByteCount = 16;
    public const int MaxByteCount = 128;
    public const int MinPinLength = 4;
    public const int MaxPinLength = 32;

    /// <summary>Password containing at least one character of every selected class.</summary>
    public static GenerationResult GeneratePassword(PasswordOptions options)
    {
        if (options.Length is < MinPasswordLength or > MaxPasswordLength)
        {
            return GenerationResult.Failure(GenerationError.LengthOutOfRange);
        }

        var classes = SelectClasses(options);
        if (classes.Count == 0)
        {
            return GenerationResult.Failure(GenerationError.NoCharacterClassSelected);
        }

        var alphabet = string.Concat(classes);

        // Rejection sampling keeps the distribution uniform, unlike forcing one char per class into fixed slots
        string password;
        do
        {
            password = RandomNumberGenerator.GetString(alphabet, options.Length);
        }
        while (!classes.All(set => password.AsSpan().IndexOfAny(set) >= 0));

        return GenerationResult.Success(password);
    }

    /// <summary>Lowercase hex of <paramref name="byteCount"/> random bytes, same as <c>openssl rand -hex N</c>.</summary>
    public static GenerationResult GenerateHex(int byteCount = 32)
    {
        if (byteCount is < MinByteCount or > MaxByteCount)
        {
            return GenerationResult.Failure(GenerationError.LengthOutOfRange);
        }

        return GenerationResult.Success(RandomNumberGenerator.GetHexString(byteCount * 2, lowercase: true));
    }

    /// <summary>Unpadded base64url of <paramref name="byteCount"/> random bytes, safe in URLs and headers.</summary>
    public static GenerationResult GenerateBase64Url(int byteCount = 32)
    {
        if (byteCount is < MinByteCount or > MaxByteCount)
        {
            return GenerationResult.Failure(GenerationError.LengthOutOfRange);
        }

        var bytes = RandomNumberGenerator.GetBytes(byteCount);
        return GenerationResult.Success(Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_'));
    }

    /// <summary>Numeric PIN of <paramref name="length"/> digits.</summary>
    public static GenerationResult GeneratePin(int length = 6)
    {
        if (length is < MinPinLength or > MaxPinLength)
        {
            return GenerationResult.Failure(GenerationError.LengthOutOfRange);
        }

        return GenerationResult.Success(RandomNumberGenerator.GetString(CharacterSets.Digits, length));
    }

    /// <summary>Entropy in bits of a password generated with <paramref name="options"/>; 0 when the options are invalid.</summary>
    public static double PasswordEntropyBits(PasswordOptions options)
    {
        var alphabetSize = SelectClasses(options).Sum(set => set.Length);
        return alphabetSize == 0 ? 0 : options.Length * Math.Log2(alphabetSize);
    }

    private static List<string> SelectClasses(PasswordOptions options)
    {
        var selected = new List<(bool IsIncluded, string Set)>
        {
            (options.IncludeLowercase, CharacterSets.Lowercase),
            (options.IncludeUppercase, CharacterSets.Uppercase),
            (options.IncludeDigits, CharacterSets.Digits),
            (options.IncludeSymbols, CharacterSets.Symbols),
        };

        return selected
            .Where(c => c.IsIncluded)
            .Select(c => options.ExcludeAmbiguous ? RemoveAmbiguous(c.Set) : c.Set)
            .ToList();
    }

    private static string RemoveAmbiguous(string set) =>
        string.Concat(set.Where(ch => !CharacterSets.Ambiguous.Contains(ch)));
}
