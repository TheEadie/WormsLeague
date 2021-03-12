using System.IO;
using Syroot.Worms.Armageddon;

namespace Worms.Resources.Schemes.Text
{
    public interface ISchemeTextWriter
    {
        void Write(Scheme definition, TextWriter textWriter);
    }
}
