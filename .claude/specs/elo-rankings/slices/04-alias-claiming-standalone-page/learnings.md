# Learnings: Alias Claiming — Standalone Page

## Implementation Notes

### CA1062 fires on public repository methods with record parameters

The plan specified `PlayersRepository` as `public sealed` (required because it is injected by interface from the Gateway assembly). The `Create(Player player)` method is therefore a public method on a public class, which triggers Roslyn CA1062 ("Validate parameter is non-null before using it"). The plan did not mention this. The fix was to add `ArgumentNullException.ThrowIfNull(player);` at the top of `Create`, consistent with the pattern used in `ReplaysRepository.Create` and `ReplaysRepository.Update`. `TeamsRepository` does not take record parameters in its public methods, so it did not trigger this analyser rule.

### `useCallback` + optional-chain dependency rejected by the React Compiler ESLint rule

The plan's code snippet for `TeamsPage.tsx` used `useCallback` with `[auth.user?.access_token]` as the dependency array and then called the callback from a `useEffect`. This triggered two ESLint errors from the React Compiler plugin:

1. `react-compiler/react-compiler` — "Compilation Skipped: Existing memoization could not be preserved" because the React Compiler inferred `auth.user` as the dependency but the source specified `auth.user?.access_token` (more specific optional chain).
2. `react-hooks/set-state-in-effect` — "Calling setState synchronously within an effect can trigger cascading renders."

The fix was to align with the pattern used in `GameDetailPage.tsx` and `LeagueListPage.tsx`:
- Remove `useCallback` entirely.
- Put the fetch logic directly inside `useEffect` using a `.then()` chain (not `async/await` in the effect body).
- For the post-mutation re-fetch, use a `refetchKey` counter state (`const [refetchKey, setRefetchKey] = useState(0)`) as an additional dependency on the effect. After a successful claim/unclaim, increment `refetchKey` with `setRefetchKey((k) => k + 1)` instead of calling `loadTeams()` directly.

This is a pattern the codebase had not previously needed (other pages have no mutation buttons) but fits naturally within the existing conventions.

## Files Added (not in plan)

None — all created and modified files were listed in the plan's "Files to Create / Modify" table.
