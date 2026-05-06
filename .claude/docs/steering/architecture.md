# Architecture

A high-level overview of the components in this repo and how they fit together. For component internals (types, conventions, configuration) see the per-component docs under `.claude/docs/components/`.

## What the system does

The Worms League consists of two user-facing surfaces:

- The **CLI** (`worms`) is what players run on their own machines. It authenticates them, hosts and joins games, manages local schemes, and uploads replays.
- The **Hub** is a server-side system that ingests those replays, runs them headlessly to extract results and animated GIFs of key moments, records everything in a database, and announces winners to Slack.

## Components

The codebase is organised into three groups of components.

### Shared Worms-domain libraries

These wrap the Worms Armageddon game itself and its on-disk file formats. They have no knowledge of the hub or the CLI and are consumed by both.

- **Armageddon Files** — reads and writes the WA file formats: replays (`.WAgame`) and schemes (`.wsc`). Pure file-format code with no process or network concerns.
- **Armageddon Game** — abstraction over the WA executable: locating an installation, launching the game, and driving it to extract logs and frames from a replay. Has a `Fake` sibling project for tests so callers can avoid spawning a real WA process.
- **Armageddon Gifs** — assembles animated GIFs from frames produced by Armageddon Game.

### CLI

The CLI is a self-contained executable shipped to players.

- **Worms.Cli** — command-line entry point and command/handler wiring.
- **Worms.Cli.Resources** — the typed HTTP client over the Hub Gateway, plus auth (device-flow login and token storage).

The CLI uses the shared Armageddon libraries directly when it needs to launch the game locally or work with replay files on disk.

### Hub

The hub is one service that can run as a monolith or be split across multiple containers, plus a separate runner that hosts the actual game.

- **Hub Gateway** — the ASP.NET Core HTTP API and the queue-consuming worker. Same binary, mode chosen by environment variables. The gateway is the only inbound HTTP surface for the CLI and is the component that announces results to Slack.
- **Hub Storage** — repositories for the Postgres database and abstractions over Azure Blob Storage / local files. Used by everything in the hub that needs to persist or read state.
- **Hub Queues** — Azure Storage Queue abstractions for the messages that flow between the gateway and the runner. Shared by both ends.
- **WA Runner** — a worker service that runs Worms Armageddon inside a Docker image (under Wine on Linux) to replay games headlessly. It consumes work from a queue, calls the shared Armageddon Game and Gifs libraries to extract logs and GIFs, then publishes results back via another queue.

### Infrastructure

- **Worms.Hub.Infrastructure** — Pulumi (C#) program that provisions the Azure resources the hub runs on, plus Cloudflare DNS and Flyway database migrations.

## How they fit together

At a high level:

- The CLI talks to the Hub Gateway over HTTPS using the typed client in Worms.Cli.Resources. Auth is JWT bearer; the CLI obtains tokens via device flow against the same authority the gateway validates against.
- The Hub Gateway is the only component that accepts CLI traffic. It owns the database (via Hub Storage) and decides what work needs to be done downstream.
- Long-running game work is handed off via Hub Queues. The gateway enqueues a "replay to process" message; the WA Runner picks it up, runs the game to produce a log and GIFs, then enqueues a "replay to update" message. The gateway's worker mode consumes that, persists the results, and announces the winner to Slack.
- The CLI and the WA Runner both depend on the shared Armageddon libraries — the runner uses them to drive a headless WA inside its container; the CLI uses them when a player runs commands locally that touch real WA files or processes.
- Hub Storage and Hub Queues are pure infrastructure abstractions: they hide whether the backing store is Azure (in production) or Azurite/Postgres-in-Docker (in local dev) so the rest of the hub doesn't care.

## Deployment topology

In production:

- The CLI is distributed as a self-contained binary and runs on player machines.
- Hub Gateway is deployed twice from the same image — once as the HTTP API, once as the queue worker — both in Azure Container Apps.
- WA Runner is deployed as its own image, scaled separately because each instance needs to spin up Wine + WA.
- Postgres and Azure Storage (blobs + queues) are managed Azure services, provisioned by the infrastructure project.

In local development a single `docker compose up` brings up Azurite, Postgres, Flyway-applied migrations, and the hub services so the whole flow can run end-to-end against a local stack.
