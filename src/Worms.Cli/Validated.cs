using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Worms.Cli;

internal abstract class Validated<T>(bool isValid, T? value, IEnumerable<string> error)
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsValid { get; } = isValid;

    public T? Value { get; } = value;

    public IEnumerable<string> Error { get; } = error;

    public Validated<TOut> Bind<TOut>(Func<T, TOut> func) =>
        IsValid ? new Valid<TOut>(func(Value!)) : new Invalid<TOut>(Error);

    public async Task<Validated<TOut>> Bind<TOut>(Func<T, Task<TOut>> asyncFunc) =>
        IsValid ? new Valid<TOut>(await asyncFunc(Value!).ConfigureAwait(false)) : new Invalid<TOut>(Error);
}

internal sealed class Valid<T>(T value) : Validated<T>(true, value, []);

internal sealed class Invalid<T> : Validated<T>
{
    public Invalid(string error)
        : base(false, default, [error]) { }

    public Invalid(IEnumerable<string> errors)
        : base(false, default, errors) { }
}

internal static class ValidatedExtensions
{
    public static async Task<Validated<TOut>> Bind<T, TOut>(
        this Task<Validated<T>> value,
        Func<T, Task<TOut>> asyncFunc)
    {
        var result = await value.ConfigureAwait(false);
        return result.IsValid
            ? new Valid<TOut>(await asyncFunc(result.Value).ConfigureAwait(false))
            : new Invalid<TOut>(result.Error);
    }

    public static async Task<Validated<TOut>> Bind<T, TOut>(this Task<Validated<T>> value, Func<T, TOut> func)
    {
        var result = await value.ConfigureAwait(false);
        return result.IsValid ? new Valid<TOut>(func(result.Value)) : new Invalid<TOut>(result.Error);
    }

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

    public static Validated<T> Validate<T>(
        this Validated<T> value,
        IEnumerable<(Func<T, bool> predicate, string error)> validations)
    {
        if (!value.IsValid)
        {
            return value;
        }

        var errors = new List<string>();
        foreach (var (predicate, error) in validations)
        {
            if (predicate(value.Value))
            {
                errors.Add(error);
            }
        }

        return errors.Count > 0 ? new Invalid<T>(errors) : new Valid<T>(value.Value);
    }

    public static async Task<Validated<T>> Validate<T>(
        this Task<Validated<T>> value,
        IEnumerable<(Func<T, bool> predicate, string error)> validations)
    {
        var result = await value.ConfigureAwait(false);
        if (!result.IsValid)
        {
            return result;
        }

        var errors = new List<string>();
        foreach (var (predicate, error) in validations)
        {
            if (predicate(result.Value))
            {
                errors.Add(error);
            }
        }

        return errors.Count > 0 ? new Invalid<T>(errors) : new Valid<T>(result.Value);
    }

    [SuppressMessage("Usage", "CA2254:Template should be a static expression")]
    public static bool LogIfInvalid<T>(this Validated<T> validated, ILogger logger)
    {
        if (validated.IsValid)
        {
            return false;
        }

        foreach (var error in validated.Error)
        {
            logger.LogError(error);
        }

        return true;
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
