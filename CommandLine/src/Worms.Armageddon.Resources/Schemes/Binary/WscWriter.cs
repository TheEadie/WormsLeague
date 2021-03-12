using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Resources.Schemes.Binary
{
    internal class WscWriter : IWscWriter
    {
        public void Write(Scheme definition, string path)
        {
            definition.Save(path);
        }
    }
}
