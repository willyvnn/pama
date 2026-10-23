namespace Pama.Core.Generation;

/// <summary>Either a generated secret or the reason generation was rejected.</summary>
public sealed record GenerationResult
{
    public string? Value { get; }

    public GenerationError? Error { get; }

    public bool IsSuccess => Error is null;

    private GenerationResult(string? value, GenerationError? error)
    {
        Value = value;
        Error = error;
    }

    public static GenerationResult Success(string value) => new(value, null);

    public static GenerationResult Failure(GenerationError error) => new(null, error);
}
