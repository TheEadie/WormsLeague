using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Reflection;
using Worms.Components.Updaters.GitHubReleaseUpdater;
using Worms.Components.Updaters.OutsideOfToolUpdater;
using worms.Configuration;
using Worms.GameRunner;

namespace Worms.Components
{
    public class ComponentsRepository
    {
        private readonly IFileSystem _fileSystem;
        private readonly IWormsLocator _wormsLocator;
        private readonly ConfigManager _configManager;

        public ComponentsRepository(IFileSystem fileSystem, IWormsLocator wormsLocator, ConfigManager configManager)
        {
            _fileSystem = fileSystem;
            _wormsLocator = wormsLocator;
            _configManager = configManager;
        }

        public IEnumerable<Component> GetAll()
        {
            var config = _configManager.Load();

            return new List<Component>
            {
                CreateCli(config),
                CreateGame()
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

        private Component CreateGame()
        {
            var gameInfo = _wormsLocator.Find();
            
            // https://worms2d.info/List_of_Worms_Armageddon_logic_versions
            var knownVersions = new List<Version> { new Version(3, 7, 2, 1) };
            
            return new Component(
                "Worms Armageddon",
                gameInfo.Version,
                gameInfo.ExeLocation,
                new OutsideOfToolUpdateConfig(knownVersions));
        }
    }
}