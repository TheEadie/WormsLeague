using CredentialManagement;

namespace Worms.Cli.Configuration.SecureStorage;

internal sealed class WindowsCredentialStorage : ICredentialStorage
{
    public string? Load(string key)
    {
        using var credentialManager = new Credential();
        credentialManager.Target = key;
        return credentialManager.Load() ? credentialManager.Password : null;
    }

    public void Store(string key, string value)
    {
        using var credentialManager = new Credential();
        credentialManager.Target = key;
        credentialManager.Password = value;
        credentialManager.PersistanceType = PersistanceType.LocalComputer;
        _ = credentialManager.Save();
    }
}
