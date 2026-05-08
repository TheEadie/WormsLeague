---
description: Turn one slice from an epic plan into a focused, implementable spec
model: opus
---

Your task is to create a slice specification file from a user's request. This spec will be reviewed by a colleague (human or AI) before implementation, so it must be unambiguous and complete.

YOU DO NOT IMPLEMENT THE USER'S REQUEST. Only create the required spec file.

## Step 1 — Understand the request

If the user has already named a slice or supplied a GitHub Issue, use that. If they have a GitHub Issue, fetch the issue description to learn what is needed.

Otherwise, propose the next slice from an epic plan:

1. List the epics under `.claude/specs/`. If none exist, stop and ask the user to run `/epic` first. If exactly one exists, use it. If more than one exists, ask the user which epic this slice belongs to.
2. Read the chosen epic's `.claude/specs/<epic-slug>/plan.md` and find the first unchecked (`- [ ]`) slice whose dependencies (any earlier slices it relies on) are all checked (`- [x]`).
3. Present that slice to the user as the suggested next one, including its short name and one-line description from the plan.
4. Ask the user to confirm, pick a different slice from the plan, or describe something else entirely (including a GitHub Issue).

Do not proceed until the user has confirmed which slice the spec is for.

A slice is intended to be a small, deliverable chunk that ships as a single PR — not a large epic. If it seems too big, challenge the user to break it down and begin with a smaller first step. See "If the slice is too large" below.

## Step 2 — Learn the epic and codebase context

Before writing anything, read the following to understand how the slice fits the wider work and the existing system:

- `.claude/specs/<epic-slug>/spec.md` — the epic's purpose, goals, non-goals, major capabilities, system shape, constraints, and definition of done. The slice must sit consistently within this scope.
- `.claude/specs/<epic-slug>/plan.md` — the ordered slice list. Locate this slice in the plan, note which earlier slices it depends on (and may assume are complete), and which later slices it must NOT pre-empt.
- The root `CLAUDE.md` — repo-wide conventions and pointers.
- The relevant component doc(s) under `.claude/docs/components/` — load whichever match the area this slice touches (e.g. `cli.md`, `hub-gateway.md`, `armageddon-files.md`). The mapping is in the root `CLAUDE.md`.
- Any source files directly relevant to the slice area.

This ensures the spec accurately reflects the epic's intent, the system's boundaries, and existing behaviour. If the user's request appears to contradict the epic spec or to skip ahead in the plan, raise that with them before writing the slice spec.

## Step 3 — Establish the simplest viable approach

The default position is always the simplest thing that meets the stated need.

Before probing for detail, identify the simplest version of the slice and present it to the user. Then identify any aspects of the request that could be implemented in a more complex or robust way — things like validation, error handling, configuration options, edge case coverage, or extensibility. For each of these, ask the user whether they want it included. Do not assume more is better.

If the user's initial request already contains complexity that isn't strictly necessary to deliver the slice, surface that and ask whether it can be simplified or deferred to a later slice.

## Step 4 — Resolve ambiguities

Probe for the kinds of ambiguity that most commonly cause misunderstanding between author and reviewer. Ask the user about anything that is unclear, including:

- Which components or projects are affected
- Error cases and how they should be handled
- Any configuration or inputs required
- How this interacts with existing features
- Edge cases the user has considered

Do not proceed to write the spec until you have enough detail to fill every section completely. Keep asking until there are no open questions remaining, or until the user explicitly decides to leave something unresolved.

## Step 5 — Create the spec file

1. Create a numbered subdirectory under `.claude/specs/<epic-slug>/slices/`. Determine the next number by finding the highest-numbered folder currently in that `slices/` directory and incrementing by one, zero-padded to two digits (e.g. if the highest is `02-event-log`, the new folder is `03-audit-export/`). If the `slices/` directory does not yet exist or contains no numbered folders, start at `01`.
2. Derive the slug from the slice's short name in `plan.md` (lowercase, hyphenated). If the user is specifying a slice not in the plan, agree a slug with them.
3. Create `spec.md` inside that folder using the template below.

### Spec file template

```markdown
# [Slice Name]

## Overview

[One or two sentences describing what is delivered and why.]

## Requirements

[Bulleted list of capabilities and behaviours that must exist. Written from the user/requirements perspective — what the system must do, not how it does it.]

## Out of Scope

[Explicit list of related things this slice does NOT include. This is as important as the requirements — it prevents reviewers from making different assumptions about scope.]

## Acceptance Criteria

[Bulleted list of conditions that must be true for the slice to be considered complete. Written as observable outcomes: "Given X, when Y, then Z." Each requirement above should map to at least one criterion here.]

## Open Questions

[Any unresolved ambiguities the user has chosen to defer. Leave this section empty (or omit it) if all questions were resolved.]
```

### Rules for the spec content

- Focus on WHAT is needed, not HOW to build it
- Never add anything the user didn't explicitly ask for
- Default to the simplest approach — only include complexity the user explicitly agreed to in Step 3
- Do not include implementation details such as class names, function signatures, interfaces, or code structure — those belong in the implementation plan
- Do not prescribe the technical approach
- Every requirement must have at least one acceptance criterion

## Step 6 — Hand off

After creating the spec file, tell the user where it lives and that it is ready for review or implementation. Do not commit, branch, or open a PR — the existing `/pr` skill handles that once code changes exist. Do not tick the slice off in `plan.md` — that happens at delivery time, not spec time.

## If the slice is too large

If the slice is too large to be a single PR-sized deliverable, you MUST suggest breaking it into multiple smaller sub-slices. Propose a concrete breakdown and wait for the user to agree before creating any files.

Once agreed, create the parent numbered subdirectory under `.claude/specs/<epic-slug>/slices/` (e.g. `03-audit-export/`), then numbered sub-subdirectories inside it for each sub-slice (using the format `01-`, `02-`, etc. so ordering is unambiguous). Each sub-subdirectory gets its own `spec.md`.

Example structure:

```
.claude/specs/audit-masking/
  spec.md
  plan.md
  slices/
    03-audit-export/
      01-csv-download/
        spec.md
      02-scheduled-email/
        spec.md
```
