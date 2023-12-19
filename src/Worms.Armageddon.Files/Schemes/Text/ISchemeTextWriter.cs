using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Schemes.Text;

public interface ISchemeTextWriter
{
    void Write(Scheme definition, TextWriter textWriter);
}
