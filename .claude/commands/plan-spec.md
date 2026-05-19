---
description: Generate a detailed implementation plan for the next spec slice, written to plan.md alongside spec.md
effort: high
---

Your task is to produce a detailed implementation plan for a slice and write it to `plan.md` next to the slice's `spec.md`. This plan is the direct input to `/implement`, so it must be precise enough for an agent to execute without further clarification.

YOU DO NOT IMPLEMENT THE SLICE. Only create the `plan.md` file.

## Step 1 — Identify the slice

Scan the user's request for a GitHub issue reference (full URL or `#NNN`). If one is present, this is **issue mode**: fetch the issue with `gh issue view <number-or-url> --json number,title,body,url`. The issue body is the slice's spec. The plan will be written as a sticky comment named `plan` on the same issue (see `.claude/docs/sticky-comments.md`) — no file will be created under `.claude/specs/`. Skip the epic-discovery flow below.

Otherwise, if the user has named a specific slice or path, use that.

If neither, find the next slice that has a `spec.md` but no `plan.md`:

1. List the epics under `.claude/specs/`. If none exist, stop and ask the user to run `/epic` first. If more than one exists, ask which epic to work on.
2. For the chosen epic, read `plan.md` at the epic level to understand the intended sequence. Then scan `.claude/specs/<epic-slug>/slices/` (and any sub-slice directories) in numbered order, looking for the first directory that contains a `spec.md` but no `plan.md`.
3. Present that slice to the user — its name and one-line description — and ask them to confirm or pick a different one.

Do not proceed until the user has confirmed the target slice.

## Step 2 — Read all context

Read everything needed to produce an accurate, codebase-consistent plan:

- The slice's `spec.md` — requirements, out of scope, acceptance criteria. **In issue mode**, this is the GitHub issue body itself.
- **Epic-mode only** — the epic's `spec.md` and `plan.md`: scope boundaries, what earlier slices have already delivered. In issue mode there is no epic; treat the issue body as the full scope.
- The root `CLAUDE.md` — repo-wide conventions and pointers to component docs
- All steering docs under `.claude/docs/steering/` — coding guidelines, testing strategy, CI patterns, and any others present
- The relevant component docs under `.claude/docs/components/` for the areas this slice touches
- The source files the slice will most likely create or modify
- Any `learnings.md` files from earlier slices in the same epic — they capture known caveats from prior implementation
- If the epic folder contains a `design/` directory (e.g. `.claude/specs/<epic-slug>/design/`) and this slice touches anything it covers, read the relevant files — treat them as a reference for layout, structure, and ideas, not as the authoritative definition.

When reading source files, record exactly what you find. Any factual claim the plan makes about existing file state — "this function is not yet registered", "the middleware block currently contains X", "this method does not exist" — must be directly verified from the file you read, not inferred or assumed from prior knowledge.

## Step 3 — Plan in plan mode

Use the EnterPlanMode tool to enter plan mode. Think through the full implementation before committing to any file content:

- Which files need to be created, modified, or deleted, and exactly what each change entails
- The right sequence to make those changes (what depends on what)
- Non-obvious decisions left open by the spec: library versions, build system wiring, CI job names, config keys, DI registration — resolve them here
- How to verify each piece of work is correct once done
- Any risks or caveats the plan should call out explicitly
- If the slice adds a new endpoint that returns a richer response type for a single item while a corresponding list endpoint already exists for the same domain resource, include an explicit scope decision: does the list endpoint need updating to match? Do not leave the asymmetry implicit — decide in or out of scope and note it in the plan.

Use the ExitPlanMode tool to exit plan mode before writing any files.

## Step 4 — Write the plan

Be concrete: file paths, exact dependency versions, make target names, CI job names, config values. An agent following this plan should not need to make decisions — all decisions are resolved here.

**Epic mode:** write `plan.md` in the same directory as the slice's `spec.md`.

**Issue mode:** render the plan body to a temp file with `<!-- claude:sticky:plan -->` as the first line, then create or update the `plan` sticky comment on the issue using the flow in `.claude/docs/sticky-comments.md`. Do not create any files under `.claude/specs/`.

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

Tell the user where the plan lives — the `plan.md` path in epic mode, or the issue URL (and a note that it is the `plan` sticky comment) in issue mode — and that it is ready for review or implementation with `/implement`. Do not commit, branch, or open a PR.
