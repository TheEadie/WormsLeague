---
description: Orchestrate plan-spec, implement, review, and interactive finding resolution for a slice
effort: medium
---

You coordinate the full slice workflow: plan → implement → review → resolve. The first three phases each run in a separate agent with a clean context window. The resolution phase runs interactively in this session. Your role is to identify the slice, confirm it with the user, then coordinate each phase in turn.

## Step 1 — Identify the slice

If the user has named a specific slice or path, use that.

Otherwise, find the next slice to work on:

1. List the epics under `.claude/specs/`. If more than one exists, ask which epic to work on.
2. Read the epic's `plan.md`. Scan slices in order and pick the first incomplete one using this logic:
   - Slice has `spec.md` but no `plan.md` → needs planning + implementing + reviewing
   - Slice has `plan.md` but no `learnings.md` → needs implementing + reviewing
   - Slice has `learnings.md` but no `review.md` → needs reviewing only
3. Present the slice to the user — its name and which phases will run — and ask them to confirm or pick a different one.

Do not proceed until the user has confirmed the target slice. Record the slice's full directory path (e.g. `.claude/specs/<epic-slug>/slices/<slice-dir>`).

## Step 2 — Plan phase

Skip this step if `<slice-path>/plan.md` already exists.

Spawn an agent with this prompt, substituting `<slice-path>`:

> Read the file `.claude/commands/plan-spec.md` and follow those instructions.
> The target slice has already been confirmed by the user. Its directory is `<slice-path>`.
> Skip Step 1 (slice identification) and proceed directly from Step 2 onward.

After the agent completes, verify that `<slice-path>/plan.md` exists. If it does not, stop and report the failure to the user.

## Step 3 — Implement phase

Skip this step if `<slice-path>/learnings.md` already exists.

Spawn an agent with this prompt, substituting `<slice-path>`:

> Read the file `.claude/commands/implement-slice.md` and follow those instructions.
> The target slice has already been confirmed by the user. Its directory is `<slice-path>`.
> Skip Step 1 (slice identification) and proceed directly from Step 2 onward.

After the agent completes, verify that `<slice-path>/learnings.md` exists. If it does not, stop and report the failure to the user.

## Step 4 — Review phase

Skip this step if `<slice-path>/review.md` already exists.

Spawn an agent with this prompt, substituting `<slice-path>`:

> Read the file `.claude/commands/review.md` and follow those instructions.
> The target slice has already been confirmed by the user. Its directory is `<slice-path>`.
> Skip Step 1 (slice identification) and proceed directly from Step 2 onward.

After the agent completes, verify that `<slice-path>/review.md` exists. If it does not, stop and report the failure to the user.

## Step 5 — Resolve findings interactively

Read `<slice-path>/review.md` and collect all findings in document order: Blockers (B1, B2, …), then Suggestions (S1, S2, …), then Nitpicks (N1, N2, …). Skip any finding whose `**Decision:**` line is already set to `Accept` or `Decline`.

For each remaining finding, look at the referenced file to make the proposed fix concrete.

Present all findings at once as a single table:

| ID | Severity | Title | File | Issue | Proposed Fix |
|----|----------|-------|------|-------|--------------|
| B1 | Blocker | … | `path/to/file:line` | … | … |
| S1 | Suggestion | … | `path/to/file:line` | … | … |
| N1 | Nitpick | … | `path/to/file:line` | … | … |

Then ask: **"Reply with the IDs you want resolved (e.g. `B1 S2`), the IDs you want ignored, or `all` to resolve everything. Any ID not mentioned will be skipped."**

Wait for a single reply, then apply all accepted fixes in one batch. After all changes are made:

- Update each accepted finding's `**Decision:**` line in `review.md` to `Accept`.
- Update each declined finding's `**Decision:**` line in `review.md` to `Decline`.
- Leave skipped findings as `*(pending)*`.

## Step 6 — Hand off

Tell the user:
- How many findings were resolved, ignored, and skipped
- Whether any unresolved Blockers remain (they must be addressed before merging)
- Next step: run `/pr` to create the pull request

Do not commit, push, or open a PR — the user triggers that.
