namespace Worms.Cli.Configuration.SecureStorage;

internal sealed class NoOpCredentialStorage : ICredentialStorage
{
    public string? Load(string key) => null;

    public void Store(string key, string value) { }
}
