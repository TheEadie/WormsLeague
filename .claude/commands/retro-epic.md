---
description: Run a retrospective on a completed epic — extract learnings from each slice's sticky comments and PR review activity, then update steering and component docs to prevent the same mistakes next time
effort: high
---

You coordinate a four-phase retrospective on a completed epic. The aim is to surface what the implementation agents got wrong (in slice `review` and `learnings` sticky comments, in PR comments, in follow-up commits after the agent review) and feed that back into the steering and component docs so the next epic does not repeat the same mistakes.

Phases 2, 3, and 4 each spawn sub-agents with clean contexts. Your job is to coordinate them, keep the `retrospective` sticky comment on the parent epic issue consistent, and confirm choices with the user where the inputs are ambiguous.

## Phase 1 — Identify the epic issue

Scan the user's request for a GitHub issue reference to the parent epic (a full URL or `#NNN`). If none is present, stop and ask the user which epic issue this retro is for. Do not proceed without an explicit reference.

Fetch the epic issue and its sub-issues:

```bash
REPO=$(gh repo view --json nameWithOwner -q .nameWithOwner)
PARENT=<epic-issue-number>
gh issue view $PARENT --json number,title,body,state,url
gh api "repos/$REPO/issues/$PARENT/sub_issues" --jq '.[] | {number, title, state, closedAt}'
```

Treat the epic as complete only if every sub-issue is in state `CLOSED`. If any remain open, list them to the user and ask whether to proceed anyway or to stop.

If a `retrospective` sticky comment already exists on the epic issue, tell the user it will be overwritten and confirm before continuing.

Record the parent issue number as `<parent>` and the ordered list of sub-issue numbers as `<sub-issues>` for later phases.

## Phase 2 — Extract learnings from slice sticky comments

Spawn one sub-agent with this prompt, substituting `<parent>` and `<sub-issues>`:

