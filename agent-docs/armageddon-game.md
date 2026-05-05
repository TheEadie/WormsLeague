# Armageddon Game Component

Projects: `Worms.Armageddon.Game`, `Worms.Armageddon.Game.Fake`

Abstracts over launching and interacting with the Worms Armageddon game process. Platform-specific implementations are selected at DI registration time.

## IWormsArmageddon

The primary interface consumed by the CLI and WA Runner:

```csharp
GameInfo FindInstallation();
Task Host();
Task GenerateReplayLog(string replayPath);
Task PlayReplay(string replayPath);
Task PlayReplay(string replayPath, TimeSpan startTime);
Task ExtractReplayFrames(string replayPath, uint fps, TimeSpan start, TimeSpan end, int xRes = 640, int yRes = 480);
```

`WormsArmageddon` delegates each call to `IWormsRunner` (launches the game process with the right CLI arguments) and `IWormsLocator` (finds the game installation).

## Platform split

`ServiceRegistration.AddWormsArmageddonGameServices()` detects the OS at startup:

- **Windows** → `WormsLocator` (reads registry via `IRegistry`), `SteamService`, and either `WormsRunner` or `WormsRunner2` (toggled by `WORMS_TOGGLE_NEW_WA_RUNNER=false`)
- **Linux** → `Linux.WormsLocator` and `Linux.WormsRunner` (used in the Docker WA runner, runs via Wine)

`IProcessRunner` / `IProcess` wrap `System.Diagnostics.Process` for testability. `IFileVersionInfo` wraps `System.Diagnostics.FileVersionInfo` similarly.

## GameInfo

```csharp
record GameInfo(bool IsInstalled, string ExeLocation, string CaptureFolder)
```

`CaptureFolder` is where WA saves extracted replay frames (`.png` files). `GifCreator` reads from this path after calling `ExtractReplayFrames`.

## WA command-line arguments

| Operation | Args |
|---|---|
| Host game | `wa://` |
| Generate replay log | `/getlog "<replayPath>" /quiet` |
| Play replay | `/play "<replayPath>" /quiet` |
| Play from offset | `/playat "<replayPath>" <time> /quiet` |
| Extract frames | `/getvideo "<replayPath>" <fps> <start> <end> <w> <h> /quiet` |

Time format for `/getvideo` and `/playat` is `hh\:mm\:ss\.ff`.

## Worms.Armageddon.Game.Fake

A test double for `IWormsArmageddon`. Used in unit tests where a real WA installation is not available. Implement any fakes here rather than using mocking libraries.

## Worms.Armageddon.Game.Tests

Tests for game discovery and runner logic. Run with `dotnet test src/Worms.Armageddon.Game.Tests`. Uses NUnit + NSubstitute for mocking `IRegistry` and other Windows-specific dependencies.
