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
- Auth via device flow: `ILoginService` (`DeviceCodeLoginService`), `IAccessTokenRefreshService`, `IUserDetailsService` — tokens are encrypted via ASP.NET Core Data Protection and persisted by `TokenStore` under `Environment.SpecialFolder.LocalApplicationData` at `Programs/Worms/tokens.json`
- `IRemoteLeagueRetriever`, `IRemoteSchemeDownloader`, `IRemoteGameUpdater`, `ICliUpdateRetriever`, `ICliUpdateDownloader` for specific hub operations
- A platform-specific `IFolderOpener` and `ICliUpdateDownloader` are registered at startup based on `RuntimeInformation.IsOSPlatform`

This project targets `EnableTrimAnalyzer`, `EnableSingleFileAnalyzer`, `EnableAotAnalyzer` because it ships inside the self-contained CLI binary. Avoid reflection-based serialisation — use `JsonContext` (source-generated `JsonSerializerContext`) for any new JSON types.

## DI registration pattern

Each `ServiceRegistration` extension follows the same pattern: a single `Add*Services(this IServiceCollection builder)` extension method that returns `IServiceCollection`, registered as `Scoped` unless there is a concrete reason for `Singleton`.

## Logging

All output goes to stderr via `ColorFormatter`. Use `ILogger<T>` in handlers; do not write to `Console` directly except for actual command output (i.e. the table of resources). Verbosity is controlled by `-v`/`-q` flags parsed in `Program.GetLogLevel()`.

## Telemetry

OpenTelemetry traces are started in `Program.Main` before DI setup. Each significant operation gets a span tag via `Activity.Current?.SetTag(...)`. Span names are constants in the `Telemetry` class.

## Testing

CLI unit tests live in `src/Worms.Cli.Tests` (NUnit + Shouldly). Tests drive the CLI through the same `CliStructure.BuildCommandLine()` root + DI container that production uses, via the `TestHost` composition root. `TestHost` overrides five production seams:

- `IHttpClientFactory` — backed by a recording handler (`RecordingHttpMessageHandler`) that captures requests and returns scripted responses.
- `IFileSystem` — `MockFileSystem` so `TokenStore` reads and writes in memory.
- `IBrowserLauncher` — recording test impl; no real browser is launched. The interface wraps the original static `BrowserLauncher` so production behaviour is unchanged.
- `TimeProvider` — `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing` so the device-code polling loop and its timeout can be fast-forwarded deterministically.
- `IFolderOpener` — `RecordingFolderOpener` captures the folder paths passed to `OpenFolder` so tests can assert on which folder was opened without launching a real file manager.

`TestHost` also registers the `Worms.Armageddon.Game.Fake` services so `IWormsArmageddon` is the in-memory fake — installed by default; opt in to a 'not installed' fake via `new TestHost(wormsInstalled: false)`. The fake is wrapped in a small recording decorator (`RecordingWormsArmageddon`) so tests can observe which `PlayReplay` calls the CLI issued.

When adding tests for a new command, extend this project rather than creating a new one. Test classes follow the `<TypeUnderTest>Should` convention; test methods describe a behaviour.
