using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Schemes.Text;

public interface ISchemeTextReader
{
    Scheme GetModel(string definition);
}
