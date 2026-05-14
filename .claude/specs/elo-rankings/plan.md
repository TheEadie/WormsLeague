# ELO Rankings — Delivery Plan

Companion to [`spec.md`](./spec.md). Each slice below is sized to ship as a single PR and described at a "what is delivered" level only — not how it is built.

## Slices

- [ ] **Parser placement extraction** — Extend the `Worms.Armageddon.Files` replay model to include per-team finish position and `(machine, team name)` identity, inferring elimination order from the game log where it is not explicit; teams eliminated on the same turn share a tied position
- [ ] **Placement persistence** — Database schema for storing ordered team placements per replay; Hub Worker extended to read placement data from the parsed replay model and persist it when a replay is processed
- [ ] **Alias schema and unclaimed teams API** — Database tables for players (keyed by Auth0 subject ID) and `(machine, team name)` aliases; Gateway endpoint listing unclaimed pairs seen in processed replays
- [ ] **Alias claiming UI and API** — Gateway endpoints for authenticated players to claim and unclaim a `(machine, team name)` alias (player record auto-created on first claim); Web UI surfaces unclaimed aliases on a standalone page and inline on each replay page scoped to the teams in that replay, with claim and unclaim actions on both surfaces
- [ ] **ELO calculation** — PlayerRank library integrated; database schema for per-league ELO ratings; ratings computed from all placement data in a league and stored
- [ ] **ELO on replay processing** — ELO recalculation wired into the replay processing pipeline so ratings are updated for all players with an alias in the game after each new replay is processed
- [ ] **ELO on alias changes** — ELO recalculation wired to alias claim and unclaim events, so historical replays are accounted for when a player registers or removes an alias
- [ ] **League rankings UI** — League detail view extended with a standings table showing each player's name, current ELO, and games played within that league
