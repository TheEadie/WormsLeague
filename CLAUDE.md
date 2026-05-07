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

Docker Compose brings up: Azurite (Azure Storage emulator), PostgreSQL, Flyway migrations, hub-gateway, hub-worker, and hub-wa-runner.

```bash
docker compose up
```

Config is via `WORMS_`-prefixed environment variables (e.g. `WORMS_CONNECTIONSTRINGS__DATABASE`). Set `WA_GAME_PATH` to point at a local Worms Armageddon installation.

Database migrations live in `src/database/migrations/` (Flyway). Local-dev seed data is in `src/database/local-dev/`.

## Architecture

For a high-level overview of the components and how they relate, see [.claude/docs/steering/architecture.md](.claude/docs/steering/architecture.md).

## Coding Guidelines

Cross-cutting conventions (visibility, DI, testing, telemetry, formatting) are documented in [.claude/docs/steering/coding-guidelines.md](.claude/docs/steering/coding-guidelines.md). Follow these for any new code.

## Testing Strategy

How the unit and integration tiers are organised, what to put where, and how they run in CI vs locally is covered in [.claude/docs/steering/testing-strategy.md](.claude/docs/steering/testing-strategy.md).

## Component Docs

Detailed practices for each component are in `.claude/docs/components/`. Load the relevant doc when working in that area:

| Component | Doc | Projects |
|---|---|---|
| CLI | [.claude/docs/components/cli.md](.claude/docs/components/cli.md) | `Worms.Cli`, `Worms.Cli.Resources` |
| Hub Gateway | [.claude/docs/components/hub-gateway.md](.claude/docs/components/hub-gateway.md) | `Worms.Hub.Gateway` |
| Hub Storage | [.claude/docs/components/hub-storage.md](.claude/docs/components/hub-storage.md) | `Worms.Hub.Storage` |
| Hub Queues | [.claude/docs/components/hub-queues.md](.claude/docs/components/hub-queues.md) | `Worms.Hub.Queues` |
| WA Runner | [.claude/docs/components/wa-runner.md](.claude/docs/components/wa-runner.md) | `Worms.Hub.Armageddon.Runner` |
| Armageddon Files | [.claude/docs/components/armageddon-files.md](.claude/docs/components/armageddon-files.md) | `Worms.Armageddon.Files` |
| Armageddon Game | [.claude/docs/components/armageddon-game.md](.claude/docs/components/armageddon-game.md) | `Worms.Armageddon.Game`, `*.Fake` |
| Armageddon Gifs | [.claude/docs/components/armageddon-gifs.md](.claude/docs/components/armageddon-gifs.md) | `Worms.Armageddon.Gifs` |
| Infrastructure | [.claude/docs/components/infrastructure.md](.claude/docs/components/infrastructure.md) | `deployment/Worms.Hub.Infrastructure` |

