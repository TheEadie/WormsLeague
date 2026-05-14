# Placement Display

## Overview

Surface the finish-order data (persisted in slice 02) across three outputs: the CLI `get replays` command, the Web UI replay pages, and the Slack game-complete announcement. Each surface degrades gracefully to its current behaviour when placement data is absent.

## Requirements

### API

- `ReplayDetailDto` is extended with a nullable `Placements` field containing an ordered list of entries, each carrying `Machine`, `TeamName`, and `Position` (integer, 1-based, repeated for ties).
- `IAnnouncer.AnnounceGameComplete` is extended to accept an optional placements list alongside the existing winner string. When placements are provided, the winner string is ignored and the placements list is used instead; the winner string is only used when placements are `null`.
- When `IFeatureFlags.IsPlacementsEnabled` returns false, placements are treated as `null` across all surfaces (API endpoints and Slack announcement); each caller handles `null` independently.

### CLI — `get replays` list view

- When placements are available, the TEAMS column is updated to show entries in position order in the format `<position>: <name>`, separated by commas; tied positions repeat the same number (e.g. `1: TeamA, 2: TeamB, 2: TeamC`). The WINNER column is removed.
- When placements are `null`, the command falls back to the current behaviour: plain team names with no position prefix, WINNER column shown.

### CLI — single-replay detail view

- When placements are available, the "Winner" line in the Awards section is replaced with a placement list in `<position>: <name>` format, one entry per line.
- When placements are `null`, the command falls back to the current behaviour: the "Winner: X" line is shown.

### Web UI — game detail page (`GameDetailPage`)

- When placements are available, the team chips in the hero card are re-ordered by position and each chip label is prefixed with `<position>: ` (e.g. `1: TeamA`); tied positions repeat the same number (e.g. `2: TeamB`, `2: TeamC`).
- When placements are `null`, the hero card chips fall back to the current behaviour: plain team name chips with no position prefix or reordering.

### Web UI — league replay list (`LeagueDetailPage`)

- When placements are available, the teams column chips for a processed replay are re-ordered by position and each chip label is prefixed with `<position>: `; tied positions repeat the same number.
- When placements are `null`, the teams column falls back to the current behaviour (winner-first sort, plain team name chips).

### Slack — game-complete announcement

- When placements are available, the game-complete Slack message replaces the winner line with a `Results:` header (in the same style as the current `Winner:` header) followed by the finishing order as a multi-line plain-text list in the format `<position>: <name>`, one team per line, in position order; tied positions repeat the same number.
- When placements are `null`, the message falls back to the current behaviour: winner name only.

## Out of Scope

- Any change to `ReplayDto`, which is the POST response for the replay upload endpoint and is not used for any GET operation.
- A dedicated placement panel or page in the Web UI.
- CLI commands other than `get replays`.
- Ordering or display changes in the LeagueDetailPage for replays that are not yet processed.

## Acceptance Criteria

**API**

- Given a replay stored with placements and schema >= v0.5, `GET /api/v1/leagues/{id}/replays` and `GET /api/v1/leagues/{id}/replays/{replayId}` both include a non-null `placements` array.
- Given schema < v0.5, both endpoints return `placements: null`.

**CLI — list view**

- Given a processed replay with placements `[{TeamA, 1}, {TeamB, 2}, {TeamC, 3}]`, the TEAMS column shows `1: TeamA, 2: TeamB, 3: TeamC` and no WINNER column is present.
- Given a processed replay with tied placements `[{TeamA, 1}, {TeamB, 2}, {TeamC, 2}]`, the TEAMS column shows `1: TeamA, 2: TeamB, 2: TeamC`.
- Given a replay with `null` placements, the TEAMS column shows plain team names and the WINNER column is present.

**CLI — single-replay view**

- Given a processed replay with placements, the output contains a placement list (one `<position>: <name>` entry per line) and no "Winner:" line.
- Given a processed replay with `null` placements, the output contains a "Winner: X" line and no placement list.

**Web UI — game detail page**

- Given a processed replay with placements, the hero card chips appear in position order with a `<position>: ` prefix on each label.
- Given a processed replay with tied placements `[{TeamA, 1}, {TeamB, 2}, {TeamC, 2}]`, the hero card chips show `1: TeamA`, `2: TeamB`, `2: TeamC`.
- Given a processed replay with `null` placements, the hero card chips appear as plain team name chips with no prefix and no reordering.

**Web UI — league replay list**

- Given a processed replay with placements, the teams column chips appear in position order with a `<position>: ` prefix.
- Given a processed replay with tied placements `[{TeamA, 1}, {TeamB, 2}, {TeamC, 2}]`, the teams column chips show `1: TeamA`, `2: TeamB`, `2: TeamC`.
- Given a processed replay with `null` placements, the teams column falls back to winner-first ordering with plain chip labels.

**Slack**

- Given a replay processed with placements, the game-complete Slack message shows a `Results:` header followed by one `<position>: <name>` entry per line.
- Given a replay processed with tied placements `[{TeamA, 1}, {TeamB, 2}, {TeamC, 2}]`, the Slack message lists `2: TeamB` and `2: TeamC` on separate lines with the same position number.
- Given a replay processed with `null` placements, the Slack message contains only the winner (current behaviour).

## Open Questions

None.
