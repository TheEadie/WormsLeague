# Slack Post — League Leaderboard with ELO Changes

## Overview

Extend the "Mayhem complete" Slack message to include the full league leaderboard after each game is processed, showing each player's current ELO rating, rank position change, and ELO delta from this game.

## Requirements

- After a game is processed and ELO ratings are calculated, the Slack "Mayhem complete" message includes a leaderboard section as a new block beneath the results.
- The leaderboard lists every player with a rating in the league, ordered by rank (1st to last).
- The leaderboard section is rendered as a code block so that fixed-width spacing is preserved and ELO values align.
- Each row shows: rank number, ELO rating (right-aligned so values line up), and player display name.
- If a player's ELO changed in this game (i.e. their ELO delta is non-zero), the delta is shown after their name in the form `(+N)` or `(-N)` where N is always a whole number.
- If a player's rank position changed compared to before this game, an arrow and magnitude are shown after the delta (if any): `⇧N` for improvement, `⇩N` for decline. The arrow appears for any player whose rank shifted, including players who did not participate but were displaced by others.
- Players with equal ELO share the same rank number in the leaderboard.
- Players with no ELO change and no rank position change have no additional indicators beyond rank, ELO, and name.
- If the leaderboard cannot be produced — because ELO calculation failed or ratings could not be fetched — the Slack message still posts, but includes a failure note in place of the leaderboard section.
- If no players have ratings in this league (empty leaderboard), the leaderboard section is omitted entirely with no failure note.
- If the ELO feature flag is off, the Slack message is identical to its current form: no leaderboard section, no failure note.
- If the replay has no league ID, the Slack message is identical to its current form.

## Out of Scope

- Emoji badges or Slack channel tags on player rows (`:star:`, `#professional`) — not needed in this version.
- Sending the leaderboard as a separate Slack message rather than as a block in the existing message.
- Rank position change on the very first game of a league — `PositionChange` will naturally be zero for all players when no prior ranking exists; no special handling is required.

## Acceptance Criteria

- Given ELO is enabled and a processed replay belongs to a league with rated players: when the Slack message is posted, it contains a leaderboard block showing all rated players in rank order, each with their rank number, ELO rating, and display name.
- Given a player participated in the game and their ELO changed: their row includes the delta in the form `(+N)` or `(-N)` where N is a whole number.
- Given a player participated in the game but their ELO did not change (e.g. solo game): no delta is shown on their row.
- Given two players have equal ELO: they share the same rank number in the leaderboard.
- Given any player's rank position changed since before this game (participant or not): their row shows `⇧N` or `⇩N` with the number of places moved.
- Given a player's rank position did not change: no arrow appears on their row.
- Given ELO calculation fails or ratings cannot be fetched: the Slack message posts successfully and contains a failure note where the leaderboard section would be.
- Given no players have ratings in this league: the Slack message posts without a leaderboard section and without a failure note.
- Given the ELO feature flag is off: the Slack message is identical to the current format.
- Given the replay has no league ID: the Slack message is identical to the current format.
- Given the example mockup below, the posted message matches its structure:

```
Mayhem complete

Results:
1: Player A
2: Player B
...
```
```
Leaderboard:
1:  1033 Player A (+7)
2:  1021 Player B (+33) ⇧3
3:  1004 Player C ⇩1
4:  1001 Player D ⇩1
5:   989 Player E (-11) ⇩1
6:   952 Player F (-29)
```

## Open Questions

None.
