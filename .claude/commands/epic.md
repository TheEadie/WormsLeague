---
description: Turn a large feature description into a high-level epic spec on a GitHub issue, with a slice breakdown as GitHub sub-issues
effort: high
---

Your task is to collaborate with the user to produce a high-level epic on GitHub:

- A **parent issue** whose body holds the project-level specification (purpose, scope, capabilities, shape).
- One **sub-issue per slice** in the delivery plan, linked to the parent via GitHub's native sub-issue relationship.

The parent issue body describes *what the epic is*; the ordered list of sub-issues describes *how it will be delivered as a sequence of PR-sized slices*. Neither prescribes implementation detail.

This is a multi-phase, conversational process. You and the user will iterate on the issue body together. Do not rush to a final draft; the value is in the conversation.

## Scope and tone

Keep both the parent body and the sub-issue bodies strictly high level.

The parent issue body is concerned with:

- What the epic is and why it exists
- The major capabilities and behaviours it provides
- The shape of the system at a conceptual level (major components, external integrations, data the system deals with)
- Constraints, assumptions, and non-goals

Each sub-issue is concerned with:

- A single **vertical tracer-bullet slice** that cuts end-to-end through every relevant layer, small enough to be a single PR, described at a "what is delivered" level

Nothing here is concerned with:

- Implementation details (class names, function signatures, file paths, code structure, libraries, frameworks unless the user has explicitly anchored on one)
- Edge cases, error handling specifics, validation rules
- Concerns that will naturally be refined when individual slices are picked up via `/spec`
- Target users / personas — deliberately omitted from this command

When in doubt, defer detail to the slice-implementation stage. Better to leave something coarse than to over-specify.

## YOU DO NOT IMPLEMENT THE EPIC

Your only outputs are the parent GitHub issue body and the slice sub-issues. Do not write source code, scaffolding, configuration, files under `.claude/specs/`, or any other artifact during this command.

---

## Phase 0 — Identify or plan to create the epic issue

Scan the user's invocation for a GitHub issue reference (a full GitHub issue URL, or a `#NNN` token).

- **If one is present**, fetch it with `gh issue view <number-or-url> --json number,title,body,url,labels` and use it as the parent epic issue. If the body is non-empty, treat its current contents as a starting draft to evolve (do not silently overwrite — confirm with the user that they want to evolve it before editing). Record the issue number and URL.
- **If none is present**, do not create the issue yet. Ask the user for a one-paragraph description of the epic, and tell them you will create the parent issue once there is a first-pass draft to put in it (Phase 3). Record the issue title you intend to use — propose one based on the description and confirm.

Resolve `{owner}/{repo}` once for use in later `gh api` calls: `gh repo view --json nameWithOwner -q .nameWithOwner`.

## Phase 1 — Elicit the initial description

Ask the user to describe the epic they want to build, with prompts like:

- What is the epic, in a sentence or two?
- What problem does it solve?
- Is there an existing system, repo, or document this builds on or replaces?
- Are there any links, issues, or reference materials you should read first?

Listen. Do not start probing for detail yet. The goal is to get the user's framing into the conversation.

## Phase 2 — Research and orient

Read whatever you need to understand the context. Depending on the user's answers, this may include:

- Files in the current working directory (`CLAUDE.md`, `README.md`)
- Source files, if the epic extends or relates to existing code
- Existing GitHub issues (`gh issue list`, `gh issue view <n>`), linked documents, or external references the user pointed to

Do NOT speculate beyond what the user said and what you can verify. If you find nothing relevant, say so.

When research is done, summarise back to the user in 3–6 bullets: your current understanding of what the epic is and any anchors (existing systems, constraints) you have picked up. Ask them to correct any misunderstanding before continuing.

## Phase 3 — Create the initial draft early

Before deep interrogation, render a first-pass draft using the parent-issue template below. Fill in what you confidently know from Phases 1 and 2. For sections you cannot yet fill, write `_TBD — to be resolved in Phase 4_` rather than guessing.

Write the draft to a temp file (e.g. `/tmp/epic-body.md`), then:

- **If the parent issue already exists** (the user supplied one in Phase 0): overwrite the body —

  ```bash
  gh issue edit <number> --body-file /tmp/epic-body.md
  ```

- **If no parent issue exists yet**: create it now —

  ```bash
  gh issue create --title "<epic title>" --body-file /tmp/epic-body.md
  ```

  Capture the URL it prints and parse the issue number from the trailing path segment.

Always use `--body-file` so multi-line content and special characters survive intact.

