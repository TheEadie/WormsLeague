---
description: Review an implementation against its spec and the repo's coding standards, producing an advisory report. Dispatches parallel sub-agents per axis.
effort: high
---

You orchestrate a two-axis review of the changes made for a slice:

- **Spec axis** — does the diff satisfy the slice's `spec.md`? Handled by `spec-reviewer`.
- **Standards axis** — does the diff follow the repo's documented coding standards? Handled by `csharp-reviewer` and/or `react-reviewer` depending on which languages the diff touches.

The sub-agents run **in parallel** so neither pollutes the other's context. You merge their findings into a single `review.md`, axis-separated. You do NOT make code changes, commit, or modify the branch.

Your review is advisory. The user will read it and decide which items to act on.

## Step 1 — Identify the slice

Scan the user's request for a GitHub issue reference (full URL or `#NNN`). If one is present, this is **issue mode**: fetch the issue and its sticky comments (see `.claude/docs/sticky-comments.md`). The issue body is the spec; the `plan` and `learnings` sticky comments must both exist (otherwise stop and tell the user the slice isn't ready for review). The review will be written as the `review` sticky comment on the same issue. Skip the epic-discovery flow below.

Otherwise, if the user has named a specific slice or branch, use that.

If neither, infer from context:

1. Check the current branch name for an epic/slice hint (e.g. `feature/some-slug`).
2. Look under `.claude/specs/` for a slice directory containing a `plan.md` and `learnings.md` — the presence of `learnings.md` means it has been implemented and is ready for review.
3. If more than one candidate exists, list them and ask the user which to review.

Do not proceed until the slice is confirmed.

## Step 2 — Get the diff and classify touched files

Determine the base branch (default `main`). Run:

```
git diff --name-only <base>...<current-branch>
```

Classify the resulting file list:

- **C# files** — `*.cs`, `*.csproj`, `*.razor`, `*.sln`, `Directory.*.props`/`targets`, anywhere under `src/` outside `src/Worms.Hub.Web/`.
- **Web files** — anything under `src/Worms.Hub.Web/`.
- **Other** — migrations, Docker, infra, docs, CI workflow files. These are still part of the spec review, but no language reviewer runs for them.

Also note which component(s) the C# files belong to (e.g. `Worms.Cli`, `Worms.Hub.Gateway`) so you can pass that hint to `csharp-reviewer`.

## Step 3 — Dispatch sub-agents in parallel

Send **one message** with multiple `Agent` tool calls so they run concurrently:

1. **Always** spawn `spec-reviewer` with:
   - The slice directory path (epic mode), or the GitHub issue URL plus a pointer to the `plan` sticky comment (issue mode — see `.claude/docs/sticky-comments.md`).
   - The base branch and current branch.

2. **If any C# files changed**, spawn `csharp-reviewer` with:
   - The base branch and current branch.
   - The list of C# files touched.
   - The component(s) those files belong to.

3. **If any web files changed**, spawn `react-reviewer` with:
   - The base branch and current branch.
   - The list of web files touched (paths under `src/Worms.Hub.Web/`).

Do not duplicate the sub-agents' work in the main context — let them read the standards docs and the diff themselves.

## Step 4 — Merge into the review artefact

Once all sub-agents have returned, render the review using the template below.

**Epic mode:** write the result to `review.md` in the slice directory.

**Issue mode:** render to a temp file with `<!-- claude:sticky:review -->` as the first line, then create or update the `review` sticky comment on the issue using the flow in `.claude/docs/sticky-comments.md`. Do not create any files under `.claude/specs/`.

Preserve each sub-agent's findings verbatim within its section — do not rerank across axes and do not collapse multiple axes into a single severity list. Renumber findings only if needed to keep them unique within their axis (e.g. `Spec B1`, `C# B1`, `Web B1` are all fine; if the spec reviewer returned no findings, the section is "None").

Write a one-paragraph verdict that summarises both axes honestly: a Spec-pass / Standards-fail change reads differently from the reverse, and the verdict should make that clear.

```markdown
# Review — [Slice Name]

## Verdict

[One paragraph. Summarise both axes: does the implementation satisfy the spec? Does it follow the standards? Any blockers the user must address before merging?]

## Spec

[Verbatim from spec-reviewer: Acceptance Criteria table, Blockers, Suggestions, Nitpicks. If a sub-section is empty, write "None".]

## C# Standards

[Verbatim from csharp-reviewer, including the build PASS/FAIL line. Omit this whole section if csharp-reviewer was not dispatched.]

## Web Standards

[Verbatim from react-reviewer, including the lint PASS/FAIL line. Omit this whole section if react-reviewer was not dispatched.]

## Recommended Actions

[For every finding across all axes, state your recommended action and a one-line reason. Reference findings by axis-prefixed ID:]

- **Spec B1** — Accept — [why the fix is clearly right]
- **C# B1** — Accept — [reason]
- **C# S1** — Decline — [why it's not worth it or out of scope]
- **Web N1** — Accept — [reason]

Valid actions: `Accept` or `Decline`. Cover every finding.
```

## Rules

- Write only the review artefact (`review.md` in epic mode, or the `review` sticky comment in issue mode). Do not edit code, commit, or touch the branch.
- Dispatch sub-agents in parallel in a single message — not sequentially.
- The orchestrator (you) does not re-do the sub-agents' analysis. Your job is dispatch, merge, verdict, and recommended actions.
- Stay in scope: review only files in the diff. Do not audit the rest of the repo.
- Ignore process artefacts (`learnings.md`, `plan.md`, `spec.md`, `review.md`, and the equivalent issue body / sticky comments in issue mode) when reasoning about scope — these are workflow artefacts, not feature code.
- Match the bar the spec set. Do not raise production-grade concerns (HA, exhaustive logging, observability) the spec did not ask for.
- If a sub-agent returns nothing in a category, write "None" in that sub-section — don't omit it (except for whole axes that weren't dispatched).
