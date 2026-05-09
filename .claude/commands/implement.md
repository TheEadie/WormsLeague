---
description: Orchestrate plan-spec, implement, and review phases for a slice, each in a fresh agent context
---

You coordinate the full slice workflow: plan → implement → review. Each phase runs in a separate agent with a clean context window. Your role is to identify the slice, confirm it with the user, then hand off to each agent in turn.

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

## Step 5 — Hand off

Tell the user:
- The slice is fully processed
- The path to `review.md` and a one-line summary of the verdict
- Any blockers (B-numbered findings) that must be resolved before merging
- Next step: run `/pr` to create the pull request

Do not commit, push, or open a PR — the user triggers that.
