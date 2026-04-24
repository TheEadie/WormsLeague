# Armageddon Files Component

Project: `Worms.Armageddon.Files`

Parses and writes Worms Armageddon file formats. Has no external dependencies beyond `Syroot.Worms.Armageddon` (scheme binary format) and standard .NET libraries.

## Scheme files

Two representations of a scheme:

- **Binary (`.wsc`)** — read/written via `IWscReader` / `IWscWriter` wrapping `Syroot.Worms.Armageddon.Scheme`. Note: the upstream library has a known bug where `FiringPausesTimer` defaults to `false` for `SchemeVersion.Version1`; `WscReader` corrects this.
- **Text** — human-readable format read/written via `ISchemeTextReader` / `ISchemeTextWriter` in `Schemes/Text/`.

`IRandomSchemeGenerator` generates random scheme settings within legal bounds.

## Replay parsing

Replays are parsed from the WA text log (`.log` sidecar file) via a pipeline of `IReplayLineParser` implementations:

| Parser | Matches |
|---|---|
| `StartTimeParser` | Game start date/time |
| `TeamParser` | Team names, machines, colours |
| `WinnerParser` | Winner announcement line |
| `StartOfTurnParser` | Turn start timestamp |
| `WeaponUsedParser` | Weapon fired lines |
| `DamageParser` | Damage dealt lines |
| `EndOfTurnParser` | Turn end timestamp |

`ReplayTextReader` iterates lines, asks each registered parser `CanParse(line)`, and calls `Parse(line, builder)` on matches. Multiple parsers can match the same line.

`ReplayResourceBuilder` accumulates the parsed data and builds the final `ReplayResource` record. `TurnBuilder` tracks per-turn state (`WithStartTime`, `WithEndTime`, `WithDamage`, etc.).

### Adding a new parser

1. Implement `IReplayLineParser` with a `[GeneratedRegex]`-based `CanParse` and a `Parse` that mutates the builder.
2. Register it in `ServiceRegistration.AddWormsArmageddonFilesServices()` as `AddScoped<IReplayLineParser, NewParser>()`.

## Domain model (Replays)

```
ReplayResource
├── DateTime Date
├── bool Processed
├── IReadOnlyCollection<Team> Teams
├── string Winner
├── string FullLog
└── IReadOnlyCollection<Turn> Turns
    └── Turn
        ├── TimeSpan Start / End
        ├── Team Team
        ├── IReadOnlyCollection<Weapon> Weapons
        └── IReadOnlyCollection<Damage> Damage
```

All types are `record`s with `[PublicAPI]` (JetBrains annotation, used by ReSharper to suppress unused-member warnings since these are consumed externally).

## Replay filename parsing

`IReplayFilenameParser` / `ReplayFilenameParser` extracts metadata (date, player names) from the WA replay filename convention.

## Regex style

Parsers use C# 9+ `[GeneratedRegex]` source-generated regexes (`partial class` with `partial static Regex Xxx()` method). This avoids runtime regex compilation overhead, which matters for files processing many replay log lines.

## Testing

`Worms.Armageddon.Files.Tests` uses NUnit + Shouldly. Tests are round-trip tests (write then read, verify no data loss) and snapshot tests against known `TestSchemes` data. No mocking — tests use real service registrations via `new ServiceCollection().AddWormsArmageddonFilesServices()`.