Creating the issue early is deliberate: it gives both you and the user a shared artifact to read, point at, and update incrementally. Do not hold the spec in your head and dump it at the end.

Tell the user the issue has been created/updated, share the URL, and invite them to read through it before the interrogation phase begins.

## Phase 4 — Deep interrogation

This is the main body of the command. Interview the user relentlessly about every aspect of the epic until you reach a shared understanding. Walk down each branch of the design tree, resolving dependencies between decisions one-by-one.

**How to ask:**

- Ask questions **one at a time** — never bundle multiple questions into a single turn. Wait for the answer before moving on.
- For each question, **provide your recommended answer** along with the question, so the user can react to a concrete proposal rather than starting from a blank slate. Explain briefly why you recommend it.
- If a question can be answered by **exploring the codebase**, explore it instead of asking the user. Only ask when the answer genuinely requires the user's intent or knowledge.
- After each answer, update the relevant section of the parent issue body immediately so the issue tracks the conversation, then move to the next question. Re-render the full body to the temp file and run `gh issue edit <number> --body-file /tmp/epic-body.md` — GitHub's CLI cannot patch a single section.

Work through the spec section by section, surfacing the high-level shape of the epic. The areas to cover:

- **Purpose:** What is the epic for? What does success look like?
- **Capabilities:** What are the major things the system must do? What are the things it deliberately will not do?
- **Inputs and outputs:** What does the system consume? What does it produce? Where do those come from and go to?
- **System shape:** At a conceptual level, what are the major components or surfaces (e.g. a CLI, a service, a data pipeline, a UI)? What external systems does it talk to?
- **Data:** What are the core entities or domain concepts the system reasons about?
- **Constraints and assumptions:** Are there fixed technologies, environments, performance characteristics, security requirements, or organisational constraints?
- **Non-goals:** What is explicitly out of scope — not just for v1, but altogether?
- **Done-ness:** How will the user know the epic is in a usable state? What is the smallest viable version?

Do **not** ask about target users / personas — that section is intentionally omitted.

Apply the following discipline throughout:

- If the user gives an answer that drifts into implementation detail or edge cases, gently steer back to the high-level intent and note the detail as something for the slice stage.
- If an answer expands scope, surface the expansion explicitly and ask whether it belongs in this epic or a later one.
- If you cannot resolve an ambiguity in conversation, capture it under "Open Questions" rather than guessing.
- Default to the simplest framing. If the user volunteers complexity that is not strictly required to describe the epic, ask whether it can be deferred.

Continue until the user agrees every section is in a state they are happy with, or has explicitly chosen to leave a question open.

## Phase 5 — Slice breakdown as sub-issues

Once the parent issue body is stable, derive the ordered slice list from the Major Capabilities.

Break the plan into **tracer bullet** slices. Each slice is a thin **vertical** cut that goes through ALL integration layers end-to-end — NOT a horizontal slice of one layer.

- Each slice delivers a narrow but COMPLETE path through every relevant layer (e.g. schema, API, UI, tests — whichever layers this epic has).
- A completed slice is demoable or verifiable on its own.
- Prefer many thin slices over few thick ones.
- Do not produce slices like "build the database schema", "build the API", "build the UI" — those are horizontal layers. Instead, produce slices like "user can create and view a single foo end-to-end", which forces a sliver of schema + API + UI + tests in one PR.

Apply these additional principles:

- **Granularity:** each slice should be small enough to be developed and shipped as a single PR. If a capability is large, split it into multiple vertical slices (e.g. by entity, by sub-capability, by happy-path vs edge case) — never by layer.
- **Ordering:** sequence slices by dependency. Foundational scaffolding that is genuinely cross-cutting (e.g. project skeleton, CI) may come first, but keep it minimal — push as much as possible into the vertical slices themselves.
- **High-level only:** describe each slice in one short sentence that captures *what* it delivers end-to-end, not *how* it works. No class names, file paths, method signatures, or implementation details.
- **Complete coverage:** every capability described in the parent issue must be reachable by following the list. Do not omit capabilities or silently defer them.
- **Respect non-goals:** do not include slices the parent issue explicitly excludes.

Before creating any sub-issues, present the proposed slice list to the user as a numbered list and ask:

