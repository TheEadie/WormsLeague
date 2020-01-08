using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Reflection;
using Worms.Components.Updaters.GitHubReleaseUpdater;
using Worms.Components.Updaters.OutsideOfToolUpdater;
using Worms.GameRunner;

namespace Worms.Components
{
    public class ComponentFactory
    {
        private readonly IFileSystem _fileSystem;
        private readonly IWormsLocator _wormsLocator;

        public ComponentFactory(IFileSystem fileSystem, IWormsLocator wormsLocator)
        {
            _fileSystem = fileSystem;
            _wormsLocator = wormsLocator;
        }

        public Component CreateCli()
        {
            var assembly = Assembly.GetEntryAssembly();
            return new Component(
                "CLI",
                assembly.GetName().Version,
                _fileSystem.Path.GetDirectoryName(assembly.Location),
                new GitHubReleaseUpdateConfig("TheEadie", "WormsLeague", "giftool/v"));
        }

        public Component CreateGame()
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