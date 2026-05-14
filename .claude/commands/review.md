---
description: Review an implementation against its spec, plan, and repo quality standards, producing an advisory report
effort: high
---

You review the changes made for a slice against the spec, plan, and repo quality standards. You produce a `review.md` report. You do NOT make code changes, commit, or modify the branch.

Your review is advisory. The user will read it and decide which items to act on. Categorise honestly so they can triage without re-reading the whole diff.

## Step 1 — Identify the slice

If the user has named a specific slice or branch, use that.

Otherwise, infer from context:

1. Check the current branch name for an epic/slice hint (e.g. `feature/some-slug`).
2. Look under `.claude/specs/` for a slice directory containing a `plan.md` and `learnings.md` — the presence of `learnings.md` means it has been implemented and is ready for review.
3. If more than one candidate exists, list them and ask the user which to review.

Do not proceed until the slice is confirmed.

## Step 2 — Read all inputs

Read, in order:

- The slice's `spec.md` — the acceptance criteria you will check against
- The slice's `plan.md` — the intended files changed and implementation approach
- The slice's `learnings.md` — implementer notes on deviations and surprises
- The epic's `spec.md` — scope and non-goals
- The root `CLAUDE.md` — repo conventions and component doc pointers
- All steering docs under `.claude/docs/steering/` — coding guidelines, testing strategy, CI patterns, and any others present
- The relevant component doc(s) under `.claude/docs/components/` for the areas touched

## Step 3 — Get the diff

Determine the base branch (default `main`). Run:

```
git diff <base>...<current-branch>
```

Read the full diff. Note every file added, modified, or deleted.

## Step 4 — Run quality checks

Run the build and lint for every component touched by the diff. Use the make targets:

- `.NET` code present: `dotnet build` the affected solution/project — must exit clean. The repo sets `TreatWarningsAsErrors` and runs Roslynator; any warning is a **Blocker**.
- Web code present (`src/Worms.Hub.Web/`): `make web.lint` — ESLint and `tsc --noEmit` must both pass. Any error is a **Blocker**.

Record the exact output of any failing command as evidence for the finding.

## Step 5 — Review the diff

### 5a — Acceptance criteria

For each acceptance criterion in the slice's `spec.md`, determine whether the diff satisfies it. Verify against the actual diff — read the specific file and line in the diff to confirm. Do not infer satisfaction from the plan's intended structure or from what you expect the implementation to have done. Mark each criterion MET, PARTIAL, or NOT MET with a file:line citation.

### 5b — Scope

Start by listing every file that appears in the diff. Then compare that list against the plan's "Files to Create / Modify" table.

- Flag files in the diff that are not in the plan.
- Flag planned files that are absent from the diff.
- Flag changes that go beyond what the plan describes for a file.

Changes not in the plan are not automatically wrong — but they need a reason. Check `learnings.md` first; if the deviation is explained there, note it as resolved. If it is unexplained, raise it as a finding.

### 5c — Coding guidelines

Check the diff for violations of the coding guidelines and CI patterns in `.claude/docs/steering/`

For any CI-related changes (new jobs, job conditions, change-detection gates, triggers), cross-reference the change against `.claude/docs/steering/ci-patterns.md` — do not evaluate correctness from spec wording alone. A criterion like "job skips on unrelated changes" may conflict with repo-wide CI conventions even if the spec asked for it.

For web code, check against the ESLint config and Prettier settings in `src/Worms.Hub.Web/`.

### 5d — Tests

Check the diff against `.claude/docs/steering/testing-strategy.md`:

## Step 6 — Write review.md

Write `review.md` in the same directory as the slice's `spec.md` and `plan.md`, using the template below.

```markdown
# Review — [Slice Name]

## Verdict

[One paragraph. Does the implementation satisfy the spec? Any blockers the user must address before merging?]

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| [criterion from spec] | MET / PARTIAL / NOT MET | file:line or explanation |

## Scope

[Does the diff match the plan's Files to Create / Modify table? List anything outside scope, with a note on whether learnings.md explains it.]

## Blockers

[Zero or more entries, numbered B1, B2, … Use the finding format below.]

## Suggestions

[Zero or more entries, numbered S1, S2, … Same format.]

## Nitpicks

[Zero or more entries, numbered N1, N2, … Same format.]

### Finding format

#### B1 — [short title]

- **File:** `path/to/file:line`
- **Issue:** One sentence describing what is wrong.
- **Fix:** Short direction for the fix.
- **Decision:** — *(pending)*

## Tests

[What test coverage was added or changed. Any coverage gaps. Any padding tests that should be removed or rewritten. Any fragile patterns.]

## Recommended Actions

[For every finding, state your recommended action and a one-line reason:]

- **B1** — Accept — [why the fix is clearly right]
- **S1** — Decline — [why it's not worth it or out of scope]
- **N1** — Accept — [reason]

Valid actions: `Accept` or `Decline`. Cover every finding.
```

## Rules

- Write only `review.md`. Do not edit code, commit, or touch the branch.
- Stay in scope: review only files in the diff. Do not audit the rest of the repo.
- Ignore process files in the diff (`learnings.md`, `plan.md`, `spec.md`, `review.md`) — these are workflow artefacts, not feature code.
- Be specific. "Error handling could be improved" is not useful; "`GifCreator.cs:42` does not handle the case where `frames` is empty" is.
- Match the bar the spec set. Do not raise production-grade concerns (HA, exhaustive logging, observability) the spec did not ask for.
- Categorise honestly. A Blocker genuinely breaks a spec criterion or the build. A Suggestion is meaningful improvement. A Nitpick is style. Do not inflate severity.
- Every finding must cite a line read from the actual current file. Do not raise a finding based on plan code examples, prior knowledge, or inference. A suggestion stating "X is missing" or "Y deviates from convention" must quote the actual file at the relevant location — if you have not read that file, read it before raising the finding. If the implementation already matches the desired state, do not raise the finding at all.
