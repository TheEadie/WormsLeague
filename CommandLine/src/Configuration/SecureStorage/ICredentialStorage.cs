using System.Collections.Generic;
using System.Text;

namespace worms.Configuration.SecureStorage
{
    public interface ICredentialStorage
    {
        string Load(string key);
        void Store(string key, string value);
    }
}
