using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Worms.Cli;

public class Validated<T>(bool isValid, T? value, IEnumerable<string> error)
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsValid { get; } = isValid;

    public T? Value { get; } = value;

    public IEnumerable<string> Error { get; } = error;
}

public class Valid<T>(T value) : Validated<T>(true, value, []);

public class Invalid<T> : Validated<T>
{
    public Invalid(string error)
        : base(false, default, [error]) { }

    public Invalid(IEnumerable<string> errors)
        : base(false, default, errors) { }
}

internal static class ValidatedExtensions
{
    public static Validated<T> Validate<T>(this T value, Func<T, bool> predicate, string error) =>
        predicate(value) ? new Valid<T>(value) : new Invalid<T>(error);

    public static Validated<T> Validate<T>(
        this T value,
        IEnumerable<(Func<T, bool> predicate, string error)> validations)
    {
        var errors = new List<string>();
        foreach (var (predicate, error) in validations)
        {
            if (predicate(value))
            {
                errors.Add(error);
            }
        }

        return errors.Count > 0 ? new Invalid<T>(errors) : new Valid<T>(value);
    }

    [SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    public static void LogErrors<T>(this Validated<T> validated, ILogger logger)
    {
        if (validated.IsValid)
        {
            return;
        }

        foreach (var error in validated.Error)
        {
            logger.LogError(error);
        }
    }
}
