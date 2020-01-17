using System;
using CredentialManagement;

namespace worms.Configuration.SecureStorage
{
    internal class WindowsCredentialStorage : ICredentialStorage
    {
        public string Load(string key)
        {
            var credentialManager = new Credential { Target = key };
            return credentialManager.Load() ? credentialManager.Password : null;
        }

        public void Store(string key, string value)
        {
            var credentialManager = new Credential { Target = key, Password = value, PersistanceType = PersistanceType.LocalComputer };
            credentialManager.Save();
        }
    }
}