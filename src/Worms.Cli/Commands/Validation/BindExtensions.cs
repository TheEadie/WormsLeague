namespace Worms.Cli.Commands.Validation;

internal static class BindExtensions
{
    public static Validated<TOut> Bind<T, TOut>(this Validated<T> value, Func<T, TOut> func) =>
        value.IsValid ? new Valid<TOut>(func(value.Value!)) : new Invalid<TOut>(value.Error);

    public static async Task<Validated<TOut>> Bind<T, TOut>(this Validated<T> value, Func<T, Task<TOut>> asyncFunc) =>
        value.IsValid
            ? new Valid<TOut>(await asyncFunc(value.Value!).ConfigureAwait(false))
            : new Invalid<TOut>(value.Error);

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
}
