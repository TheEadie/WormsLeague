using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Resources.Schemes.Text
{
    public interface ISchemeTextReader
    {
        Scheme GetModel(string definition);
    }
}
