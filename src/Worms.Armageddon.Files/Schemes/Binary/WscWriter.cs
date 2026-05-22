using System.IO.Abstractions;
using Syroot.Worms.Armageddon;

namespace Worms.Armageddon.Files.Schemes.Binary;

internal sealed class WscWriter(IFileSystem fileSystem) : IWscWriter
{
    public void Write(Scheme definition, string path)
    {
        using var stream = new MemoryStream();
        definition.Save(stream);
        fileSystem.File.WriteAllBytes(path, stream.ToArray());
    }
}
