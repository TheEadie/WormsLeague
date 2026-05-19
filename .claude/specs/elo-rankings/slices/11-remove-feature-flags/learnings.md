# Learnings: Remove Feature Flags

## Implementation Notes

### `using Microsoft.Extensions.DependencyInjection.Extensions;` must be retained in Gateway `ServiceRegistration`

The plan's step 2 ("`ServiceRegistration.cs` (Hub Gateway)") instructs removal of
`using Worms.Hub.Gateway.FeatureFlags;` from the file but says nothing about the
existing `using Microsoft.Extensions.DependencyInjection.Extensions;` directive.
That directive is still required because `TryAddScoped<RatingsCalculator>()`
remains in both `AddGatewayServices` and `AddWorkerServices`. An initial rewrite
that dropped both `using`s left `TryAddScoped` unresolved; the
`Microsoft.Extensions.DependencyInjection.Extensions` `using` was re-added.
Worth mentioning in the plan template so the implementer doesn't strip both
namespaces by reflex.

### Pre-existing `NU1902` errors on `Worms.Hub.Infrastructure` block a full-solution `dotnet build --warnaserror`

`dotnet build --warnaserror` at the solution root fails with `NU1902` package
vulnerability errors for `OpenTelemetry.Api` 1.9.0 and
`OpenTelemetry.Exporter.OpenTelemetryProtocol` 1.9.0 in
`deployment/Worms.Hub.Infrastructure`. These are pre-existing and unrelated to
this slice. Verification was completed by building `src/Worms.Hub.Gateway`
directly (the project the slice modifies) which succeeded with 0 warnings and
0 errors under `--warnaserror`. `make cli.test.unit` and `npm run build` (in
`src/Worms.Hub.Web`) both passed.

### Web build required `npm install` first

`npm run build` in `src/Worms.Hub.Web` failed with missing-type errors for
`@testing-library/jest-dom`, `vitest/globals`, and `vitest/config` because
`node_modules` was not present in the worktree. Running `npm install` resolved
it. Not a deviation from the plan — just an environment setup detail.

## Files Added (not in plan)

None.
