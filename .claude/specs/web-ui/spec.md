# Worms Hub Web UI

## Overview

A new web-based interface for the Worms Hub, giving league members a browser-accessible way to view past games, their replays, and the leagues they belong to — data that today is only reachable via the CLI or surfaced via Slack announcements. It is additive: the CLI and Slack flows remain unchanged. The first iteration is read-only and historical; later iterations may extend it to live/interactive concerns such as showing active games and joining them, and to aggregate views (standings, per-player stats) once player↔team mapping exists.

## Goals

- Give league members a browser-accessible list of leagues.
- Give league members a per-league page that shows the league's historic games/replays and the scheme used for that league.
- Establish a web UI surface that future, more interactive features (active games, join, league standings, per-player pages and stats, etc.) can be built on.

## Non-Goals

- Hosting, joining, or otherwise interacting with live/active games (deferred to a future epic).
- Replacing or deprecating the `worms` CLI or the Slack announcement flow.
- Administrative / moderation tooling.
- Mobile-native apps.
- Public, content-rich pages beyond a simple landing page (e.g. publicly browseable standings or replays).
- League standings of any kind — like per-player pages, this is blocked on a player↔team mapping that does not yet exist.
- Search, filter, or rich sorting on game lists — v1 shows raw lists only.
- Browseable schemes independent of a league (schemes only appear in v1 in the context of the league that uses them).
- Per-player pages of any kind (game history or stats). Deferred because reliably attributing games to players depends on a player↔team mapping that does not yet exist.
- Rich per-player statistics (e.g. best weapons, win percentage, head-to-head) — a follow-on epic, predicated on per-player pages existing.
- Mapping players to teams as a first-class concept — likely a separate epic in its own right.

## Major Capabilities

- A public landing page that is accessible without signing in.
- Sign-in for league members, using the same identity system the CLI authenticates against.
- Once signed in: see a list of leagues to navigate into.
- Once signed in: open a per-league page that lists the league's historic games / replays and shows the scheme used for that league.
- Once signed in: drill into an individual game's details (participants, winner, scheme, date, etc.).
- Extend the Hub Gateway JSON API where the data the UI needs is not already exposed in the right shape (e.g. league listing, per-league game lists).

## System Shape

A new browser-facing web UI delivered as a React single-page application. It is a standalone front-end that consumes the existing Hub Gateway JSON API — the same API the CLI talks to — and surfaces the data to league members in a browser. No new external integrations are introduced in this epic; the UI consumes the same Postgres-backed data the gateway already exposes.

The SPA is deployed as its own artifact alongside the existing Hub services, decoupled from the Gateway's release cycle. Local development brings it up alongside the existing services so the whole flow can be exercised end-to-end.

Authentication uses the same authority the CLI and Gateway already trust, via an interactive browser sign-in flow appropriate to a SPA (rather than the CLI's device flow). The landing page is reachable anonymously; every other page in the UI requires a signed-in league member, and authenticated calls to the Gateway carry a JWT bearer token in the same shape the Gateway already accepts.

## Core Domain Concepts

- **League** — a competitive grouping with a set of historic games and a scheme it uses.
- **Game** — a single completed match within a league, with participants, a winner, a scheme, and a date.
- **Replay** — the `.WAgame` artifact uploaded for a game.
- **Scheme** — the rule-set / configuration used for a league's games.
- **Player / League member** — the authenticated user viewing the UI.

## Constraints and Assumptions

- The UI is additive to existing Hub components; the Gateway, Worker, WA Runner, and CLI continue to operate unchanged.
- League members already have identities in the same authority used by the CLI; the UI is expected to reuse that identity system.
- Production hosting is Azure Container Apps (consistent with the rest of the Hub); local development runs under `docker compose` alongside the existing services.
- The UI is a React single-page application. This introduces a JavaScript/TypeScript toolchain to a previously .NET-only repository.
- The UI must run against the existing Gateway JSON API without changing the Gateway's auth contract (JWT bearer against the existing authority). The Gateway's API may be extended additively to serve the UI's read needs, but existing endpoints used by the CLI must remain compatible.
- Browser access to the Gateway requires CORS to be configured for the UI's origin(s) — the Gateway today only serves the CLI, which does not exercise CORS.
- A new OIDC client must be registered in the existing authority for the SPA's interactive browser sign-in flow, alongside the existing CLI device-flow client.

## Definition of Done

The smallest viable version is: a league member can open the web UI in a browser, land on the public landing page, sign in, see a list of leagues, open a league's page to see its historic games and scheme, and click into a past game to see its details. It is deployed alongside the existing Hub in production and runnable under `docker compose` for local development.

## Open Questions

- How the SPA's static assets are served in production (static hosting vs. a tiny container, behind the same domain or a sibling).
