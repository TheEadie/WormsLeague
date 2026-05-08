---
description: Turn a large feature description into a high-level epic spec with a sliced PR breakdown
---

Your task is to collaborate with the user to produce two high-level epic documents in the current project:

- `.claude/specs/<epic-slug>/spec.md` — the project-level specification (purpose, scope, capabilities, shape).
- `.claude/specs/<epic-slug>/plan.md` — the ordered, PR-sized slice breakdown derived from the spec.

Neither document prescribes implementation detail. They are companion files: the spec describes *what the epic is*, and the plan describes *how it will be delivered as a sequence of PR-sized slices*.

This is a multi-phase, conversational process. You and the user will iterate on the document together. Do not rush to a final draft; the value is in the conversation.

Multiple epics may live in the same repo. Each epic gets its own sub-folder under `.claude/specs/`.

## Scope and tone

Keep both documents strictly high level.

`spec.md` is concerned with:

- What the epic is and why it exists
- The major capabilities and behaviours it provides
- The shape of the system at a conceptual level (major components, external integrations, data the system deals with)
- Constraints, assumptions, and non-goals

`plan.md` is concerned with:

- An ordered list of vertical slices, each small enough to be a single PR, described at a "what is delivered" level

Neither file is concerned with:

- Implementation details (class names, function signatures, file paths, code structure, libraries, frameworks unless the user has explicitly anchored on one)
- Edge cases, error handling specifics, validation rules
- Concerns that will naturally be refined when individual slices are picked up
- Target users / personas — deliberately omitted from this command

When in doubt, defer detail to the slice-implementation stage. Better to leave something coarse than to over-specify.

## YOU DO NOT IMPLEMENT THE EPIC

Your only outputs are `.claude/specs/<epic-slug>/spec.md` and `.claude/specs/<epic-slug>/plan.md`. Do not write source code, scaffolding, configuration, or any other artifact during this command.

---

## Phase 0 — Choose epic slug and create folder

If the user invoked the command with a description in the args, use it. Otherwise ask them for a one-paragraph description of the epic.

Propose a short, lowercase, hyphenated slug derived from the description — typically one or two words, no spaces (e.g. "Audit Secret Masking" → `audit-masking`, "Databricks Support for RgCompare" → `databricks`). Show the proposed slug and ask the user to confirm or supply an alternative.

Once the slug is agreed:

- If `.claude/specs/<slug>/spec.md` or `.claude/specs/<slug>/plan.md` already exists, stop and ask whether to resume editing them or pick a different slug. Do not clobber existing files.
- Otherwise create `.claude/specs/<slug>/` (the parent `.claude/specs/` may need creating too). The `spec.md` and `plan.md` files will be created in later phases.

## Phase 1 — Elicit the initial description

Ask the user to describe the epic they want to build, with prompts like:

- What is the epic, in a sentence or two?
- What problem does it solve?
- Is there an existing system, repo, or document this builds on or replaces?
- Are there any links, issues, or reference materials you should read first?

Listen. Do not start probing for detail yet. The goal is to get the user's framing into the conversation.

## Phase 2 — Research and orient

Read whatever you need to understand the context. Depending on the user's answers, this may include:

- Files in the current working directory (`CLAUDE.md`, `README.md`, any existing specs under `.claude/specs/` or `specs/`)
- Source files, if the epic extends or relates to existing code
- GitHub issues, linked documents, or external references the user pointed to

Do NOT speculate beyond what the user said and what you can verify. If you find nothing relevant, say so.

When research is done, summarise back to the user in 3–6 bullets: your current understanding of what the epic is and any anchors (existing systems, constraints) you have picked up. Ask them to correct any misunderstanding before continuing.

## Phase 3 — Create the initial draft early

Before deep interrogation, populate `.claude/specs/<slug>/spec.md` with a first-pass draft using the template below. Fill in what you confidently know from Phases 1 and 2. For sections you cannot yet fill, write `_TBD — to be resolved in Phase 4_` rather than guessing.

Creating the file early is deliberate: it gives both you and the user a shared artifact to read, point at, and update incrementally. Do not hold the spec in your head and dump it at the end.

Tell the user the file has been populated and invite them to read through it before the interrogation phase begins.

## Phase 4 — Deep interrogation

This is the main body of the command. Work through the spec section by section with the user, asking the kinds of questions that surface the high-level shape of the epic. Examples:

- **Purpose:** What is the epic for? What does success look like?
- **Capabilities:** What are the major things the system must do? What are the things it deliberately will not do?
- **Inputs and outputs:** What does the system consume? What does it produce? Where do those come from and go to?
- **System shape:** At a conceptual level, what are the major components or surfaces (e.g. a CLI, a service, a data pipeline, a UI)? What external systems does it talk to?
- **Data:** What are the core entities or domain concepts the system reasons about?
- **Constraints and assumptions:** Are there fixed technologies, environments, performance characteristics, security requirements, or organisational constraints?
- **Non-goals:** What is explicitly out of scope — not just for v1, but altogether?
- **Done-ness:** How will the user know the epic is in a usable state? What is the smallest viable version?

Do **not** ask about target users / personas — that section is intentionally omitted.

Ask one focused area at a time. After each answer, update the relevant section of `spec.md` immediately (using the Edit tool) so the document tracks the conversation. Then move to the next area.

Apply the following discipline throughout:

