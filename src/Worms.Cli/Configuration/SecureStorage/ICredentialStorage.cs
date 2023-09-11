namespace Worms.Cli.Configuration.SecureStorage
{
    public interface ICredentialStorage
    {
        string Load(string key);

        void Store(string key, string value);
    }
}
