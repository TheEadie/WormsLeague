using System.IO.Abstractions;

namespace Worms.Armageddon.Game.Tests.Framework;

internal interface IWormsArmageddonBuilder
{
    IWormsArmageddonBuilder WhereHostCmdDoesNotCreateReplayFile();

    IWormsArmageddonBuilder Installed(string? path = null, Version? version = null);

    IWormsArmageddonBuilder NotInstalled();

    IWormsArmageddonBuilder WithReplayFilePath(string replayFilePath);

    IFileSystem GetFileSystem();

    IWormsArmageddon Build();
}
