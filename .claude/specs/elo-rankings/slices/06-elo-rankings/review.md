# Review — ELO Rankings

## Verdict

The implementation satisfies every acceptance criterion in the spec. The build passes clean with `--warnaserror`, and `make web.lint` passes with no errors. There are no blockers. One suggestion and zero nitpicks are raised below.

---

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| `PlayerRank` appears in `Worms.Hub.Gateway.csproj` and the project builds with `--warnaserror` | MET | `Worms.Hub.Gateway.csproj`: `<PackageReference Include="PlayerRank" Version="5.0.38"/>`. `dotnet build --warnaserror` exits 0, 0 warnings. |
| DB migration adds a `player_ratings` table | MET | `src/database/migrations/V0.8__AddPlayerRatings.sql:1–7`: table created with FK to `players`, PK on `(player_auth_subject, league_id)`. |
| `GET /api/v1/leagues` and `GET /api/v1/leagues/{id}` return `"standings": null` before migration | MET | `LeaguesController.cs:17–24` and `53–61`: `standings` initialised as `null` and only set when `eloEnabled` is true. |
| After migration, `GET /api/v1/leagues/{id}` returns a `standings` array with `playerName`, `elo`, `gamesPlayed`, ordered by ELO descending | MET | `RatingsRepository.cs:21`: `ORDER BY pr.rating DESC`. `LeaguesController.cs:55–60`: selects `DisplayName`, `Rating`, `GamesPlayed` into `StandingDto`. |
| No rated players → `standings` is empty array (not null) | MET | `LeaguesController.cs:54–60`: when `eloEnabled` is true, code always sets `standings` to a (possibly empty) list from `GetByLeagueId`. |
| Starting ELO of 1000 before first delta | MET | `RatingsCalculator.cs:63`: `new EloScoringStrategy(new Points(64), new Points(400), new Points(1000))`. Players with no ELO-eligible games also receive 1000 at line 70. |
| Partial aliases: unclaimed team excluded from ELO; claiming player's `gamesPlayed` still incremented | MET | `RatingsCalculator.cs:34–51`: only claimed placements enter `matchedPlayers`; `gamesPlayed` incremented at lines 43–45 regardless of match count. |
| Single-player replay: excluded from ELO, counted in `gamesPlayed` | MET | `RatingsCalculator.cs:49–52`: `if (matchedPlayers.Count < 2) continue` skips ELO but `gamesPlayed` is already incremented above. |
| ELO recalculated after new replay processed | MET | `Processor.cs:98–108`: `ratingsCalculator.Calculate(updatedReplay.LeagueId)` called after replay is marked Processed. |
| ELO calc failure does not block processing; replay marked Processed; queue message deleted; error logged | MET | `Processor.cs:98–108`: `try/catch` logs via `LogError`; execution continues to `AnnounceGameComplete` at line 120 and `DeleteMessage` at line 123. |
| League with no aliased teams → `standings` is empty array | MET | `RatingsCalculator.cs:67–74`: `gamesPlayed` will be empty; `ReplaceForLeague` called with empty list; controller returns empty list. |
| Standings section shown when at least one entry present | MET | `LeagueDetailPage.tsx:168`: `{league.standings !== null && league.standings.length > 0 && (…)}` |
| Standings section omitted when `standings` is `null` or empty | MET | Same guard as above covers both cases. |
| Columns: Rank, Player Name, ELO, Games Played | MET | `LeagueDetailPage.tsx:177–195`: all four column headers present. |
| `make web.lint` passes | MET | Confirmed — 0 ESLint errors, 0 TypeScript errors, 0 Prettier formatting errors. |

---

## Scope

All files in the diff match the plan's "Files to Create / Modify" table exactly.

**New files (all present):** `V0.8__AddPlayerRatings.sql`, `PlayerRating.cs`, `IRatingsRepository.cs`, `RatingsRepository.cs`, `RatingsCalculator.cs`, `StandingDto.cs`.

**Modified files (all present):** `Worms.Hub.Gateway.csproj`, `ServiceRegistration.cs` (Storage), `IFeatureFlags.cs`, `FeatureFlags.cs`, `LeagueDto.cs`, `LeaguesController.cs`, `ServiceRegistration.cs` (Gateway), `Processor.cs`, `LeagueDetailPage.tsx`.

No files outside the plan appear in the diff. The learnings file documents one deviation: `new PlayerRank.League()` and `new PlayerRank.Game()` are fully qualified at `RatingsCalculator.cs:27,54` rather than relying on `using PlayerRank;`, to resolve the CS0104 ambiguity with `Worms.Hub.Storage.Domain.League` and `Game`. This is explained in `learnings.md` and is a correct fix.

---

## Blockers

None.

---

## Suggestions

#### S1 — React table key uses display name rather than a stable unique identifier

- **File:** `src/Worms.Hub.Web/src/pages/LeagueDetailPage.tsx:199`
- **Issue:** `<TableRow key={s.playerName}>` uses the player's display name as the React reconciliation key. Display names are not guaranteed unique — two players could share a display name, producing duplicate keys and a React reconciliation warning in the browser console.
- **Fix:** Use the array `index` as the key (`key={index}`). The standings list is not user-reordered and is stable between renders for a given API response, so index-as-key is acceptable. Alternatively, include a stable `id` or `authSubject` field in `StandingDto` and use that.
- **Decision:** Accept

---

## Nitpicks

None.

---

## Tests

No new test files were added. This is consistent with the testing strategy: the Hub Gateway, Hub Storage, and Worker layers have no dedicated unit-test projects, and the testing strategy explicitly defers behaviour at those layers to integration tests.

The `RatingsCalculator` contains non-trivial business logic (alias matching, games-played counting, single-player replay exclusion, ELO delegation to PlayerRank). The testing strategy notes "When adding meaningful logic at those layers, prefer adding a new `<Project>.Tests`." No `Worms.Hub.Gateway.Tests` project exists today and the spec does not require one, so this is pre-existing debt rather than a regression from this slice.

The Web UI standings table adds no Vitest tests. The spec requires only that `make web.lint` passes; no test is mandated for a read-only display component, consistent with the testing strategy's rule that only security/auth-critical components require mandatory tests.

---

## Recommended Actions

- **S1** (React key on `playerName`) — **Accept** — Switching to `key={index}` is a one-line fix that eliminates a potential React warning at zero cost. The standings list is stable (server-ordered by ELO, not reordered by the user), so index-as-key is safe here.
