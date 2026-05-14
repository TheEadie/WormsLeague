---
description: Run a retrospective on a completed epic — extract learnings from slice files and PR review activity, then update steering and component docs to prevent the same mistakes next time
effort: high
---

You coordinate a four-phase retrospective on a completed epic. The aim is to surface what the implementation agents got wrong (in slice review.md files, in PR comments, in follow-up commits after the agent review) and feed that back into the steering and component docs so the next epic does not repeat the same mistakes.

Phases 2, 3, and 4 each spawn sub-agents with clean contexts. Your job is to coordinate them, keep the epic-level `learnings.md` consistent, and confirm choices with the user where the inputs are ambiguous.

## Phase 1 — Identify the epic

If the user has named a specific epic, use that.

Otherwise, find the most recently completed epic:

1. List the epic directories under `.claude/specs/` (each is a directory containing `spec.md` and `plan.md`).
2. For each epic, read `plan.md` and treat it as complete if every slice line is checked (`- [x]`) and no `- [ ]` remains. Skip incomplete epics.
3. Among complete epics, pick the most recently finished by mtime of the latest file under `slices/` (typically the last `review.md`).
4. Present the chosen epic to the user — its slug and the number of slices — and ask them to confirm or pick a different one.

Do not proceed until the user has confirmed the target epic. Record the epic's full directory path (e.g. `.claude/specs/<epic-slug>`).

If `<epic-path>/learnings.md` already exists, tell the user it will be overwritten and confirm before continuing.

## Phase 2 — Extract learnings from slice files

Spawn one sub-agent with this prompt, substituting `<epic-path>`:

> You are running Phase 2 of an epic retrospective. The epic directory is `<epic-path>`.
>
> Read every slice's `learnings.md` and `review.md` under `<epic-path>/slices/`. Across all of them, identify:
>
> - Common patterns of mistakes the implementation agent made (things flagged in review.md Blockers/Suggestions, or things the implementer had to fix mid-flight as recorded in learnings.md).
> - Recurring scope or plan deviations that point at a missing instruction in the process.
> - Repeated gaps in test coverage, CI wiring, or guideline adherence.
> - Any single-occurrence issue that is severe enough or general enough to be worth preventing next time.
>
> Do NOT include:
> - Issues that were one-off implementation details with no broader lesson.
> - Praise or things that went well — this retro is about what to change.
>
> Write the results to `<epic-path>/learnings.md` using this structure:
>
> ```markdown
> # Retrospective — <epic name>
>
> ## From slice files
>
> ### L1 — <short title>
>
> - **Pattern:** <one or two sentences describing what kept going wrong>
> - **Evidence:** <slice dir>/review.md (B1, S2), <slice dir>/learnings.md
> - **Where to fix it:** <which steering or component doc should change, and what the change should say>
>
> ### L2 — …
> ```
>
> Number entries L1, L2, L3, … in document order. For each, the "Where to fix it" line should name the specific doc under `.claude/docs/` (steering or component) most likely to prevent recurrence, and describe in one sentence what new guidance would have prevented the mistake.
>
> Do not modify any other file. Do not edit code or docs. Only write `<epic-path>/learnings.md`.

After the agent completes, verify `<epic-path>/learnings.md` exists and contains at least one numbered entry. If it does not, stop and report the failure to the user.

## Phase 3 — Extract learnings from GitHub PR activity

### 3a — Determine the candidate PR list

Find the date window in which the epic was delivered:

- **Start:** the earliest mtime among `<epic-path>/slices/*/spec.md`.
- **End:** the latest mtime among `<epic-path>/slices/*/review.md`.

List PRs merged into `main` during that window:

```
gh pr list --base main --state merged --limit 100 \
  --search "merged:<start-date>..<end-date>" \
  --json number,title,headRefName,mergedAt
```

Present the list to the user and ask them to confirm which PRs belong to this epic. Renovate, unrelated refactors, and other concurrent work will appear in the window and must be filtered out. Wait for the user's confirmed list before continuing.

### 3b — Spawn one sub-agent per confirmed PR