1. Is the ordering correct?
2. Are any slices missing, or should any be merged or split?
3. Is any slice secretly a horizontal layer (e.g. "build the schema", "build the API") that needs reshaping into a vertical end-to-end cut?
4. Are any slices out of scope (i.e. described in the parent issue's non-goals)?

Incorporate their feedback. Then, **in the agreed order**, create one sub-issue per slice and link it to the parent using the GitHub sub-issues API:

```bash
REPO=$(gh repo view --json nameWithOwner -q .nameWithOwner)
PARENT=<parent-issue-number>

# For each slice, in order:
CHILD_URL=$(gh issue create \
  --title "<slice short name>" \
  --body-file /tmp/slice-body.md)
CHILD_NUMBER=${CHILD_URL##*/}

# Get the REST database id for the child (the sub-issues endpoint takes the id, not the number)
CHILD_ID=$(gh api "repos/$REPO/issues/$CHILD_NUMBER" --jq .id)

# Link as a native sub-issue under the parent
gh api -X POST "repos/$REPO/issues/$PARENT/sub_issues" -F sub_issue_id=$CHILD_ID
```

Use the sub-issue body template below — one sentence describing what is delivered end-to-end, plus a back-pointer to the parent. The sub-issue body is intentionally minimal; running `/spec #<sub-issue-number>` later will overwrite it with a full slice spec.

If the `sub_issues` POST fails (for example, the repository has not enabled the sub-issues feature), stop and report this to the user rather than falling back to a checklist in the parent body — surfacing the failure is more useful than silently degrading.

After all sub-issues are created, tell the user the slice list is in place and share the parent issue URL so they can verify the sub-issue panel renders correctly on GitHub.

## Phase 6 — Final review

When the user signals they are done iterating:

1. Re-fetch the parent issue (`gh issue view <number> --json body`) and read it end-to-end.
2. List the sub-issues attached to the parent (`gh api "repos/$REPO/issues/$PARENT/sub_issues" --jq '.[] | {number, title}'`) and confirm:
   - Every Major Capability in the parent body is reachable from at least one sub-issue.
   - No sub-issue introduces a capability not described in the parent body.
   - The sub-issues are in dependency order.
3. Check the parent body for internal consistency, unfilled `_TBD_` markers, and any sections that drift into implementation detail.
4. Surface any issues to the user and offer to fix them, or accept them as deliberate.
5. Do NOT commit, branch, or open a PR. Tell the user the epic and slice sub-issues are ready and let them decide what to do next (typically `/spec #<sub-issue-number>` on the first slice).

---

## Parent issue body template

Use this structure when writing the parent issue body. Section headings are fixed; content under each is filled in collaboratively.

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

## Sub-issue body template

Each slice sub-issue starts as a one-sentence stub. The body will be overwritten with a full spec by `/spec` later — this is intentional.

```markdown
[One sentence describing what this slice delivers end-to-end.]
```

Do **not** add a `Part of #<parent>` line — the parent/child link is established by the `sub_issues` API call and is visible in GitHub's sub-issues panel; downstream commands query that relationship via GraphQL rather than parsing the body.

The sub-issue **title** is the slice's short name (meaningful enough that the user can identify it at a glance and that conveys the end-to-end capability it delivers — e.g. "Create-and-view foo", "Seeded RNG produces reproducible run", "Slack announce on game end"). It is not prefixed with a number; ordering is conveyed by sub-issue creation order and by the GitHub sub-issues panel on the parent.

## Rules for the parent issue body

- Stay high-level. If you find yourself describing how something is built, stop and rewrite at the level of what it does.
- Never invent detail the user did not give you. If a section cannot be filled, mark it `_TBD_` and resolve it through conversation.
- Do not include implementation details, library choices, code structure, or edge-case handling.
- Do not include time estimates, milestones, or roadmaps unless the user explicitly asks for them.
- Every Major Capability should be self-contained enough that a future slice spec could be written against it.
- Re-render the full body to the temp file and `gh issue edit --body-file` on each update. Do not let the issue drift behind the conversation.

## Rules for sub-issues

- Each sub-issue is a vertical tracer bullet — a thin end-to-end cut through every relevant layer, demoable on its own.
- Reject any slice whose title or description implies a single horizontal layer (e.g. "database schema", "API endpoints", "UI components"). Reshape it into vertical end-to-end slices.
- Each sub-issue body is exactly one sentence. No section headings, sub-lists, back-pointer lines, or other commentary — the parent link is the `sub_issues` API call, not text in the body.
- Do not add sub-issues not derivable from the parent body.
- Do not close sub-issues during `/epic`. Slices are closed when their PR merges, not when their spec is drafted.
- Create sub-issues in dependency order: every issue created earlier must be completable before issues created later.
