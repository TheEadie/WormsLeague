namespace Worms.Cli.Tests.Fakes;

internal sealed class ConsoleOutputScope : IDisposable
{
    private readonly TextWriter _previous;

    public StringWriter Output { get; } = new();

    public ConsoleOutputScope()
    {
        _previous = Console.Out;
        Console.SetOut(Output);
    }

    public void Dispose() => Console.SetOut(_previous);
}
