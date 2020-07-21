using Syroot.Worms.Armageddon;

namespace Worms.Resources.Schemes.Binary
{
    internal class WscReader : IWscReader
    {
        public Scheme Read(string path)
        {
            return new Scheme(path);
        }
    }
}
