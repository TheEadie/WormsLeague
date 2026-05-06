# CLI Component

Projects: `Worms.Cli`, `Worms.Cli.Resources`

## Command structure

Commands are wired in `CliStructure.BuildCommandLine()`. The structure follows a kubectl-style resource verb model:

```
worms auth
worms host
worms version
worms update
worms get <scheme|replay|game>
worms delete <scheme|replay>
worms create <scheme|gif>
worms browse <scheme|replay|gif>
worms view <replay>
worms process <replay>
```

Each resource sub-command lives in its own file at `Commands/Resources/<ResourceType>/<Verb><ResourceType>.cs`. The file contains two types:

- `<Verb><ResourceType> : Command` — declares the command name, aliases, arguments and options
- `<Verb><ResourceType>Handler : AsynchronousCommandLineAction` — the execution logic, injected via DI

Handlers are registered in `ServiceRegistration.AddWormsCliServices()` and resolved via `IServiceProvider` when the command is invoked. No handler is ever instantiated until its command runs.

## Adding a new command

1. Create a `Command` subclass + `AsynchronousCommandLineAction` subclass in the appropriate `Commands/Resources/<Type>/` folder.
2. Register the handler in `Worms.Cli/ServiceRegistration.cs`.
3. Wire it under the correct verb group in `CliStructure.BuildCommandLine()`.

## Resource printing

Output goes through `IResourcePrinter<T>` implementations (e.g. `SchemeTextPrinter`, `ReplayTextPrinter`, `GameTextPrinter`). Table formatting uses `TableBuilder` / `TablePrinter` in `Logging/TableOutput/`. Column widths adapt to `Console.WindowWidth` (fallback 80).

## Worms.Cli.Resources

Provides all HTTP-facing services used by the CLI:

- `IWormsServerApi` — typed HTTP client for the hub gateway (games, replays, schemes, CLI files, league)
- Auth via device flow: `ILoginService`, `IAccessTokenRefreshService` — tokens stored with DPAPI via `TokenStore` at `%LOCALAPPDATA%/Programs/Worms/tokens.json`
- `IRemoteLeagueRetriever`, `IRemoteSchemeDownloader`, `ICliUpdateDownloader` for specific hub operations

This project targets `EnableTrimAnalyzer`, `EnableSingleFileAnalyzer`, `EnableAotAnalyzer` because it ships inside the self-contained CLI binary. Avoid reflection-based serialisation — use `JsonContext` (source-generated `JsonSerializerContext`) for any new JSON types.

## DI registration pattern

Each `ServiceRegistration` extension follows the same pattern: a single `Add*Services(this IServiceCollection builder)` extension method that returns `IServiceCollection`, registered as `Scoped` unless there is a concrete reason for `Singleton`.

## Logging

All output goes to stderr via `ColorFormatter`. Use `ILogger<T>` in handlers; do not write to `Console` directly except for actual command output (i.e. the table of resources). Verbosity is controlled by `-v`/`-q` flags parsed in `Program.GetLogLevel()`.

## Telemetry

OpenTelemetry traces are started in `Program.Main` before DI setup. Each significant operation gets a span tag via `Activity.Current?.SetTag(...)`. Span names are constants in the `Telemetry` class.
