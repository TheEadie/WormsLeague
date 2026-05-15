# Learnings: Placement Display

## Implementation Notes

### `PlacementInfo` record must be `sealed`

The plan declared `PlacementInfo` as a plain `record`, but the Roslynator/Roslyn analyser CA1852 ("Type can be sealed because it has no subtypes in its containing assembly and is not externally visible") treats `internal record` without `sealed` as a warning-as-error. The fix was to declare it `internal sealed record PlacementInfo(...)`, consistent with the `internal sealed record` pattern used throughout the codebase.

### Null-check pattern triggers RCS1146 in `SlackAnnouncer`

The plan's suggested null guard `if (placements is not null && placements.Count > 0)` was flagged by Roslynator RCS1146 ("Use conditional access"). The fix was to write `if (placements?.Count > 0)` instead, which is idiomatic C# and collapses the two checks into one expression that the analyser accepts.

### `GetReplays` controller method must become `async`

The existing `GetReplays` action was `public ActionResult<...> GetReplays(string id)` (synchronous). Adding `await featureFlags.IsPlacementsEnabledAsync()` inside it required changing the return type to `Task<ActionResult<...>>` and adding the `async` keyword, as described in the plan's code snippet. The same applied to `GetReplay`. This is expected but was not explicitly called out in the plan.

### Prettier reformats hand-written TSX indentation

The plan's TypeScript code snippets for `GameDetailPage.tsx` and `LeagueDetailPage.tsx` were pasted in with slightly different indentation from what Prettier requires (trailing commas, indent width on nested chains). Running `npx prettier --write` on both files after the edits corrected this. Always run Prettier before committing web changes.

## Files Added (not in plan)

None — all modified files were listed in the plan's "Files to Create / Modify" table.
