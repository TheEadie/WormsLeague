---
description: Generate a detailed implementation plan for a slice and write it as the `plan` sticky comment on the slice's GitHub issue
effort: high
---

Your task is to produce a detailed implementation plan for a slice and write it as the `plan` sticky comment on the slice's GitHub issue. This plan is the direct input to `/implement`, so it must be precise enough for an agent to execute without further clarification.

YOU DO NOT IMPLEMENT THE SLICE. Only write the `plan` sticky comment.

## Step 1 — Identify the slice issue

Scan the user's request for a GitHub issue reference (a full GitHub issue URL, or a `#NNN` token). If none is present, stop and ask the user which issue this plan is for. Do not proceed without an explicit issue reference.

Fetch the issue:

```bash
gh issue view <number-or-url> --json number,title,body,url
```

The issue body is the slice's spec. If the body still looks like the one-sentence stub created by `/epic` (i.e. `/spec` has not been run yet against this issue), stop and tell the user to run `/spec` first.

Record the issue number and URL for use in Step 4.

## Step 2 — Read all context

Read everything needed to produce an accurate, codebase-consistent plan:

- The slice issue body — requirements, out of scope, acceptance criteria.
- If the issue body contains a `Part of #<n>` pointer, fetch that parent epic issue and read its body — scope boundaries, what earlier slices have already delivered.
- For each earlier sub-issue of the same parent that is closed (or has a `learnings` sticky), read its `learnings` sticky comment — they capture known caveats from prior implementation.
- The root `CLAUDE.md` — repo-wide conventions and pointers to component docs.
- All steering docs under `.claude/docs/steering/` — coding guidelines, testing strategy, CI patterns, and any others present.
- The relevant component docs under `.claude/docs/components/` for the areas this slice touches.
- The source files the slice will most likely create or modify.

When reading source files, record exactly what you find. Any factual claim the plan makes about existing file state — "this function is not yet registered", "the middleware block currently contains X", "this method does not exist" — must be directly verified from the file you read, not inferred or assumed from prior knowledge.

## Step 3 — Plan in plan mode

Use the EnterPlanMode tool to enter plan mode. Think through the full implementation before committing to any file content:

- Which files need to be created, modified, or deleted, and exactly what each change entails
- The right sequence to make those changes (what depends on what)
- Non-obvious decisions left open by the spec: library versions, build system wiring, CI job names, config keys, DI registration — resolve them here
- How to verify each piece of work is correct once done
- Any risks or caveats the plan should call out explicitly
- If the slice adds a new endpoint that returns a richer response type for a single item while a corresponding list endpoint already exists for the same domain resource, include an explicit scope decision: does the list endpoint need updating to match? Do not leave the asymmetry implicit — decide in or out of scope and note it in the plan.

Use the ExitPlanMode tool to exit plan mode before writing the sticky comment.

## Step 4 — Write the plan sticky comment

Render the plan body to a temp file (e.g. `/tmp/sticky-plan.md`) with `<!-- claude:sticky:plan -->` as the first line, then create or update the `plan` sticky comment on the issue using the flow in `.claude/docs/sticky-comments.md`.

Be concrete: file paths, exact dependency versions, make target names, CI job names, config values. An agent following this plan should not need to make decisions — all decisions are resolved here.

### Plan body template

```markdown
<!-- claude:sticky:plan -->

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

Tell the user the issue URL (noting that the plan is in the `plan` sticky comment) and that it is ready for review or implementation with `/implement`. Do not commit, branch, or open a PR.
