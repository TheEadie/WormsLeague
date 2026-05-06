# WA Runner Component

Project: `Worms.Hub.Armageddon.Runner`

A .NET Worker Service that runs inside a Docker container and processes replay files using Worms Armageddon via Wine.

## What it does

Consumes messages from the `replays-to-process` queue, runs WA to extract the replay log and generate a GIF of the best turn, then publishes results to the `replays-to-update` queue.

### Processing steps (Processor.ProcessReplay)

1. Dequeue a `ReplayToProcessMessage` from `replays-to-process`
2. Verify the replay file exists in `Storage:TempReplayFolder`
3. Check WA is installed at `/root/.wine/drive_c/WA`; if not, copy from the `/game` volume mount
4. Call `IWormsArmageddon.GenerateReplayLog(replayPath)` — runs `WA.exe /getlog ... /quiet` via Wine
5. Parse the log with `IReplayTextReader`
6. Select the turn with the highest total damage
7. Attempt to generate a GIF for that turn via `GifCreator` (failure is logged but non-fatal)
8. Enqueue a `ReplayToUpdateMessage` to `replays-to-update`
9. Delete the input message from the queue

## Wine integration

The container uses Wine to run WA on Linux. Game files are expected at `/root/.wine/drive_c/WA`. On first run, if the game is not found there, the runner copies files from the `/game` volume mount (populated at container start).

The `Linux.WormsRunner` implementation runs `wine WA.exe <args>` via `IProcessRunner`.

## Docker image

Built from `build/docker/wa-runner/Dockerfile`. The image includes Wine, libicu-dev, and other dependencies needed to run WA. The build uses `docker buildx bake`.

Make targets:
```bash
make wa-runner.build    # build the image
make wa-runner.package  # push to registry
```

## Integration test

`Worms.Hub.Armageddon.Runner.Tests` contains a single `[Category("Integration")]` test (`ProcessReplayShould`) that:
- Requires WA game files at `$WA_GAME_PATH` (or `/home/eadie/games/worms` by default)
- Requires `sample-data/replays/sample.WAGame` to exist
- Builds the Docker image and starts `hub-wa-runner` + `azure-storage` via `docker compose`
- Enqueues a `ReplayToProcessMessage` and polls for a log file + output queue message (up to 5 minutes)

Run with:
```bash
dotnet test src/Worms.Hub.Armageddon.Runner.Tests --filter Category=Integration
```

The test manages service lifecycle (`[OneTimeSetUp]` / `[OneTimeTearDown]`) — do not run alongside a live `docker compose up` stack.

## Configuration

| Config key | Purpose |
|---|---|
| `ConnectionStrings:Storage` | Azurite / Azure Storage Queue |
| `Storage:TempReplayFolder` | Shared volume path for replay files |

## Telemetry

Spans use `ActivityKind.Consumer` with the parent context extracted from the queue message via W3C trace propagation (see hub-queues.md).