- If the user gives an answer that drifts into implementation detail or edge cases, gently steer back to the high-level intent and note the detail as something for the slice stage.
- If an answer expands scope, surface the expansion explicitly and ask whether it belongs in this epic or a later one.
- If you cannot resolve an ambiguity in conversation, capture it under "Open Questions" rather than guessing.
- Default to the simplest framing. If the user volunteers complexity that is not strictly required to describe the epic, ask whether it can be deferred.

Continue until the user agrees every section is in a state they are happy with, or has explicitly chosen to leave a question open.

## Phase 5 — Slice breakdown (plan.md)

Once `spec.md` is stable, create `.claude/specs/<slug>/plan.md` and derive the ordered slice checklist from the Major Capabilities. Apply these principles:

- **Granularity:** each slice should be small enough to be developed and shipped as a single PR. If a capability is large, split it.
- **Ordering:** sequence slices by dependency, and prefer vertical slices over horizontal layers. Foundational scaffolding comes first; after that, group work so a single coherent capability can be completed before the next begins. Only break this rule when a true cross-cutting dependency forces it.
- **High-level only:** describe each slice in one short sentence that captures *what* it delivers, not *how* it works. No class names, file paths, method signatures, or implementation details.
- **Complete coverage:** every capability described in the spec must be reachable by following the list. Do not omit capabilities or silently defer them.
- **Respect non-goals:** do not include slices the spec explicitly excludes.

Before writing into the file, present the proposed slice list to the user as a numbered list and ask:

1. Is the ordering correct?
2. Are any slices missing, or should any be merged or split?
3. Are any slices out of scope (i.e. described in the spec's non-goals)?

Incorporate their feedback, then write the agreed list into `plan.md` using the plan template below.

## Phase 6 — Final review

When the user signals they are done iterating:

1. Read both `spec.md` and `plan.md` end-to-end.
2. Check `spec.md` for internal consistency (e.g. a capability mentioned in one section is not contradicted in another), unfilled `_TBD_` markers, and any sections that still drift into implementation detail.
3. Confirm every Major Capability in `spec.md` is reachable from the slice list in `plan.md`, and that no slice in `plan.md` introduces capabilities not described in `spec.md`.
4. Surface any issues to the user and offer to fix them, or accept them as deliberate.
5. Do NOT commit, branch, or open a PR automatically. Tell the user the spec and plan are ready and let them decide what to do with them.

---

## Spec file template (`spec.md`)

Use this structure when creating `.claude/specs/<slug>/spec.md`. Section headings are fixed; content under each is filled in collaboratively.

```markdown
# [Epic Name]

## Overview

[2–4 sentences: what the epic is and what problem it solves.]

## Goals

[Bulleted list of the outcomes this epic is trying to achieve. Written from the user/business perspective, not technical.]

## Non-Goals

[Bulleted list of things this epic deliberately does not aim to do. As important as Goals — prevents scope drift.]

## Major Capabilities

[Bulleted list of the main things the system can do, at a high level. Each item should be a coherent capability, not a single screen or function. Detail belongs in the slices.]

## System Shape

[Conceptual description of the major components, surfaces, and external integrations. No implementation detail — just the shape: e.g. "a CLI tool that reads from X and writes to Y", "a web UI backed by a service that talks to a database and an LLM provider".]

## Core Domain Concepts

[The primary entities, objects, or concepts the system reasons about. One line each.]

## Constraints and Assumptions

[Fixed technologies, environments, performance expectations, security/compliance requirements, organisational constraints, or assumptions the epic depends on.]

## Definition of Done

[What does it mean for this epic to be in a usable / shippable state? What is the smallest viable version?]

## Open Questions

[Unresolved items the user has chosen to defer. Omit the section if none.]
```

## Plan file template (`plan.md`)

Use this structure when creating `.claude/specs/<slug>/plan.md`. The plan is intentionally minimal: a short pointer back to the spec, then the ordered slice checklist.

```markdown
# [Epic Name] — Delivery Plan

Companion to [`spec.md`](./spec.md). Each slice below is sized to ship as a single PR and described at a "what is delivered" level only — not how it is built.

## Slices

- [ ] **[short name]** — [one sentence describing what is delivered]
- [ ] **[short name]** — [one sentence describing what is delivered]
```

## Rules for spec content

- Stay high-level. If you find yourself describing how something is built, stop and rewrite at the level of what it does.
- Never invent detail the user did not give you. If a section cannot be filled, mark it `_TBD_` and resolve it through conversation.
- Do not include implementation details, library choices, code structure, or edge-case handling.
- Do not include time estimates, milestones, or roadmaps unless the user explicitly asks for them.
- Every Major Capability should be self-contained enough that a future feature spec could be written against it.
- Update the file as the conversation progresses. Do not let the file drift behind the conversation.

## Rules for plan.md

- The slice list is an ordered checklist. Items at the top must be completable before items below them.
- Each entry is one line: a checkbox, a bolded short name, an em dash, and a single sentence.
- The short name should be meaningful enough that the user can identify the slice at a glance (e.g. "CLI scaffolding", "Seeded RNG", "T-SQL script output").
- No section headings, sub-lists, or commentary beyond the pointer back to `spec.md` and the checklist itself.
- Do not add slices not derivable from `spec.md`.
- Mark a slice `[x]` only if `spec.md` explicitly states that capability already exists.
