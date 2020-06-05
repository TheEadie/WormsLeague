using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Worms.Logging;
using Worms.Resources.Schemes;
using Worms.WormsArmageddon;
using Worms.WormsArmageddon.Schemes.WscFiles;

// ReSharper disable MemberCanBePrivate.Global - CLI library uses magic to read members
// ReSharper disable UnassignedGetOnlyAutoProperty - CLI library uses magic to set members
// ReSharper disable UnusedMember.Global - CLI library uses magic to call OnExecuteAsync()

namespace Worms.Commands
{
    [Command("scheme", "schemes", Description = "Create Worms Schemes (.wsc files)")]
    internal class CreateScheme : CommandBase
    {
        private readonly IFileSystem _fileSystem;
        private readonly WscWriter _wscWriter;
        private readonly IWormsLocator _wormsLocator;

        [Argument(0, Description = "", Name = "name")]
        public string Name { get; }

        [Option(
            Description = "If set the scheme definition will be loaded from this file",
            ShortName = "f")]
        public string FilePath { get; }

        public CreateScheme(IFileSystem fileSystem, WscWriter wscWriter, IWormsLocator wormsLocator)
        {
            _fileSystem = fileSystem;
            _wscWriter = wscWriter;
            _wormsLocator = wormsLocator;
        }

        public Task<int> OnExecuteAsync(IConsole console)
        {
            var definition = string.Empty;

            if (string.IsNullOrWhiteSpace(Name))
            {
                Logger.Error("No name provided for the scheme being created.");
            }

            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                definition = _fileSystem.File.ReadAllText(FilePath);
            }
            else if (console.IsInputRedirected)
            {
                definition = console.In.ReadToEnd();
            }
            else
            {
                Logger.Error("No scheme definition provided");
                Task.FromResult(1);
            }

            var gameInfo = _wormsLocator.Find();

            var schemeReader = new SchemeTextReader();
            var scheme = schemeReader.GetModel(definition);

            _wscWriter.WriteModel(scheme, _fileSystem.Path.Combine(gameInfo.SchemesFolder , Name + ".wsc"));

            return Task.FromResult(0);
        }
    }
}
