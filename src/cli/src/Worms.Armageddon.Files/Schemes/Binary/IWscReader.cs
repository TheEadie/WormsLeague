using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Resources.Schemes.Binary
{
    public interface IWscReader
    {
        Scheme Read(string path);
    }
}
