namespace Worms.Cli.Commands.Validation;

internal static class MapExtensions
{
    public static Validated<TOut> Map<T, TOut>(this Validated<T> value, Func<T, TOut> func) =>
        value.IsValid ? new Valid<TOut>(func(value.Value!)) : new Invalid<TOut>(value.Error);

    public static async Task<Validated<TOut>> Map<T, TOut>(this Validated<T> value, Func<T, Task<TOut>> asyncFunc) =>
        value.IsValid
            ? new Valid<TOut>(await asyncFunc(value.Value!))
            : new Invalid<TOut>(value.Error);

    public static async Task<Validated<TOut>> Map<T, TOut>(this Task<Validated<T>> value, Func<T, Task<TOut>> asyncFunc)
    {
        var result = await value;
        return result.IsValid
            ? new Valid<TOut>(await asyncFunc(result.Value))
            : new Invalid<TOut>(result.Error);
    }

    public static async Task<Validated<TOut>> Map<T, TOut>(this Task<Validated<T>> value, Func<T, TOut> func)
    {
        var result = await value;
        return result.IsValid ? new Valid<TOut>(func(result.Value)) : new Invalid<TOut>(result.Error);
    }
}
