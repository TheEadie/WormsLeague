using System.Diagnostics.CodeAnalysis;

namespace Worms.Cli;

public abstract class Maybe<T>;

public class Some<T>(T value) : Maybe<T>
{
    public T Value { get; } = value;
}

[SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "Standard Functional Programming term")]
public class Nothing<T> : Maybe<T>;

[SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "Standard Functional Programming term")]
public class Error<T>(Exception exception) : Maybe<T>
{
    public Exception Exception { get; } = exception;
}

public class UnhandledNothing<T> : Nothing<T>;

public class UnhandledError<T>(Exception exception) : Error<T>(exception);

[SuppressMessage("Usage", "CA2201:Do not raise reserved exception types")]
[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
[SuppressMessage("Design", "CA1062:Validate arguments of public methods")]
public static class MaybeExtensions
{
    public static void Match<T>(this Maybe<T> input, Action<T> some, Action nothing, Action<Exception> error)
    {
        switch (input)
        {
            case Some<T> s:
                some(s.Value);
                break;
            case Nothing<T> _:
                nothing();
                break;
            case Error<T> e:
                error(e.Exception);
                break;
            default:
                throw new Exception("New Maybe state that isn't coded for!: " + input.GetType());
        }
    }

    public static Maybe<TOut> Bind<TIn, TOut>(this Maybe<TIn> input, Func<TIn, TOut> f)
    {
        try
        {
            return input switch
            {
                Some<TIn> s when !EqualityComparer<TIn>.Default.Equals(s.Value, default) => new Some<TOut>(f(s.Value)),
                Some<TIn> s when s.GetType().GetGenericArguments()[0].IsPrimitive => new Some<TOut>(f(s.Value)),
                Some<TIn> _ => new UnhandledNothing<TOut>(),
                UnhandledNothing<TIn> _ => new UnhandledNothing<TOut>(),
                Nothing<TIn> _ => new Nothing<TOut>(),
                UnhandledError<TIn> e => new UnhandledError<TOut>(e.Exception),
                Error<TIn> e => new Error<TOut>(e.Exception),
                _ => new Error<TOut>(new Exception("New Maybe state that isn't coded for!: " + input.GetType()))
            };
        }
        catch (Exception e)
        {
            return new Error<TOut>(e);
        }
    }

    public static Maybe<T> OnSome<T>(this Maybe<T> input, Action<T> a)
    {
        if (input is Some<T> i)
        {
            a(i.Value);
        }

        return input;
    }

    public static Maybe<T> OnNothing<T>(this Maybe<T> input, Action a)
    {
        if (input is UnhandledNothing<T> _)
        {
            a();
        }

        return input;
    }

    public static Maybe<T> OnError<T>(this Maybe<T> input, Action<Exception> a)
    {
        if (input is UnhandledError<T> e)
        {
            a(e.Exception);
        }

        return input;
    }
}
