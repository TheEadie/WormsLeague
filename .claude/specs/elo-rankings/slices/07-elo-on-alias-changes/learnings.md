# Learnings: ELO on Alias Changes

## Implementation Notes

### Roslynator RCS1124 requires inlining the local variable in CalculateForTeam

The plan's code sample for `CalculateForTeam` assigned `GetAffectedLeagueIds` to a local variable before iterating it:

```csharp
var leagueIds = replaysRepository.GetAffectedLeagueIds(machine, teamName);
foreach (var leagueId in leagueIds)
```

The Roslynator analyser (RCS1124 — Inline local variable) treats this as a warning, which becomes a build error under `--warnaserror`. The fix is to inline the call directly into the `foreach`:

```csharp
foreach (var leagueId in replaysRepository.GetAffectedLeagueIds(machine, teamName))
```

This is consistent with how the rest of the codebase uses LINQ results directly.

### AddGatewayServices was a single-expression method — expanded to block body

The plan showed `AddGatewayServices` as a multi-statement block with `TryAddScoped<RatingsCalculator>()` appended. In the actual codebase it was a single expression-bodied method using fluent chaining (`=>`). Because `TryAddScoped` is a void call that cannot be chained fluently, the method had to be converted from an expression body to a block body. The plan implied this change but did not call it out explicitly.

## Files Added (not in plan)

None — all modified files were listed in the plan's "Files to Create / Modify" table.
