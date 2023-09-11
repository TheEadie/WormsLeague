using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Resources.Schemes.Binary
{
    public interface IWscWriter
    {
        void Write(Scheme definition, string path);
    }
}
