using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Schemes.Binary
{
    public interface IWscReader
    {
        Scheme Read(string path);
    }
}
