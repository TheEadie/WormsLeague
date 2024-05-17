using System.Diagnostics.CodeAnalysis;

namespace Worms.Cli.Commands.Validation;

internal abstract class Validated<T>(bool isValid, T? value, IEnumerable<string> error)
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsValid { get; } = isValid;

    public T? Value { get; } = value;

    public IEnumerable<string> Error { get; } = error;
}

internal sealed class Valid<T>(T value) : Validated<T>(true, value, []);

internal sealed class Invalid<T> : Validated<T>
{
    public Invalid(string error)
        : base(false, default, [error]) { }

    public Invalid(IEnumerable<string> errors)
        : base(false, default, errors) { }
}
