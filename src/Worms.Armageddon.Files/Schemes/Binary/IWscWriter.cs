using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Schemes.Binary;

public interface IWscWriter
{
    void Write(Scheme definition, string path);
}
