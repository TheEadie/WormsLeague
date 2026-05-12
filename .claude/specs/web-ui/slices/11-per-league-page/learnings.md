# Learnings: Per-League Page

## Implementation Notes

### Promoting `ReplaysRepository` to `public` triggers CA1062 on `Create` and `Update`

The plan correctly noted that `ReplaysRepository` must be `public sealed` (not `internal sealed`) so `LeaguesController` in the Gateway assembly can inject it directly. However, making the class `public` also makes its interface methods (`Create`, `Update`) externally visible to Roslyn's CA1062 analyser, which requires null-guard checks on reference parameters. `GamesRepository` escapes this because it remains `internal`. The fix is straightforward: add `ArgumentNullException.ThrowIfNull(item)` at the top of `Create` and `Update`.

### `ReplayDb.Teams` must be `IReadOnlyList<string>?`, not `string[]?`

The plan specified `string[]?` for `ReplayDb.Teams` with a note that Npgsql maps PostgreSQL `text[]` natively to `string[]`. However, the `CA1819` analyser rule ("Properties should not return arrays") fires on array-typed record parameters that are exposed as public properties. Changing the type to `IReadOnlyList<string>?` resolves the warning; Dapper still receives `string[]` at the column-mapping boundary since the record constructor is called with the raw Npgsql value before the interface type is applied.

### Positional `new Replay(...)` call in `ReplaysController` needed updating

`ReplaysController.Post` creates a new `Replay` using the positional constructor: `new Replay("0", ..., null)`. Adding four new nullable parameters to the record expanded the constructor to nine arguments, so this call had to be updated to pass four additional `null` arguments. The plan mentioned checking for positional callers but did not list `ReplaysController` specifically — it is the one caller that needed updating.

### Prettier reformatted `LeagueDetailPage.tsx`

Running `npx prettier --write src` reformatted `LeagueDetailPage.tsx` (collapsing some multi-line JSX into fewer lines). The authored formatting was close but not identical to Prettier's output. The fix was trivial: run `npx prettier --write src/pages/LeagueDetailPage.tsx` and re-run `make web.lint` to confirm.

### `V0.3.1__SeedRedgateLeague.sql` was a prerequisite for the V0.4 FK backfill

The `V0.3.1__SeedRedgateLeague.sql` migration (inserting `('redgate', 'Redgate')` into `leagues`) was added before this slice began, as a follow-up to the league-list slice which explicitly deferred production seeding. This migration is a logical prerequisite: `V0.4.1__BackfillReplayLeagueFields.sql` sets `league_id = 'redgate'` on all existing `replays` rows, and the FK constraint added in `V0.4__AddReplayLeagueFields.sql` would reject that value if `redgate` did not already exist in `leagues`. When planning future slices that add FK columns and backfill them, seed the referenced table first.

### `replayModel` is parsed before `Update` in the final Processor

The plan showed `Update` before `GetModel`, then overrode by instructing to parse the replay log first and then set the `with` expression. The implementation moved `GetModel` before the `with` expression and `Update` call (parsing then updating), which is the only logical order. This is a minor sequencing deviation from the literal code snippet in the plan.
