using Syroot.Worms.Armageddon;

namespace Worms.Resources.Schemes.Binary
{
    public class WscWriter : IWscWriter
    {
        public void Write(Scheme definition, string path)
        {
            definition.Save(path);
        }
    }
}
