---
description: Orchestrate plan-spec, implement, review, and interactive finding resolution for a slice
effort: medium
---

You coordinate the full slice workflow: plan → implement → review → resolve. The first three phases each run in a separate agent with a clean context window. The resolution phase runs interactively in this session. Your role is to identify the slice, confirm it with the user, then coordinate each phase in turn.

## Step 1 — Identify the slice

Scan the user's request for a GitHub issue reference (full URL or `#NNN`). If one is present, this is **issue mode**: fetch the issue and inspect its sticky comments (see `.claude/docs/sticky-comments.md`) to determine which phases need to run:

- Issue body exists (spec) but no `plan` sticky comment → planning + implementing + reviewing
- `plan` sticky exists but no `learnings` sticky → implementing + reviewing
- `learnings` sticky exists but no `review` sticky → reviewing only

If the issue has no body, stop and tell the user to run `/spec` against it first. Record the issue URL and number, and skip the epic-discovery flow below.

Otherwise, if the user has named a specific slice or path, use that.

If neither, find the next slice to work on:

1. List the epics under `.claude/specs/`. If more than one exists, ask which epic to work on.
2. Read the epic's `plan.md`. Scan slices in order and pick the first incomplete one using this logic:
   - Slice has `spec.md` but no `plan.md` → needs planning + implementing + reviewing
   - Slice has `plan.md` but no `learnings.md` → needs implementing + reviewing
   - Slice has `learnings.md` but no `review.md` → needs reviewing only
3. Present the slice to the user — its name and which phases will run — and ask them to confirm or pick a different one.

Do not proceed until the user has confirmed the target. Record either the slice's full directory path (epic mode, e.g. `.claude/specs/<epic-slug>/slices/<slice-dir>`) or the issue URL (issue mode) — this is `<target>` below.

In each of Steps 2–4 below, the agent prompt and the existence check depend on which mode you are in. The "target identifier" passed to each sub-agent is either the slice directory path (epic mode) or the issue URL (issue mode).

## Step 2 — Plan phase

**Skip condition:** in epic mode, skip if `<slice-path>/plan.md` exists; in issue mode, skip if the `plan` sticky comment exists on the issue.

Spawn an agent with this prompt, substituting `<target>` with the slice path or issue URL:

> Read the file `.claude/commands/plan-spec.md` and follow those instructions.
> The target has already been confirmed by the user: `<target>`.
> Skip Step 1 (identification) and proceed directly from Step 2 onward.

After the agent completes, verify the plan artefact exists (`<slice-path>/plan.md` in epic mode, or the `plan` sticky comment on the issue in issue mode). If it does not, stop and report the failure to the user.

## Step 3 — Implement phase

**Skip condition:** in epic mode, skip if `<slice-path>/learnings.md` exists; in issue mode, skip if the `learnings` sticky comment exists on the issue.

Spawn an agent with this prompt, substituting `<target>` with the slice path or issue URL:

> Read the file `.claude/commands/implement-slice.md` and follow those instructions.
> The target has already been confirmed by the user: `<target>`.
> Skip Step 1 (identification) and proceed directly from Step 2 onward.

After the agent completes, verify the learnings artefact exists (`<slice-path>/learnings.md` in epic mode, or the `learnings` sticky comment on the issue in issue mode). If it does not, stop and report the failure to the user.

## Step 4 — Review phase

**Skip condition:** in epic mode, skip if `<slice-path>/review.md` exists; in issue mode, skip if the `review` sticky comment exists on the issue.

Spawn an agent with this prompt, substituting `<target>` with the slice path or issue URL:

> Read the file `.claude/commands/review.md` and follow those instructions.
> The target has already been confirmed by the user: `<target>`.
> Skip Step 1 (identification) and proceed directly from Step 2 onward.

After the agent completes, verify the review artefact exists (`<slice-path>/review.md` in epic mode, or the `review` sticky comment on the issue in issue mode). If it does not, stop and report the failure to the user.

## Step 5 — Resolve findings interactively

Read the review artefact — `<slice-path>/review.md` in epic mode, or the `review` sticky comment on the issue in issue mode (see `.claude/docs/sticky-comments.md`) — and collect all findings in document order: Blockers (B1, B2, …), then Suggestions (S1, S2, …), then Nitpicks (N1, N2, …). Skip any finding whose `**Decision:**` line is already set to `Accept` or `Decline`.

For each remaining finding, look at the referenced file to make the proposed fix concrete.

Present all findings at once as a single table:

| ID | Severity | Title | File | Issue | Proposed Fix |
|----|----------|-------|------|-------|--------------|
| B1 | Blocker | … | `path/to/file:line` | … | … |
| S1 | Suggestion | … | `path/to/file:line` | … | … |
| N1 | Nitpick | … | `path/to/file:line` | … | … |

Then ask: **"Reply with the IDs you want resolved (e.g. `B1 S2`), the IDs you want ignored, or `all` to resolve everything. Any ID not mentioned will be skipped."**

Wait for a single reply, then apply all accepted fixes in one batch. After all changes are made, update the review artefact in place:

- Update each accepted finding's `**Decision:**` line to `Accept`.
- Update each declined finding's `**Decision:**` line to `Decline`.
- Leave skipped findings as `*(pending)*`.

In epic mode this is an edit to `review.md`. In issue mode, read the current body of the `review` sticky comment, apply the same line changes, and write the full updated body back to the same comment (do not append a second `review` sticky — see `.claude/docs/sticky-comments.md`).

## Step 6 — Hand off

Tell the user:
- How many findings were resolved, ignored, and skipped
- Whether any unresolved Blockers remain (they must be addressed before merging)
- Next step: run `/pr` to create the pull request

Do not commit, push, or open a PR — the user triggers that.
