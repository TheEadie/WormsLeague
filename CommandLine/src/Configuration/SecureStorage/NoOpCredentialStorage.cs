namespace Worms.Configuration.SecureStorage
{
    internal class NoOpCredentialStorage : ICredentialStorage
    {
        public string Load(string key) => null;

        public void Store(string key, string value) {}
    }
}
