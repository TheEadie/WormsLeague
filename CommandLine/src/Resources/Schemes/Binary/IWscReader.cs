using Syroot.Worms.Armageddon;

namespace Worms.Resources.Schemes.Binary
{
    public interface IWscReader
    {
        Scheme Read(string path);
    }
}
