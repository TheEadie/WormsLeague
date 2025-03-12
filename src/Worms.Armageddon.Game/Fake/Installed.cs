﻿using System.Globalization;
using System.IO.Abstractions.TestingHelpers;

namespace Worms.Armageddon.Game.Fake;

internal sealed class Installed(MockFileSystem fileSystem, string? path = null, Version? version = null)
    : IWormsArmageddon
{
    private readonly string _path = path ?? @"C:\Program Files (x86)\Steam\steamapps\common\Worms Armageddon";
    private readonly Version _version = version ?? new Version(1, 0, 0, 0);

    public GameInfo FindInstallation()
    {
        return new GameInfo(
            true,
            Path.Combine(_path, "WA.exe"),
            "WA",
            _version,
            Path.Combine(_path, "User", "Schemes"),
            Path.Combine(_path, "User", "Games"),
            Path.Combine(_path, "User", "Capture"));
    }

    public Task Host()
    {
        var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss", CultureInfo.InvariantCulture);
        fileSystem.AddEmptyFile(Path.Combine(_path, "User", "Games", $"{dateTime} [Offline] 1-UP, 2-UP.WAGame"));
        return Task.CompletedTask;
    }

    public Task GenerateReplayLog(string replayPath) => throw new NotImplementedException();

    public Task PlayReplay(string replayPath) => throw new NotImplementedException();

    public Task PlayReplay(string replayPath, TimeSpan startTime) => throw new NotImplementedException();

    public Task ExtractReplayFrames(
        string replayPath,
        uint fps,
        TimeSpan startTime,
        TimeSpan endTime,
        int xResolution = 640,
        int yResolution = 480) =>
        throw new NotImplementedException();
}