For each PR number `N` in the confirmed list, spawn a sub-agent **in parallel** with this prompt, substituting `<N>` and `<epic-path>`:

> You are running Phase 3 of an epic retrospective for PR #<N>. The epic learnings file lives at `<epic-path>/learnings.md`.
>
> Gather PR activity:
>
> ```
> gh pr view <N> --json number,title,body,mergedAt,reviews,comments,commits
> gh api repos/{owner}/{repo}/pulls/<N>/comments  # inline review comments
> ```
>
> (Use `gh repo view --json owner,name` to resolve `{owner}` and `{repo}` if needed.)
>
> Identify two classes of signal:
>
> 1. **Human review comments** that asked for changes — what the reviewer caught that the agent's own review.md missed.
> 2. **Commits made on the PR branch AFTER the agent's review.md was written** (i.e. follow-up fixes pushed in response to human review or post-review testing). The `<epic-path>/slices/*/review.md` mtime gives a reasonable cutoff; for commits, use the timestamp on the commit itself versus the PR's review.md.
>
> For each genuine learning — something the implementation or agent-review process should have caught but didn't — append a new numbered entry to `<epic-path>/learnings.md` under a section header `## From PR #<N>` (create the section if it does not exist). Use the same entry format as Phase 2:
>
> ```markdown
> ### P<N>.1 — <short title>
>
> - **Pattern:** <one or two sentences>
> - **Evidence:** PR #<N> review comment by <reviewer>, follow-up commit <sha-short>
> - **Where to fix it:** <which doc, what change>
> ```
>
> Skip noise: typo fixes, formatting churn, comments that are just questions, and merge-conflict resolution commits do not produce learnings. Only record things that point at a process or guidance gap.
>
> Append to `<epic-path>/learnings.md`. Do not modify any other file. Do not edit code or docs.

After all PR agents complete, read `<epic-path>/learnings.md` and skim it for duplicates across sections. If multiple entries describe the same underlying pattern, consolidate them into a single entry that cites all the evidence — keep the entry numbering stable within each section.

## Phase 4 — Update steering and component docs

Spawn one sub-agent with this prompt, substituting `<epic-path>`:

> You are running Phase 4 of an epic retrospective. The learnings live at `<epic-path>/learnings.md`.
>
> For each numbered learning entry, follow its **Where to fix it** line and make the smallest edit to the named doc under `.claude/docs/` that would prevent the mistake from recurring. If the named doc is wrong or the guidance fits better elsewhere, pick the more appropriate doc — but explain why in your final report.
>
> Rules:
>
> - Edit existing docs only. Do not create new files unless an entry has no plausible home and the user is told.
> - Steering docs (`.claude/docs/steering/*.md`) must remain high-level — no file paths, line numbers, or version numbers (see `.claude/commands/update-docs.md` for the full rules; respect them).
> - Component docs may reference type names but not line numbers.
> - Match the tone and structure of the existing docs. Add the new guidance where a reader would expect it, not as a tacked-on "lessons learned" section.
> - Do not reference PR numbers, commit hashes, slice numbers, or the retrospective itself in the docs. The docs should read as durable guidance, not as a changelog.
> - If two learnings would produce contradictory or redundant guidance, reconcile them into one coherent edit and note that in your report.
>
> When done, report:
>
> - A bullet list of doc files changed and the substantive change in each (one line per file).
> - Any learning you deliberately did NOT act on, with a one-line reason.
> - Any learning where the guidance belongs somewhere outside `.claude/docs/` (e.g. a command file under `.claude/commands/`, a makefile, or CI config) — flag these for the user, do not edit them yourself.
>
> Do not commit. The user reviews and commits.

After the agent completes, surface its report to the user verbatim.

## Phase 5 — Hand off

Tell the user:

- Where the epic `learnings.md` lives.
- How many docs were updated and the headline of each change.
- Any learnings the Phase 4 agent flagged as needing edits outside `.claude/docs/` — these are your follow-ups.
- Next step: review the changes and commit when satisfied.

Do not commit or push — the user triggers that.
