# Testing Strategy

How tests are organised in this repo and what each tier is for. For naming and category conventions see [coding-guidelines.md](coding-guidelines.md); this doc covers the *strategy* — what we test, where, and why.

## Tiers

We have two test tiers, separated by NUnit category:

### Unit tests (default)

Fast, in-process tests with no external dependencies. The test projects that exist today cover:

- File-format parsing and serialisation in the Armageddon Files library
- Game-discovery and runner logic in the Armageddon Game library (with `NSubstitute` for OS-specific seams like the registry)
- The Armageddon Gifs assembly logic, exercised against fixtures

Unit tests use **NUnit** with **Shouldly** for assertions. They are the default `dotnet test` run and gate every PR via `make cli.test.unit` / equivalent.

The CLI, hub gateway, queues, and storage projects do not currently have dedicated unit-test projects — behaviour at those layers is exercised indirectly via the integration tier and the libraries above. When adding meaningful logic at those layers, prefer adding a new `<Project>.Tests` rather than retrofitting the integration test.

External dependencies are abstracted at the seam, not mocked at the call site:
- File-system access goes through `System.IO.Abstractions` so unit tests use `MockFileSystem` instead of touching disk.
- The Worms Armageddon process is abstracted by the Armageddon Game library, with a dedicated `Worms.Armageddon.Game.Fake` project providing a deterministic test double — units that orchestrate WA depend on the abstraction and are wired up against the fake in tests.

### Integration tests (`Category=Integration`)

These exercise real infrastructure end-to-end. They are excluded from the default unit run and selected with `--filter Category=Integration`. Currently the main one is the WA Runner integration test, which:

- Builds the wa-runner Docker image
- Starts Azurite via `docker compose`
- Enqueues a real replay message
- Waits for the runner to process the replay and produce an output message
- Asserts on the resulting log/GIFs

Integration tests have prerequisites (Docker available; in some cases a real Worms Armageddon installation on the host) and are slower than unit tests, so they are not part of the default local loop. They are intentionally not mocked — the value of the tier comes from running real Docker, real Azurite queues, and the real WA binary under Wine.

The database is in the same bucket: prefer integration tests against a real Postgres (the one from `docker compose`) over mocking repositories. Mocks of the data layer have historically diverged from production behaviour and hidden migration bugs.

## What to write where

When adding a feature, default to **unit tests** for everything that can be exercised through abstractions. Reach for an **integration test** when:

- The behaviour only exists when the real infrastructure is present (e.g. a queue message round-trips, a replay actually runs through Wine + WA, a migration applies cleanly to Postgres).
- A unit test would require mocking so much that the test is really just asserting the mock setup.
- You're verifying behaviour at a deployment boundary (image starts, env vars are read correctly, services compose).

Don't duplicate: if a behaviour is meaningfully covered by a unit test, you don't owe it an integration test as well.

## CI vs local

- **CI** runs unit tests on every PR. Integration tests that need Docker/WA run on machines provisioned for it — don't add an integration test that only the author can run.
- **Locally**, `dotnet test` (or the make targets) runs the unit suite. Integration tests are opt-in via the category filter so you don't accidentally pay the Docker startup cost on every change.

## Fixtures and sample data

Sample replays and schemes live under `sample-data/` and are committed to the repo. Tests should reference fixtures from that folder rather than generating ad-hoc binary blobs inline. Add a new fixture when the existing ones genuinely don't cover the case — don't fork an existing one for a one-character variation.
