﻿using System.IO.Abstractions;
using Serilog;
using Worms.Armageddon.Files.Schemes.Binary;
using Worms.Armageddon.Files.Schemes.Text;

namespace Worms.Cli.Resources.Local.Schemes;

internal sealed class LocalSchemeCreator : IResourceCreator<LocalScheme, LocalSchemeCreateParameters>
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

    public Task<LocalScheme> Create(LocalSchemeCreateParameters parameters, ILogger logger, CancellationToken cancellationToken)
    {
        var scheme = _schemeTextReader.GetModel(parameters.Definition);
        var path = _fileSystem.Path.Combine(parameters.Folder, parameters.Name + ".wsc");
        _wscWriter.Write(scheme, path);

        return Task.FromResult(new LocalScheme(path, parameters.Name, scheme));
    }
}