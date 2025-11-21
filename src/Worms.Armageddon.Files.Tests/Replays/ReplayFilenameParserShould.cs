using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;
using Worms.Armageddon.Files.Replays.Filename;

namespace Worms.Armageddon.Files.Tests.Replays;

internal sealed class ReplayFilenameParserShould
{
    private readonly IReplayFilenameParser _replayFilenameParser;

    public ReplayFilenameParserShould()
    {
        var services = new ServiceCollection();
        _ = services.AddWormsArmageddonFilesServices();
        var serviceProvider = services.BuildServiceProvider();
        _replayFilenameParser = serviceProvider.GetRequiredService<IReplayFilenameParser>();
    }

    [Test]
    public void ParseDateFromOnlineFilename()
    {
        const string filename = "2025-10-23 13.31.45 [Online] @Eadie, Skip, peter.WAgame";
        var result = _replayFilenameParser.Parse(filename);
        result.Date.ShouldBe(new DateTime(2025, 10, 23, 13, 31, 45));
        result.GameMode.ShouldBe("Online");
    }

    [Test]
    public void ParseDateTimeFromTrainingFilename()
    {
        const string filename = "2025-10-31 14.39.59 [Training] Eadie's Army.WAgame";
        var result = _replayFilenameParser.Parse(filename);
        result.Date.ShouldBe(new DateTime(2025, 10, 31, 14, 39, 59));
        result.GameMode.ShouldBe("Training");
    }

    [Test]
    public void ParseDateTimeFromQuickCpuFilename()
    {
        const string filename = "2025-10-31 14.41.03 [Quick CPU] Player One, ROYALTY.WAgame";
        var result = _replayFilenameParser.Parse(filename);
        result.Date.ShouldBe(new DateTime(2025, 10, 31, 14, 41, 03));
        result.GameMode.ShouldBe("Quick CPU");
    }

    [Test]
    public void ParsePlayerMachineNameDetailsFromOnlineFilename()
    {
        const string filename = "2025-10-23 13.31.45 [Online] @Eadie, Skip, peter.WAgame";
        var result = _replayFilenameParser.Parse(filename);

        result.GameMode.ShouldBe("Online");
        result.PlayerMachineNames.ShouldBe(
        [
            "Eadie",
            "Skip",
            "peter"
        ]);
        result.HostMachineName.ShouldBe("Eadie");
        result.LocalMachineName.ShouldBe("Eadie");
    }

    [Test]
    public void ParsePlayerMachineNameDetailsFromOnlineFilenameWhenLocalPlayerIsNotTheHost()
    {
        const string filename = "2025-10-23 13.31.45 [Online] Eadie, @Skip, peter.WAgame";
        var result = _replayFilenameParser.Parse(filename);

        result.GameMode.ShouldBe("Online");
        result.PlayerMachineNames.ShouldBe(
        [
            "Eadie",
            "Skip",
            "peter"
        ]);
        result.HostMachineName.ShouldBe("Eadie");
        result.LocalMachineName.ShouldBe("Skip");
    }
}
