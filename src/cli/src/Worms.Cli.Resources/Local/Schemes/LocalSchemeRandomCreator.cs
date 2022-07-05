using System.IO.Abstractions;
using System.Threading.Tasks;
using Worms.Armageddon.Resources.Schemes.Binary;
using Worms.Armageddon.Resources.Schemes.Random;

namespace Worms.Cli.Resources.Local.Schemes
{
    public class LocalSchemeRandomCreator : IResourceCreator<LocalScheme, LocalSchemeCreateRandomParameters>
    {
        private readonly IRandomSchemeGenerator _randomSchemeGenerator;
        private readonly IWscWriter _wscWriter;
        private readonly IFileSystem _fileSystem;

        public LocalSchemeRandomCreator(IRandomSchemeGenerator randomSchemeGenerator, IWscWriter wscWriter, IFileSystem fileSystem)
        {
            _randomSchemeGenerator = randomSchemeGenerator;
            _wscWriter = wscWriter;
            _fileSystem = fileSystem;
        }

        public Task<LocalScheme> Create(LocalSchemeCreateRandomParameters parameters)
        {
            var scheme = _randomSchemeGenerator.Generate();
            var path = _fileSystem.Path.Combine(parameters.Folder, parameters.Name + ".wsc");
            _wscWriter.Write(scheme, path);

            return Task.FromResult(new LocalScheme(path, parameters.Name, scheme));
        }
    }
}
