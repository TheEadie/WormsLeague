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

## Step 2b — Review the plan with the user

Read `<slice-path>/plan.md` and display its full contents to the user.

Then ask: **"Does this plan look good, or would you like to change anything before implementation starts?"**

If the user requests changes:
1. Delete `<slice-path>/plan.md`.
2. Note the user's feedback.
3. Re-run Step 2, appending this to the agent prompt: `The user reviewed the previous plan and asked for these changes: <feedback>. Incorporate this feedback when writing the new plan.`
4. Repeat Step 2b.

Do not proceed to Step 3 until the user explicitly confirms the plan.

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

For each remaining finding, look at the referenced file to make the proposed fix concrete, then present it to the user in this format:

```
**[B1] — [title]**
File: `path/to/file:line`
Issue: [issue from review]

Proposed fix:
[precise description of what to change, with a code snippet if it helps]

→ Resolve / Ignore / Skip?
```

Based on the user's response:

- **Resolve** — implement the fix. Then update the finding's `**Decision:**` line in `review.md` from `— *(pending)*` to `Accept`.
- **Ignore** — do nothing. Update the finding's `**Decision:**` line in `review.md` to `Decline`.
- **Skip** — leave the finding as-is and move to the next one.

Work through all findings before moving to the hand-off step.

## Step 6 — Hand off

Tell the user:
- How many findings were resolved, ignored, and skipped
- Whether any unresolved Blockers remain (they must be addressed before merging)
- Next step: run `/pr` to create the pull request

Do not commit, push, or open a PR — the user triggers that.
