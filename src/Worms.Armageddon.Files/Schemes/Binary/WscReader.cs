using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Schemes.Binary;

internal sealed class WscReader : IWscReader
{
    public Scheme Read(string path)
    {
        var scheme = new Scheme(path);

        // This value incorrectly defaults to false in the 3rd party library
        // when the scheme is Version 1
        if (scheme.Version == SchemeVersion.Version1)
        {
            scheme.Extended.FiringPausesTimer = true;
        }

        return scheme;
    }
}
