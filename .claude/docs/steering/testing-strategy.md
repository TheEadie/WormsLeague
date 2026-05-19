# Testing Strategy

How tests are organised in this repo and what each tier is for. For naming and category conventions see [coding-guidelines.md](coding-guidelines.md); this doc covers the *strategy* — what we test, where, and why.

## Tiers

We have two test tiers, separated by NUnit category:

### Unit tests (default)

Fast, in-process tests with no external dependencies. The test projects that exist today cover:

- File-format parsing and serialisation in the Armageddon Files library
- Game-discovery and runner logic in the Armageddon Game library (with `NSubstitute` for OS-specific seams like the registry)
- The Armageddon Gifs assembly logic, exercised against fixtures
- React components in the Web UI (Vitest + React Testing Library; run via `make web.test`)

Unit tests for .NET use **NUnit** with **Shouldly** for assertions. They are the default `dotnet test` run and gate every PR via `make cli.test.unit` / equivalent. Web unit tests run independently via `make web.test`.

The CLI, hub gateway, queues, and storage projects do not currently have dedicated unit-test projects — behaviour at those layers is exercised indirectly via the integration tier and the libraries above. When adding meaningful logic at those layers, prefer adding a new `<Project>.Tests` rather than retrofitting the integration test.

When a slice introduces non-trivial logic into the Gateway — calculators, formatters, ranking, leaderboard builders — the slice creates the gateway test project rather than deferring. An acceptance criterion that calls for unit tests cannot be discharged by pointing at the absence of a test project; the slice that introduces the logic introduces the project.

External dependencies are abstracted at the seam, not mocked at the call site:
- File-system access goes through `System.IO.Abstractions` so unit tests use `MockFileSystem` instead of touching disk.
- The Worms Armageddon process is abstracted by the Armageddon Game library, with a dedicated `Worms.Armageddon.Game.Fake` project providing a deterministic test double — units that orchestrate WA depend on the abstraction and are wired up against the fake in tests.
- The database is abstracted behind `IRepository<T>` (and the per-aggregate repository interfaces) in `Worms.Hub.Storage`. Unit tests of Hub layers above storage — Gateway endpoints, Worker handlers, Queue producers/consumers — mock or fake those repository interfaces so the suite stays fast and in-process. Do not stand up Postgres for tests of code that only talks to the repository contract.

### Integration tests (`Category=Integration`)

These exercise real infrastructure end-to-end. They are excluded from the default unit run and selected with `--filter Category=Integration`. Currently the main one is the WA Runner integration test, which:

- Builds the wa-runner Docker image
- Starts Azurite via `docker compose`
- Enqueues a real replay message
- Waits for the runner to process the replay and produce an output message
- Asserts on the resulting log/GIFs

Integration tests have prerequisites (Docker available; in some cases a real Worms Armageddon installation on the host) and are slower than unit tests, so they are not part of the default local loop. They are intentionally not mocked — the value of the tier comes from running real Docker, real Azurite queues, and the real WA binary under Wine.

### Repository contract tests

Repository implementations themselves are the one place where mocks cannot protect us — a `GamesRepository` that compiles fine against a stale SQL query will still pass any higher-layer test that mocks it, but will throw at runtime once the schema moves. Each repository needs a small, dedicated integration test suite (`Category=Integration`) that exercises every method against a real Postgres from `docker compose` and asserts on round-trip behaviour: insert via `Add`, read via `GetAll`/`GetById`, update via `Update`, and any bespoke query method on the per-aggregate repo. These tests guard against drift between SQL strings, the `XxxDb` mapping record, and the live schema. They are the reason higher layers can mock repositories safely — if a mock returns something the real repo could never produce, that's a code-review issue; if the real repo's SQL has drifted from the schema, the contract test catches it before production does.

A slice that adds a new repository method must add a contract test for that method in the same slice. A slice that changes the schema must update affected contract tests, not defer them.

## What to write where

When adding a feature, default to **unit tests** for everything that can be exercised through abstractions. Reach for an **integration test** when:

- The behaviour only exists when the real infrastructure is present (e.g. a queue message round-trips, a replay actually runs through Wine + WA, a migration applies cleanly to Postgres).
- A unit test would require mocking so much that the test is really just asserting the mock setup.
- You're verifying behaviour at a deployment boundary (image starts, env vars are read correctly, services compose).

Don't duplicate: if a behaviour is meaningfully covered by a unit test, you don't owe it an integration test as well.

## `make test` means tests, not linting

`make test` runs actual tests — unit or integration. Do not wire linting, static analysis, or format checks into `make test` or the `test::` phony target.

Linting (ESLint, Prettier, `tsc --noEmit`, Roslyn analysers) belongs in `code-scanning.yml` as SARIF uploads, consistent with how .NET static analysis is handled in this repo. See [ci-patterns.md](ci-patterns.md).

## CI vs local

- **CI** runs unit tests on every PR. Integration tests that need Docker/WA run on machines provisioned for it — don't add an integration test that only the author can run.
- **Locally**, `dotnet test` (or the make targets) runs the unit suite. Integration tests are opt-in via the category filter so you don't accidentally pay the Docker startup cost on every change.

## Fixtures and sample data

Sample replays and schemes live under `sample-data/` and are committed to the repo. Tests should reference fixtures from that folder rather than generating ad-hoc binary blobs inline. Add a new fixture when the existing ones genuinely don't cover the case — don't fork an existing one for a one-character variation.

## Seed data as an acceptance criterion

Any slice that adds a new column, chip, page section, or other user-visible surface must update local-dev seed data so the new surface is exercisable in `docker compose up`. This is part of the slice's acceptance criteria, not a follow-up. Reviewers treat unchecked manual-test items in the PR body as a blocker — "satisfies all acceptance criteria" is not a valid verdict when the seed data does not exercise the new surface end-to-end.
