using JetBrains.Annotations;

namespace Worms.Cli.Commands.Validation;

internal static class BindExtensions
{
    public static Validated<TOut> Bind<T, TOut>(this T value, Func<T, TOut> func) => new Valid<TOut>(func(value!));

    [UsedImplicitly]
    public static async Task<Validated<TOut>> Bind<T, TOut>(this T value, Func<T, Task<TOut>> asyncFunc) =>
        new Valid<TOut>(await asyncFunc(value!));

    [UsedImplicitly]
    public static async Task<Validated<TOut>> Bind<T, TOut>(this Task<T> value, Func<T, Task<TOut>> asyncFunc)
    {
        var result = await value;
        return new Valid<TOut>(await asyncFunc(result));
    }

    [UsedImplicitly]
    public static async Task<Validated<TOut>> Bind<T, TOut>(this Task<T> value, Func<T, TOut> func)
    {
        var result = await value;
        return new Valid<TOut>(func(result));
    }
}
