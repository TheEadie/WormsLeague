using Syroot.Worms.Armageddon;

namespace Worms.Resources.Schemes.Text
{
    public interface ISchemeTextReader
    {
        Scheme GetModel(string definition);
    }
}
