namespace Worms.Cli.Resources;

public interface IResourcePrinter<in T>
{
    void Print(TextWriter writer, T resource, int outputMaxWidth);

    void Print(TextWriter writer, IReadOnlyCollection<T> resources, int outputMaxWidth);
}
