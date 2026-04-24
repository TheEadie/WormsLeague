# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

Tooling and server infrastructure for a Worms Armageddon league. It consists of two main deliverables:

- **`worms` CLI** — a cross-platform command-line tool players use to authenticate, host games, manage schemes and replays
- **Worms Hub** — a server-side system (ASP.NET Core gateway + background worker + WA runner) that manages games, stores replays, and announces results to Slack

## Build System

The project uses `make` with separate makefiles per component included at the root level. There is no single root `Makefile` — targets come from `build/cli/makefile` and `build/docker/makefile`.

### CLI

```bash
make cli.build          # dotnet build Worms.Cli
make cli.test.unit      # dotnet test --filter "Category!=Integration"
make cli.package.windows
make cli.package.linux
make cli.package.docker
```

Run a single test project directly:

```bash
dotnet test src/Worms.Armageddon.Files.Tests
dotnet test src/Worms.Armageddon.Game.Tests
dotnet test src/Worms.Hub.Armageddon.Runner.Tests --filter Category=Integration
```

### Hub (Docker)

```bash
make gateway.build      # docker buildx bake the gateway image
make gateway.package
make wa-runner.build
make wa-runner.package
```

### Local Development Stack

Docker Compose brings up: Azurite (Azure Storage emulator), PostgreSQL 17, Flyway migrations, hub-gateway, hub-worker, and hub-wa-runner.

```bash
docker compose up
```

Config is via `WORMS_`-prefixed environment variables (e.g. `WORMS_CONNECTIONSTRINGS__DATABASE`). Set `WA_GAME_PATH` to point at a local Worms Armageddon installation.

Database migrations live in `src/database/migrations/` (Flyway). Local-dev seed data is in `src/database/local-dev/`.

## Project Structure

```
src/
  Worms.Cli/                      # CLI entry point (System.CommandLine, self-contained exe)
  Worms.Cli.Resources/            # HTTP client services used by the CLI (auth, remote API calls)
  Worms.Armageddon.Game/          # Abstraction over launching/detecting WA game process
  Worms.Armageddon.Game.Fake/     # Test double for WA game
  Worms.Armageddon.Files/         # Reading/writing WA file formats (schemes .wsc, replays .WAgame)
  Worms.Armageddon.Gifs/          # GIF generation from replays
  Worms.Hub.Gateway/              # ASP.NET Core Web API (games, replays, schemes, leagues, CLI files)
  Worms.Hub.Storage/              # Dapper+Postgres repositories and Azure Blob/file abstractions
  Worms.Hub.Queues/               # Azure Storage Queue abstractions
  Worms.Hub.ReplayProcessor.Queues/
  Worms.Hub.ReplayUpdater.Queues/
  Worms.Hub.Armageddon.Runner/    # Worker service: runs WA game inside Docker via Wine, posts replay
  database/                       # Flyway migrations and schema model

deployment/
  Worms.Hub.Infrastructure/       # Pulumi (C#) for Azure infrastructure (azure-native, northeurope)

build/
  cli/                            # makefile + Dockerfile for CLI packaging
  docker/gateway/                 # makefile + Dockerfile + docker-bake.hcl for gateway
  docker/wa-runner/               # makefile + Dockerfile + docker-bake.hcl for WA runner
```

## Component Docs

Detailed practices for each component are in `agent-docs/`. Load the relevant doc when working in that area:

| Component | Doc | Projects |
|---|---|---|
| CLI | [agent-docs/cli.md](agent-docs/cli.md) | `Worms.Cli`, `Worms.Cli.Resources` |
| Hub Gateway | [agent-docs/hub-gateway.md](agent-docs/hub-gateway.md) | `Worms.Hub.Gateway` |
| Hub Storage | [agent-docs/hub-storage.md](agent-docs/hub-storage.md) | `Worms.Hub.Storage` |
| Hub Queues | [agent-docs/hub-queues.md](agent-docs/hub-queues.md) | `Worms.Hub.Queues`, `*.ReplayProcessor.Queues`, `*.ReplayUpdater.Queues` |
| WA Runner | [agent-docs/wa-runner.md](agent-docs/wa-runner.md) | `Worms.Hub.Armageddon.Runner` |
| Armageddon Files | [agent-docs/armageddon-files.md](agent-docs/armageddon-files.md) | `Worms.Armageddon.Files` |
| Armageddon Game | [agent-docs/armageddon-game.md](agent-docs/armageddon-game.md) | `Worms.Armageddon.Game`, `*.Fake` |
| Armageddon Gifs | [agent-docs/armageddon-gifs.md](agent-docs/armageddon-gifs.md) | `Worms.Armageddon.Gifs` |
| Infrastructure | [agent-docs/infrastructure.md](agent-docs/infrastructure.md) | `deployment/Worms.Hub.Infrastructure` |

## Architecture Notes

### Hub Gateway modes

`Worms.Hub.Gateway` is a single binary that can run in three modes controlled by environment variables:

- **Monolith** (default): runs both gateway (HTTP API) and worker (queue processor) in-process
- **Distributed gateway** (`WORMS_HUB_DISTRIBUTED=true` + `WORMS_HUB_GATEWAY=true`): HTTP API only
- **Distributed worker** (`WORMS_HUB_DISTRIBUTED=true` + `WORMS_HUB_WORKER=true`): queue consumer only
- **Batch job** (`WORMS_BATCH=true`): processes one replay from the queue then exits

### Replay processing flow

1. CLI uploads a `.WAgame` file to the gateway (`ReplaysController`)
2. Gateway stores the file and enqueues a `ReplayToUpdateMessage` (Azure Storage Queue)
3. Worker (`CheckForMessagesService` → `Processor`) dequeues, reads the log, updates the DB, announces the winner to Slack

### CLI command structure

Commands follow a kubectl-style resource model: `worms get replays`, `worms delete schemes`, `worms browse gifs`, etc. The entry point is `CliStructure.BuildCommandLine()`. Each resource type has its own command file under `Commands/Resources/<ResourceType>/`.

### Infrastructure

Deployed on Azure (Pulumi, C#). Images are published to Docker Hub as `theeadie/worms-hub-gateway` and `theeadie/worms-hub-wa-runner`. Versioning is derived from git tags via `build/version.sh`.

### Auth

The hub uses JWT Bearer auth (Auth0-style). The CLI authenticates via device flow and stores tokens locally (`TokenStore`). Config keys: `WORMS_AUTH__AUTHORITY`, `WORMS_AUTH__AUDIENCE`, `WORMS_AUTH__NAMECLAIM`, `WORMS_AUTH__PERMISSIONSCLAIM`.

### Target frameworks

- CLI and shared libraries: multi-targeted (net8.0 + net9.0 in dev, published as self-contained single-file)
- Hub Gateway / WA Runner: ASP.NET Core / Worker SDK
- CI uses .NET 10.0.x
