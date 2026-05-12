# Learnings: League List

## Implementation Notes

### LeaguesRepository must be `public`, not `internal sealed`

The plan instructs making `LeaguesRepository` `internal sealed`, following the pattern of `GamesRepository` and `ReplaysRepository`. However, those two repositories are consumed by the Gateway via the `public IRepository<T>` interface — the concrete `internal` type is never referenced from Gateway code. `LeaguesRepository` has no such interface and is injected directly as a concrete type into `LeaguesController` in the Gateway assembly. Because cross-assembly access requires `public` visibility, `LeaguesRepository` must be declared `public sealed`. This matches the visibility of `SchemeFiles`, which is also injected directly into Gateway controllers.

The plan's cross-referencing note ("review the existing code: `GamesRepository` is `internal sealed`...") was accurate about the existing repos but did not recognise that those repos are accessed via a public interface, not directly. The fix is straightforward: use `public sealed` instead of `internal sealed` for `LeaguesRepository`.

### Prettier left all files unchanged

Running `npx prettier --write src` before `make web.lint` left every file unchanged, confirming the authored formatting already matched Prettier's output. No manual adjustment was needed.

### Everything else proceeded exactly as planned

- Database migration, seed script, `LeagueListPage.tsx`, `CallbackPage.tsx` redirect, `App.tsx` routing update, and `AuthenticatedPage.tsx` deletion all matched the plan without deviation.
- `make web.build`, `make web.lint`, and both `dotnet build --warnaserror` targets passed after the repository visibility fix.
