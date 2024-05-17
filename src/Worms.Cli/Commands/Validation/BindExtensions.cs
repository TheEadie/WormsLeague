namespace Worms.Cli.Commands.Validation;

internal static class BindExtensions
{
    public static Validated<TOut> Bind<T, TOut>(this T value, Func<T, TOut> func) => new Valid<TOut>(func(value!));

    public static async Task<Validated<TOut>> Bind<T, TOut>(this T value, Func<T, Task<TOut>> asyncFunc) =>
        new Valid<TOut>(await asyncFunc(value!).ConfigureAwait(false));

    public static async Task<Validated<TOut>> Bind<T, TOut>(this Task<T> value, Func<T, Task<TOut>> asyncFunc)
    {
        var result = await value.ConfigureAwait(false);
        return new Valid<TOut>(await asyncFunc(result).ConfigureAwait(false));
    }

    public static async Task<Validated<TOut>> Bind<T, TOut>(this Task<T> value, Func<T, TOut> func)
    {
        var result = await value.ConfigureAwait(false);
        return new Valid<TOut>(func(result));
    }
}
