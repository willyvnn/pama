using Pama.Core.Generation;

namespace Pama.Core.Tests.Generation;

public sealed class SecretGeneratorTests
{
    [Fact]
    public void should_return_password_of_requested_length_when_options_are_valid()
    {
        var options = new PasswordOptions(Length: 32);

        var result = SecretGenerator.GeneratePassword(options);

        Assert.True(result.IsSuccess);
        Assert.Equal(32, result.Value!.Length);
    }

    [Fact]
    public void should_include_every_selected_class_when_all_classes_are_selected()
    {
        var options = new PasswordOptions(Length: SecretGenerator.MinPasswordLength);

        var result = SecretGenerator.GeneratePassword(options);

        Assert.Contains(result.Value!, char.IsLower);
        Assert.Contains(result.Value!, char.IsUpper);
        Assert.Contains(result.Value!, char.IsDigit);
        Assert.Contains(result.Value!, ch => !char.IsLetterOrDigit(ch));
    }

    [Fact]
    public void should_contain_only_digits_when_only_digits_are_selected()
    {
        var options = new PasswordOptions(IncludeLowercase: false, IncludeUppercase: false, IncludeSymbols: false);

        var result = SecretGenerator.GeneratePassword(options);

        Assert.All(result.Value!, ch => Assert.True(char.IsAsciiDigit(ch)));
    }

    [Fact]
    public void should_omit_ambiguous_characters_when_exclude_ambiguous_is_set()
    {
        var options = new PasswordOptions(Length: SecretGenerator.MaxPasswordLength, ExcludeAmbiguous: true);

        var result = SecretGenerator.GeneratePassword(options);

        Assert.Equal(-1, result.Value!.IndexOfAny("Il1O0o|`'\"".ToCharArray()));
    }

    [Theory]
    [InlineData(SecretGenerator.MinPasswordLength - 1)]
    [InlineData(SecretGenerator.MaxPasswordLength + 1)]
    public void should_reject_password_when_length_is_out_of_range(int length)
    {
        var options = new PasswordOptions(Length: length);

        var result = SecretGenerator.GeneratePassword(options);

        Assert.Equal(GenerationError.LengthOutOfRange, result.Error);
    }

    [Fact]
    public void should_reject_password_when_no_class_is_selected()
    {
        var options = new PasswordOptions(
            IncludeLowercase: false, IncludeUppercase: false, IncludeDigits: false, IncludeSymbols: false);

        var result = SecretGenerator.GeneratePassword(options);

        Assert.Equal(GenerationError.NoCharacterClassSelected, result.Error);
    }

    [Fact]
    public void should_return_64_lowercase_hex_chars_when_hex_uses_default_32_bytes()
    {
        var result = SecretGenerator.GenerateHex();

        Assert.Matches("^[0-9a-f]{64}$", result.Value!);
    }

    [Fact]
    public void should_return_unpadded_base64url_when_base64url_is_requested()
    {
        var result = SecretGenerator.GenerateBase64Url(32);

        Assert.Matches("^[A-Za-z0-9_-]{43}$", result.Value!);
    }

    [Theory]
    [InlineData(SecretGenerator.MinByteCount - 1)]
    [InlineData(SecretGenerator.MaxByteCount + 1)]
    public void should_reject_hex_when_byte_count_is_out_of_range(int byteCount)
    {
        var result = SecretGenerator.GenerateHex(byteCount);

        Assert.Equal(GenerationError.LengthOutOfRange, result.Error);
    }

    [Fact]
    public void should_return_digits_only_when_pin_is_requested()
    {
        var result = SecretGenerator.GeneratePin(6);

        Assert.Matches("^[0-9]{6}$", result.Value!);
    }

    [Fact]
    public void should_return_length_times_log2_alphabet_when_estimating_entropy()
    {
        var options = new PasswordOptions(Length: 10, IncludeLowercase: false, IncludeUppercase: false, IncludeSymbols: false);

        var bits = SecretGenerator.PasswordEntropyBits(options);

        Assert.Equal(10 * Math.Log2(10), bits, precision: 6);
    }
}
