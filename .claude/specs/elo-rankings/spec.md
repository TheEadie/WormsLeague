# ELO Rankings

## Overview

Bring back the ELO ranking system that previously lived in the now-defunct SC Bot Slack integration. Players authenticated with the Hub are linked to the team names they use in replays, enabling full per-player game standings to be computed and an ELO rating to be maintained and displayed per league.

## Goals

- Allow players to associate their Hub account with the team name(s) they use in Worms Armageddon replays
- Determine the complete finish order of all teams in each processed replay, including tied positions for same-turn eliminations
- Calculate ELO ratings for each player using the PlayerRank library, supporting games with more than two players
- Surface player rankings on the Web UI, scoped per league

## Non-Goals

- Migrating or importing historical data from SC Bot
- Ranking modes other than ELO (e.g. Glicko, TrueSkill)
- Global cross-league rankings
- Automated team-name inference or de-duplication (players manage their own aliases)

## Major Capabilities

- **Game placement extraction** — the `Worms.Armageddon.Files` replay parser is extended to include finish order on the parsed replay model; elimination order is inferred from the game log where it is not explicit; teams eliminated on the same turn share a tied position; this data is then persisted by the Hub Worker when a replay is processed, with team names with no alias owner stored as unclaimed
- **Unclaimed team browsing** — the Web UI shows (machine, team name) pairs seen in replays that have not yet been claimed by any player, so players know which identities are available to claim
- **Team alias claiming and unclaiming** — an authenticated player can claim one or more unclaimed (machine, team name) pairs as aliases for their account; claiming triggers full ELO recalculation across all historical replays in each affected league (the backfill mechanism); players can also unclaim an alias, which triggers the inverse recalculation
- **ELO rating calculation** — ELO ratings are (re)calculated for all players with a registered alias using the PlayerRank library; triggered both by a new replay being processed and by a player claiming a team name
- **Rankings display** — the Web UI shows a live league standings table (player name, current ELO, games played) for each league

## System Shape

- **Hub Gateway API** — endpoints for listing unclaimed team names and claiming a name; the Auth0 subject ID from the access token is the stable player identifier stored in the database
- **Hub Worker (replay processing pipeline)** — extended to read finish order from the parsed replay model and persist it, then trigger ELO recalculation for affected players; recalculation is also triggered when a player claims or unclaims an alias
- **Hub Storage (Postgres)** — new tables for players, team aliases, and per-league ELO ratings; existing replays table extended to store ordered placement data per team
- **PlayerRank library** — .NET library (github.com/TheEadie/PlayerRank) called to compute ELO ratings across all historical replays for a player; supports N-player games and tied positions
- **Web UI** — unclaimed teams list (so players can see what to claim) and a league rankings table (player name, ELO, games played) on the league detail view

## Core Domain Concepts

- **Player** — a Hub-authenticated user, identified by their Auth0 subject ID, with a display name
- **Alias** — a `(machine, team name)` pair extracted from a replay that a player has claimed as one of their identities; the combination is the unique key, so the same team name on a different machine is a distinct alias
- **Placement** — the finish position of a team within a single game; positions are integers starting at 1; tied positions are possible
- **Rating** — a player's current ELO score within a specific league, updated after each game they appear in
- **League** — existing concept; the scope within which ratings are calculated and displayed

## Constraints and Assumptions

- Auth0 subject ID (from the access token) is the stable, unique identifier for players — no new auth infrastructure is required
- The existing replay log (already stored as `fullLog` in the replays table) contains sufficient turn-by-turn information to reconstruct finish order; same-turn deaths produce tied positions
- The PlayerRank library is owned by the same author as this repo and can be taken as a dependency
- Alias uniqueness is based on `(machine, team name)` — this covers the common case where players play from different machines; edge cases (all teams on one machine, multiple players sharing a machine) are known limitations and may still result in ambiguous aliases that require manual unclaiming to resolve
- ELO ratings are recalculated per league, not globally
- Replays without a complete set of linked aliases still have their placement stored; ELO is only updated for players with a registered alias
- Claiming or unclaiming an alias triggers full ELO recalculation for the affected player — no separate backfill step is needed

## Definition of Done

- When a replay is processed, finish positions are stored for all teams; team names with no owner are stored as unclaimed
- A player can browse unclaimed `(machine, team name)` pairs in the Web UI and claim or unclaim them against their account
- Claiming or unclaiming an alias triggers ELO recalculation across all historical replays in the affected league(s)
- After processing a new replay, ELO ratings are updated for all players whose alias appears in the game
- The Web UI displays a league standings table showing each player's current ELO and games played
- The end-to-end flow works for a real league replay with two or more players, including a backfill scenario
