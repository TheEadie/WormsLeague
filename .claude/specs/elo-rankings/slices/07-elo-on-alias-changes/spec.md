# ELO on Alias Changes

## Overview

When a player claims or unclaims a team alias, ELO ratings are synchronously recalculated for every league that contains a replay with that team's `(machine, teamName)` pair, so standings immediately reflect the updated alias ownership.

## Requirements

- When a player successfully claims a team alias, ELO ratings are recalculated for all leagues that have at least one replay containing that `(machine, teamName)` pair as a placement.
- When a player successfully unclaims a team alias, ELO ratings are recalculated for all leagues that have at least one replay containing that `(machine, teamName)` pair as a placement.
- Recalculation applies to all affected leagues, not just the most recent one.
- Recalculation is synchronous — it completes before the HTTP response is returned.
- Recalculation is gated behind the existing ELO ratings feature flag; no recalculation is attempted if the feature is not enabled.
- If ELO recalculation fails for a league, the error is logged and suppressed; the claim/unclaim operation still returns 200 and recalculation of any remaining affected leagues still proceeds.
- If the team appears in no replays (no affected leagues), the claim/unclaim completes normally with no recalculation.
- The existing conflict (409) and forbidden (403) responses for invalid claim/unclaim requests are unchanged.
- No changes are made to how replay processing triggers ELO recalculation.
- No changes are made to the startup backfiller.
- A new method `GetAffectedLeagueIds(string machine, string teamName)` is added to `IReplaysRepository` (and implemented in `ReplaysRepositoryV05`), returning the distinct league IDs of all processed replays that contain a placement with that `(machine, teamName)` pair.
- A new `CalculateForTeam(string machine, string teamName)` method is added to `RatingsCalculator`: it calls `GetAffectedLeagueIds` then iterates the results, calling `Calculate(leagueId)` for each, with per-league error handling (log and continue).
- `TeamsController` calls `ratingsCalculator.CalculateForTeam(team.Machine, team.TeamName)` after a successful claim or unclaim, guarded by `IsEloRatingsEnabledAsync()`.

## Out of Scope

- Web UI changes — the standings table already updates from the recalculated `player_ratings` table; no frontend work is required.
- Database schema changes — no new tables or columns are needed.
- Asynchronous or background recalculation.
- Locking or serialisation of concurrent claim/unclaim requests.
- Changes to the startup backfiller skip-if-populated guard.
- Changes to how replay processing triggers ELO recalculation.

## Acceptance Criteria

- Given the ELO feature is enabled and a player claims a team that appears in league L, when the `PUT /api/v1/teams` request returns 200, the `player_ratings` table for league L reflects ELO computed using that player's claimed alias across all historical replays in L.
- Given the ELO feature is enabled and a player unclaims a team that appears in league L, when the `PUT /api/v1/teams` request returns 200, the `player_ratings` table for league L no longer includes ELO contributions from that alias for that player.
- Given a team that appears in replays across leagues L1 and L2, when the alias is claimed or unclaimed, both L1 and L2 are recalculated before the response is returned.
- Given a team that appears in no replays (no affected leagues exist), the claim/unclaim returns 200 and no recalculation is attempted.
- Given the ELO ratings feature flag is not enabled, the claim/unclaim returns 200 and no recalculation is attempted.
- Given ELO recalculation for league L1 throws an exception when L1 and L2 are both affected, the error for L1 is logged, recalculation for L2 still runs, and the claim/unclaim still returns 200.
- Existing 409 Conflict and 403 Forbidden responses for invalid claim/unclaim operations are unaffected.

## Open Questions

None.