> You are running Phase 2 of an epic retrospective. The parent epic issue is `#<parent>`. Its sub-issues are: `<sub-issues>`.
>
> For each sub-issue, read its `learnings` and `review` sticky comments (see `.claude/docs/sticky-comments.md`). To fetch them:
>
> ```bash
> REPO=$(gh repo view --json nameWithOwner -q .nameWithOwner)
> gh api "repos/$REPO/issues/<sub>/comments" --paginate \
>   --jq '.[] | select(.body | startswith("<!-- claude:sticky:")) | {body, url}'
> ```
>
> Across all sub-issues, identify:
>
> - Common patterns of mistakes the implementation agent made (things flagged in the `review` sticky's Blockers/Suggestions, or things the implementer had to fix mid-flight as recorded in the `learnings` sticky).
> - Recurring scope or plan deviations that point at a missing instruction in the process.
> - Repeated gaps in test coverage, CI wiring, or guideline adherence.
> - Any single-occurrence issue that is severe enough or general enough to be worth preventing next time.
>
> Do NOT include:
>
> - Issues that were one-off implementation details with no broader lesson.
> - Praise or things that went well — this retro is about what to change.
>
> Write the results to a temp file `/tmp/sticky-retrospective.md` with this structure:
>
> ```markdown
> <!-- claude:sticky:retrospective -->
>
> # Retrospective — <epic title>
>
> ## From slice issues
>
> ### L1 — <short title>
>
> - **Pattern:** <one or two sentences describing what kept going wrong>
> - **Evidence:** issue #<n> review sticky (B1, S2), issue #<m> learnings sticky
> - **Where to fix it:** <which steering or component doc should change, and what the change should say>
>
> ### L2 — …
> ```
>
> Number entries L1, L2, L3, … in document order. For each, the "Where to fix it" line should name the specific doc under `.claude/docs/` (steering or component) most likely to prevent recurrence, and describe in one sentence what new guidance would have prevented the mistake.
>
> Then create or update the `retrospective` sticky comment on issue #`<parent>` using the create-or-update flow in `.claude/docs/sticky-comments.md`, with `/tmp/sticky-retrospective.md` as the body.
>
> Do not modify any other file or comment. Do not edit code or docs.

After the agent completes, fetch the `retrospective` sticky comment on the parent issue and verify it contains at least one numbered entry. If it does not, stop and report the failure to the user.

## Phase 3 — Extract learnings from GitHub PR activity

### 3a — Determine the candidate PR list

Find the date window in which the epic was delivered using the sub-issue timestamps:

- **Start:** the earliest `createdAt` among the sub-issues.
- **End:** the latest `closedAt` among the sub-issues.

```bash
gh api "repos/$REPO/issues/$PARENT/sub_issues" \
  --jq 'map({number, createdAt, closedAt}) | sort_by(.createdAt)'
```

List PRs merged into `main` during that window:

```bash
gh pr list --base main --state merged --limit 100 \
  --search "merged:<start-date>..<end-date>" \
  --json number,title,headRefName,mergedAt
```

Present the list to the user and ask them to confirm which PRs belong to this epic. Renovate, unrelated refactors, and other concurrent work will appear in the window and must be filtered out. Wait for the user's confirmed list before continuing.

### 3b — Spawn one sub-agent per confirmed PR

For each PR number `<N>` in the confirmed list, spawn a sub-agent **in parallel** with this prompt, substituting `<N>` and `<parent>`:

> You are running Phase 3 of an epic retrospective for PR #`<N>`. The retrospective lives in the `retrospective` sticky comment on epic issue #`<parent>`.
>
> Gather PR activity:
>
> ```bash
> gh pr view <N> --json number,title,body,mergedAt,reviews,comments,commits
> gh api repos/{owner}/{repo}/pulls/<N>/comments  # inline review comments
> ```
>
> (Use `gh repo view --json nameWithOwner` to resolve `{owner}/{repo}` if needed.)
>
> Identify two classes of signal:
>
> 1. **Human review comments** that asked for changes — what the reviewer caught that the agent's own `review` sticky missed.
> 2. **Commits made on the PR branch AFTER the agent's `review` sticky was written** (i.e. follow-up fixes pushed in response to human review or post-review testing). The `review` sticky's `created_at`/`updated_at` (returned by `gh api repos/{owner}/{repo}/issues/<sub>/comments`) gives the cutoff to compare commit timestamps against.
>
> For each genuine learning — something the implementation or agent-review process should have caught but didn't — read the current `retrospective` sticky body, append a new numbered entry under a section header `## From PR #<N>` (create the section if it does not exist), and write the updated body back via the create-or-update flow. Use the same entry format as Phase 2:
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
> Concurrency: multiple Phase 3 agents may update the same sticky in parallel. Re-read the current body immediately before each write so you do not overwrite a sibling's section.
>
> Do not modify any other comment or file. Do not edit code or docs.

After all PR agents complete, fetch the `retrospective` sticky comment and skim it for duplicates across sections. If multiple entries describe the same underlying pattern, consolidate them into a single entry that cites all the evidence — keep the entry numbering stable within each section. Write the consolidated body back to the same `retrospective` sticky.

## Phase 4 — Update steering and component docs

Spawn one sub-agent with this prompt, substituting `<parent>`:

> You are running Phase 4 of an epic retrospective. The learnings live in the `retrospective` sticky comment on epic issue #`<parent>`. Fetch it via the read flow in `.claude/docs/sticky-comments.md`.
>
> For each numbered learning entry, follow its **Where to fix it** line and make the smallest edit to the named doc under `.claude/docs/` that would prevent the mistake from recurring. If the named doc is wrong or the guidance fits better elsewhere, pick the more appropriate doc — but explain why in your final report.
>
> Rules:
>
> - Edit existing docs only. Do not create new files unless an entry has no plausible home and the user is told.
> - Steering docs (`.claude/docs/steering/*.md`) must remain high-level — no file paths, line numbers, or version numbers (see `.claude/commands/update-docs.md` for the full rules; respect them).
> - Component docs may reference type names but not line numbers.
> - Match the tone and structure of the existing docs. Add the new guidance where a reader would expect it, not as a tacked-on "lessons learned" section.
> - Do not reference PR numbers, commit hashes, sub-issue numbers, or the retrospective itself in the docs. The docs should read as durable guidance, not as a changelog.
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

- The epic issue URL (the `retrospective` sticky comment is where the learnings live).
- How many docs were updated and the headline of each change.
- Any learnings the Phase 4 agent flagged as needing edits outside `.claude/docs/` — these are your follow-ups.
- Next step: review the changes and commit when satisfied.

Do not commit or push — the user triggers that.
