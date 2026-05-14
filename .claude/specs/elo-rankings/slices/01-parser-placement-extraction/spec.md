# Parser Placement Extraction

## Overview

Extend `ReplayResource` in `Worms.Armageddon.Files` to include a per-team finish order derived from worm kill events in the replay log. This gives downstream consumers the placement data needed to persist match results for ELO calculation.

## Requirements

- `ReplayResource` exposes a `Placements` collection where each entry associates a `Team` with an integer finish position.
- Position 1 is the best (winner/survivor); higher integers indicate earlier elimination.
- Positions are determined by tracking cumulative worm kills per team across the game. A team is eliminated in the game turn during which their accumulated kill count reaches the total worm count.
- The total worm count per team is inferred from the replay: it equals the maximum total kills accumulated by any single team across the whole game. All teams are assumed to start with the same number of worms.
- Teams eliminated during the same game turn share the same tied position. A "game turn" is the unit bounded by consecutive turn-start markers in the log; all damage events between two turn-start markers belong to the same turn.
- Position is defined as the number of teams that finished ahead of a given team plus one. Teams that finished ahead are those eliminated in a later turn (or who survived). Tied teams share the same position number; subsequent positions skip accordingly (e.g. two teams tied at position 2 means the next position is 4).
- For a draw (`Winner = "Draw"`): all teams whose final elimination turn is the same last turn share the tied first position. Teams eliminated in earlier turns receive higher position numbers calculated by the same rule.
- If the log has no winner line (abandoned game, truncated log), `Placements` is an empty collection.
- Kills recorded against a team after their accumulated total has already reached the worm count are ignored.
- Each `Placement` entry exposes both the team name and machine (i.e. the full `(machine, team name)` pair), consistent with the existing `Team` model.

## Out of Scope

- Persisting placements to the database — handled in the next slice (Placement Persistence).
- Determining worm count from the scheme file — deferred; per-game inference is sufficient for now.
- Any change to how draws are detected — the existing `Winner = "Draw"` field is used as-is.
- Any changes outside `Worms.Armageddon.Files` and its test project.

## Acceptance Criteria

- Given a completed replay with three teams eliminated in different turns, `Placements` contains one entry per team: the surviving team at position 1, and the remaining teams at positions 2 and 3 in reverse elimination order.
- Given a replay where two teams are eliminated in the same game turn, both entries in `Placements` share the same position value; the surviving team is at position 1.
- Given a drawn game (`Winner = "Draw"`) where all three teams are eliminated in the same final turn, all three entries in `Placements` share position 1.
- Given a drawn game where one team was eliminated before the final turn, that team receives a higher position number; the two teams eliminated in the final turn are tied at the lower position number.
- Given a log with no winner line, `Placements` is an empty collection.
- Given a log where kill events for a team accumulate beyond the inferred worm count, the excess kills have no effect on any team's recorded position.
- All pre-existing `ReplayTextReaderShould` tests pass without modification.
- New unit tests covering the cases above are added to `Worms.Armageddon.Files.Tests`, following the NUnit + Shouldly conventions used in `ReplayTextReaderShould`.

## Open Questions

None.
