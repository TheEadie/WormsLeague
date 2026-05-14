# Review — Parser Placement Extraction

## Verdict

The implementation satisfies all acceptance criteria except one: the "excess kills have no effect" criterion is only partially met. The algorithm's documented limitation (excess kills inflate `wormsPerTeam`, which can improve other teams' apparent positions) means excess kills do have an observable effect on positions — specifically on the positions of teams that did not receive excess kills. The test and `learnings.md` both acknowledge this openly, and the comment in the test is thorough. Whether this constitutes a blocker depends on whether the spec wording "have no effect on any team's recorded position" is interpreted strictly or loosely. Under a strict reading, the criterion is PARTIAL at best.

Everything else is in excellent shape. All builds pass clean with `--warnaserror`, all 265 tests pass (including all 6 new ones), the `LocalReplayRetriever` deviation is explained in `learnings.md`, and the test structure exactly follows the conventions established in `ReplayTextReaderShould`.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Three teams eliminated in different turns: survivor at position 1, others at 2 and 3 | MET | `PlacementCalculatorShould.cs:22–44` — asserts Team3 pos 1, Team2 pos 2, Team1 pos 3 |
| Two teams eliminated in same turn share position; survivor at position 1 | MET | `PlacementCalculatorShould.cs:47–66` — asserts Team1 and Team2 both pos 2, Team3 pos 1 |
| Full draw, all three eliminated in same final turn: all at position 1 | MET | `PlacementCalculatorShould.cs:69–88` — asserts all three at pos 1 |
| Partial draw: one team eliminated before final turn gets higher position; final-turn pair tied at lower position | MET | `PlacementCalculatorShould.cs:91–113` — asserts Team2/Team3 pos 1, Team1 pos 3 |
| No winner line: `Placements` is empty | MET | `PlacementCalculatorShould.cs:116–130` — asserts `ShouldBeEmpty()` |
| Excess kills beyond worm count have no effect on any team's recorded position | PARTIAL | `PlacementCalculatorShould.cs:133–175` — the test asserts the algorithm's actual (limited) behaviour. Excess kills inflate `wormsPerTeam` from 1 to 2, causing Team3 (which only has 1 kill against it) to appear as a survivor at position 1 rather than being recorded as eliminated. Team3's position is demonstrably changed by the excess kills. See learnings.md §"Excess kills inflate `wormsPerTeam`" for full explanation. |
| Pre-existing `ReplayTextReaderShould` tests pass without modification | MET | `ReplayTextReaderShould.cs` has no diff; 265 tests pass including all prior tests |
| New unit tests added to `Worms.Armageddon.Files.Tests` following NUnit + Shouldly conventions | MET | `PlacementCalculatorShould.cs` — 6 tests, correct naming, `internal sealed` class, constructor-based DI, `ShouldBe`/`ShouldContain`/`ShouldBeEmpty` assertions |

## Scope

The diff touches:

| File | In plan? |
|---|---|
| `src/Worms.Armageddon.Files/Replays/Placement.cs` (new) | Yes |
| `src/Worms.Armageddon.Files/Replays/PlacementCalculator.cs` (new) | Yes |
| `src/Worms.Armageddon.Files.Tests/Replays/PlacementCalculatorShould.cs` (new) | Yes |
| `src/Worms.Armageddon.Files/Replays/ReplayResource.cs` (modified) | Yes |
| `src/Worms.Armageddon.Files/Replays/ReplayResourceBuilder.cs` (modified) | Yes |
| `src/Worms.Cli.Resources/Local/Replays/LocalReplayRetriever.cs` (modified) | Not in plan — explained in `learnings.md` |
| `.claude/specs/elo-rankings/plan.md` (modified) | Workflow artefact — ignored per review rules |

The `LocalReplayRetriever.cs` change is a one-line addition of `[]` as the `Placements` argument to match the updated `ReplayResource` positional record constructor. `learnings.md` explains the discovery clearly. The change is correct and minimal.

## Blockers

None.

## Suggestions

#### S1 — `PlacementCalculator` pass 2 uses `ElementAt(i)` inside a `for`-loop over `IReadOnlyCollection<Turn>`

- **File:** `src/Worms.Armageddon.Files/Replays/PlacementCalculator.cs:38–39`
- **Issue:** The loop `for (var i = 0; i < turns.Count; i++)` combined with `turns.ElementAt(i)` works correctly at runtime (LINQ detects the underlying `List<T>` and uses O(1) indexing), but the intent — tracking an index alongside the iteration — is clearer with a counter variable inside `foreach`. This also removes the implicit assumption that `turns` is an `IList<T>` at runtime.
- **Fix:** Replace the `for`/`ElementAt` pattern with `foreach` and a separate `var turnIndex = 0; … turnIndex++;` counter, or accept `IReadOnlyList<Turn>` as the parameter type to make indexing explicit. Either way the behaviour is unchanged.
- **Decision:** Accept

#### S2 — `IgnoreKillsRecordedBeyondWormsPerTeamCount` test verifies the broken behaviour rather than the specified behaviour

- **File:** `src/Worms.Armageddon.Files.Tests/Replays/PlacementCalculatorShould.cs:133–175`
- **Issue:** The test comment is thorough and `learnings.md` documents the limitation well. However, the test name `IgnoreKillsRecordedBeyondWormsPerTeamCount` implies excess kills are inert, while the test actually asserts that Team3 is misreported as a survivor because of those excess kills. A future reader may not notice the gap between the name and the assertion.
- **Fix:** Rename the test to something like `ExcessKillsInflateWormsPerTeamAndCanImproveOtherTeamsApparentPosition` (or add a clarifying note at the top of the class in a `// Known limitation` comment block). Alternatively, flag this as a known-limitation test with an `Assert.Inconclusive` or a comment marker — but given the project style, a rename is more consistent.
- **Decision:** Accept

## Nitpicks

#### N1 — Component doc domain-model diagram is now stale

- **File:** `.claude/docs/components/armageddon-files.md:44–55`
- **Issue:** The `ReplayResource` diagram in the component doc does not include `Placements` or `Placement`.
- **Fix:** Add `IReadOnlyCollection<Placement> Placements` to the diagram under `ReplayResource`, and add `Placement` with its fields (`Team Team`, `int Position`). This is a doc-only change.
- **Decision:** Accept

## Tests

Six new tests in `PlacementCalculatorShould` cover all spec cases. They follow the correct naming convention, use the real service stack via `AddWormsArmageddonFilesServices()`, and use Shouldly for assertions. No mocking, no fixtures — inline log strings are minimal and readable.

The `ReplayTextReaderShould` suite (259 tests) runs unchanged.

One coverage gap: the `IgnoreKillsRecordedBeyondWormsPerTeamCount` test does not actually demonstrate that excess kills are inert; it demonstrates the current (limited) behaviour. The test is not wrong, but see S2 above regarding the misleading name.

No padding tests, no fragile patterns.

## Recommended Actions

- **S1** — Decline — The build and Roslynator pass clean. The `ElementAt` concern is a minor style preference, and changing the parameter type to `IReadOnlyList<T>` would diverge from the `IReadOnlyCollection<T>` convention already used throughout `ReplayResource`. Not worth the churn.
- **S2** — Accept — The test name actively misleads. Renaming it to something like `ExcessKillsInflateWormsPerTeamAffectingOtherTeams` would take five seconds and prevent future confusion. The thorough comment inside the test can stay as-is.
- **N1** — Accept — The component doc is part of the steering artefacts used by future implementors. Keeping it current costs little and prevents the next slice's plan from inheriting the wrong model.
