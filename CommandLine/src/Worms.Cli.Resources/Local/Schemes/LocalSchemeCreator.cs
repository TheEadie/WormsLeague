using System.IO.Abstractions;
using System.Threading.Tasks;
using Worms.Armageddon.Resources.Schemes.Binary;
using Worms.Armageddon.Resources.Schemes.Text;

namespace Worms.Cli.Resources.Local.Schemes
{
    public class LocalSchemeCreator : IResourceCreator<LocalSchemeCreateParameters>
    {
        private readonly ISchemeTextReader _schemeTextReader;
        private readonly IWscWriter _wscWriter;
        private readonly IFileSystem _fileSystem;

        public LocalSchemeCreator(ISchemeTextReader schemeTextReader, IWscWriter wscWriter, IFileSystem fileSystem)
        {
            _schemeTextReader = schemeTextReader;
            _wscWriter = wscWriter;
            _fileSystem = fileSystem;
        }

        public Task Create(LocalSchemeCreateParameters parameters)
        {
            var scheme = _schemeTextReader.GetModel(parameters.Definition);
            var path = _fileSystem.Path.Combine(parameters.Folder, parameters.Name + ".wsc");
            _wscWriter.Write(scheme, path);

            return Task.CompletedTask;
        }
    }
}
