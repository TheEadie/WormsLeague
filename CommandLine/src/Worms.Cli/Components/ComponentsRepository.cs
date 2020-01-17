using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Reflection;
using Worms.Components.Updaters.GitHubReleaseUpdater;
using worms.Configuration;

namespace Worms.Components
{
    public class ComponentsRepository
    {
        private readonly IFileSystem _fileSystem;
        private readonly ConfigManager _configManager;

        public ComponentsRepository(IFileSystem fileSystem, ConfigManager configManager)
        {
            _fileSystem = fileSystem;
            _configManager = configManager;
        }

        public IEnumerable<Component> GetAll()
        {
            var config = _configManager.Load();

            return new List<Component>
            {
                CreateCli(config)
            };
        }

        private Component CreateCli(Config config)
        {
            var assembly = Assembly.GetEntryAssembly();
            return new Component(
                "CLI",
                assembly.GetName().Version,
                _fileSystem.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule?.FileName),
                new GitHubReleaseUpdateConfig("TheEadie", "WormsLeague", "cli/v", config.GitHubPersonalAccessToken));
        }
    }
}