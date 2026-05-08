---
description: Generate a detailed implementation plan for the next spec slice, written to plan.md alongside spec.md
---

Your task is to produce a detailed implementation plan for a slice and write it to `plan.md` next to the slice's `spec.md`. This plan is the direct input to `/implement`, so it must be precise enough for an agent to execute without further clarification.

YOU DO NOT IMPLEMENT THE SLICE. Only create the `plan.md` file.

## Step 1 — Identify the slice

If the user has named a specific slice or path, use that.

Otherwise, find the next slice that has a `spec.md` but no `plan.md`:

1. List the epics under `.claude/specs/`. If none exist, stop and ask the user to run `/epic` first. If more than one exists, ask which epic to work on.
2. For the chosen epic, read `plan.md` at the epic level to understand the intended sequence. Then scan `.claude/specs/<epic-slug>/slices/` (and any sub-slice directories) in numbered order, looking for the first directory that contains a `spec.md` but no `plan.md`.
3. Present that slice to the user — its name and one-line description — and ask them to confirm or pick a different one.

Do not proceed until the user has confirmed the target slice.

## Step 2 — Read all context

Read everything needed to produce an accurate, codebase-consistent plan:

- The slice's `spec.md` — requirements, out of scope, acceptance criteria
- The epic's `spec.md` and `plan.md` — scope boundaries, what earlier slices have already delivered
- The root `CLAUDE.md` — repo-wide conventions and pointers to component docs
- All steering docs under `.claude/docs/steering/` — coding guidelines, testing strategy, CI patterns, and any others present
- The relevant component docs under `.claude/docs/components/` for the areas this slice touches
- The source files the slice will most likely create or modify
- Any `learnings.md` files from earlier slices in the same epic — they capture known caveats from prior implementation

## Step 3 — Plan in plan mode

Use the EnterPlanMode tool to enter plan mode. Think through the full implementation before committing to any file content:

- Which files need to be created, modified, or deleted, and exactly what each change entails
- The right sequence to make those changes (what depends on what)
- Non-obvious decisions left open by the spec: library versions, build system wiring, CI job names, config keys, DI registration — resolve them here
- How to verify each piece of work is correct once done
- Any risks or caveats the plan should call out explicitly

Use the ExitPlanMode tool to exit plan mode before writing any files.

## Step 4 — Write plan.md

Write `plan.md` in the same directory as the slice's `spec.md`. Be concrete: file paths, exact dependency versions, make target names, CI job names, config values. An agent following this plan should not need to make decisions — all decisions are resolved here.

### Plan file template

```markdown
# Plan: [Slice Name]

## Context

[One paragraph: what this slice delivers and how it fits the wider system. Call out which earlier slices it builds on and what they have already put in place.]

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `path/to/file` | What it does |

### Modified files

| Path | Change |
|---|---|
| `path/to/file` | What changes and why |

---

## Implementation Details

### 1. [Major concern]

[Detailed guidance: exact file contents or structure where it matters, specific values
(dependency versions, config keys, env var names), non-obvious wiring (DI registration,
makefile includes, CI job ordering), any known caveats from the codebase.]

### 2. [Next concern]

[...]

---

## Verification

1. [Command or action that confirms a specific piece of work is correct]
2. [Observable outcome that confirms end-to-end correctness]
```

## Step 5 — Hand off

Tell the user where `plan.md` was written and that it is ready for review or implementation with `/implement`. Do not commit, branch, or open a PR.
