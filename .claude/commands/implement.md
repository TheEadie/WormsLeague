---
description: Implement a slice from its plan.md, recording anything surprising or missing in learnings.md
---

Your task is to implement a slice by following its `plan.md` precisely, tracking progress with tasks, and recording anything the plan missed or got wrong in `learnings.md`.

## Step 1 — Identify the slice

If the user has named a specific slice or path, use that.

Otherwise, find the next slice that is ready to implement:

1. List the epics under `.claude/specs/`. If more than one exists, ask which epic to work on.
2. Read the epic's `plan.md`. Find the first unchecked (`- [ ]`) slice.
3. Check whether `.claude/specs/<epic-slug>/slices/<slice-dir>/plan.md` exists. If it does not, stop and tell the user to run `/plan-spec` first to generate the implementation plan.
4. Present the slice to the user and ask them to confirm or pick a different one.

Do not proceed until the user has confirmed the target slice.

## Step 2 — Read the plan and context

Read everything before touching any files:

- The slice's `plan.md` — the authoritative implementation guide
- The slice's `spec.md` — acceptance criteria you will verify against at the end
- The root `CLAUDE.md` — repo-wide conventions
- All steering docs under `.claude/docs/steering/` — coding guidelines, testing strategy, CI patterns, and any others present
- Relevant component docs under `.claude/docs/components/` for the areas touched
- Any `learnings.md` files from earlier slices in the same epic — they capture caveats that may apply here too

## Step 3 — Create tasks

Use TaskCreate to break the plan into discrete tasks before starting any work — one task per major section of the plan's implementation details. This gives the user a live view of progress. Mark each task `in_progress` immediately before starting it and `completed` immediately after finishing it. Do not batch completions.

## Step 4 — Implement

Follow the plan exactly. For each task:

1. Mark the task `in_progress`.
2. Make the changes the plan describes.
3. Run any verification steps the plan specifies for this section.
4. Mark the task `completed`.

If the plan is silent on something you need to decide, make the simplest reasonable choice and record it as a learning (see below).

If a verification step fails, diagnose the root cause rather than retrying the same action. If the fix requires a significant deviation from the plan, note it as a learning.

### What to record as a learning

Keep a running mental note of anything that warrants recording. Capture a learning when:

- The plan omits something you had to discover yourself (a missing dependency, a required lockfile, a package not listed, a config key the plan didn't mention)
- A tool call fails in a non-obvious way and needs a workaround
- You loop on a problem more than once before resolving it
- You make a decision the plan did not cover
- An assumption in the plan turns out to be wrong
- Something in the codebase behaves differently from what the plan implied

Do not record learnings for steps that went exactly as planned.

## Step 5 — Tick the slice in the epic plan

After all implementation tasks are complete, mark the slice as done in the epic's top-level `plan.md` by changing its checkbox from `- [ ]` to `- [x]`.

## Step 6 — Write learnings.md

After ticking the epic plan, write `learnings.md` in the same directory as the slice's `plan.md`.

Write it even if there are no learnings — its presence signals the slice has been implemented. If there is nothing to record, say so briefly.

### Learnings file template

```markdown
# Learnings: [Slice Name]

## Implementation Notes

[One heading per learning. Each entry should describe:
- What the plan said or assumed (or what it omitted)
- What actually happened
- What had to be done differently or added

Keep entries factual and specific — the goal is to improve the plan template and
component docs in a future /update-docs pass.]

## Files Added (not in plan)

[List any files created that the plan did not mention, with a brief reason.
Omit this section if there are none.]
```

## Step 7 — Hand off

Tell the user:
- The implementation is complete
- Where `learnings.md` was written and a one-line summary of the most significant learning (if any)
- That the slice is ready for review and PR creation with `/pr`

Do not commit, push, or open a PR — the user triggers that.
