# ELO Rankings — Delivery Plan

Companion to [`spec.md`](./spec.md). Each slice below is sized to ship as a single PR and described at a "what is delivered" level only — not how it is built.

## Slices

- [x] **Parser placement extraction** — Extend the `Worms.Armageddon.Files` replay model to include per-team finish position and `(machine, team name)` identity, inferring elimination order from the game log where it is not explicit; teams eliminated on the same turn share a tied position
- [x] **Placement persistence** — Database schema for storing ordered team placements per replay; Hub Worker extended to read placement data from the parsed replay model and persist it when a replay is processed
- [x] **Placement display** — CLI `get replays` output extended to show finish positions per team; Web UI replay detail page updated to display the placement table alongside existing replay data; Slack game-complete message updated to include the finishing order of teams
- [x] **Alias claiming — standalone page** — DB migration for players (keyed by Auth0 subject ID) and `(machine, team name)` aliases tables, repositories, GET endpoint listing all unclaimed pairs seen in processed replays, POST claim and DELETE unclaim endpoints (player record auto-created on first claim); standalone Web UI page listing all unclaimed aliases with claim and unclaim actions for authenticated users
- [x] **Alias claiming — replay detail inline** — Inline section on the existing replay detail page showing the `(machine, team name)` pairs for teams in that specific replay with claim and unclaim actions, reusing the API from the previous slice
- [x] **ELO rankings** — PlayerRank library integrated, DB migration for per-league ELO ratings, ratings computed from all placement data in a league and wired into the replay processing pipeline so standings update when a new replay is processed, league detail page extended with a standings table showing each player's name, current ELO, and games played
- [x] **ELO on alias changes** — ELO recalculation triggered when a player claims or unclaims an alias, so historical replays are accounted for when a player registers or removes an alias
- [x] **ELO delta on game detail** — Per-game ELO delta shown on each placement pill on the game/replay detail page (e.g. "+12 ELO"), deferred from the ELO rankings slice
- [x] **ELO leaderboard on league cards** — Top-3 ELO leaderboard preview shown on each league card on the leagues list page, deferred from the ELO rankings slice
- [x] **Slack post — league leaderboard with ELO changes** — Slack game-complete message extended to include the full league leaderboard after the game, showing each player's position change and ELO ranking change resulting from the game
- [x] **Remove feature flags** — Remove the feature flags introduced during this epic once the feature is fully deployed to production
