# Learnings: Placement Persistence

## Implementation Notes

### `TryAddScoped` requires `Microsoft.Extensions.DependencyInjection.Extensions`

The plan said to call `builder.TryAddScoped<IFeatureFlags, GatewayFeatureFlags>()` in `AddWorkerServices()`. The Gateway project uses `Microsoft.NET.Sdk.Web`, which provides implicit usings including the `Microsoft.Extensions.DependencyInjection` namespace, but `TryAddScoped` is defined in `Microsoft.Extensions.DependencyInjection.Extensions` — a separate namespace that is **not** included in the implicit usings. An explicit `using Microsoft.Extensions.DependencyInjection.Extensions;` was required. The existing `Worms.Armageddon.Files/ServiceRegistration.cs` (which also uses `TryAddScoped`) already has this using, confirming the pattern.

### `[SuppressMessage]` cannot annotate a `catch` clause directly

The plan prescribed catching `Exception` in `PlacementsBackfillService` so the backfill continues per-replay. Roslyn's CA1031 ("Do not catch general exception types") treats this as a warning-as-error. The `[SuppressMessage]` attribute cannot be placed directly before a `catch` clause — it is not valid C# syntax at that position. Instead, the attribute was placed on the enclosing method (`ExecuteAsync`), which is the standard pattern used elsewhere in this codebase (e.g. `Worms.Cli/Runner.cs`, `Worms.Cli/Program.cs`).

### Roslynator RCS1124 — inline single-use local variables

The `var replays = ...` and `var count = ...` locals were flagged by Roslynator RCS1124 ("Inline local variable") because each was used in exactly one subsequent statement. Both were inlined: the count query was moved directly into the `if` condition, and the replays query was moved directly into the `foreach` expression.

## Files Added (not in plan)

None — all created and modified files were listed in the plan's "Files to Create / Modify" table.
