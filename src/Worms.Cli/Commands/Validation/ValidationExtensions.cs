using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Worms.Cli.Commands.Validation;

internal static class ValidationExtensions
{
    public static Validated<T> Validate<T>(this T value, List<ValidationRule<T>> validations)
    {
        var errors = new List<string>();
        foreach (var (predicate, error) in validations)
        {
            if (!predicate(value))
            {
                errors.Add(error(value));
            }
        }

        return errors.Count > 0 ? new Invalid<T>(errors) : new Valid<T>(value);
    }

    public static Validated<T> Validate<T>(this Validated<T> value, List<ValidationRule<T>> validations) =>
        !value.IsValid ? value : value.Value.Validate(validations);

    public static async Task<Validated<T>> Validate<T>(
        this Task<Validated<T>> value,
        List<ValidationRule<T>> validations)
    {
        var result = await value.ConfigureAwait(false);
        return result.Validate(validations);
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
