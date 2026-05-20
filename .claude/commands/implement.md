---
description: Orchestrate planning, implementation, review, and interactive finding resolution for a slice issue
effort: medium
---

You coordinate the full slice workflow: plan → implement → review → resolve. The first three phases each run in a separate sub-agent (`slice-planner`, `slice-implementer`, `slice-reviewer`) with a clean context window — each agent's frontmatter pins the model it runs on. The resolution phase runs interactively in this session. Your role is to identify the slice issue, confirm with the user which phases need to run, then coordinate each phase in turn.

## Step 1 — Identify the slice issue

Scan the user's request for a GitHub issue reference (a full GitHub issue URL, or a `#NNN` token). If none is present, stop and ask the user which issue to work on. Do not proceed without an explicit issue reference.

Fetch the issue and inspect its sticky comments (see `.claude/docs/sticky-comments.md`) to determine which phases need to run:

- Issue body exists (spec) but no `plan` sticky → planning + implementing + reviewing
- `plan` sticky exists but no `learnings` sticky → implementing + reviewing
- `learnings` sticky exists but no `review` sticky → reviewing only
- All three stickies exist → only Step 5 (interactive resolution) runs

If the issue body is empty or still looks like the one-sentence stub created by `/epic`, stop and tell the user to run `/spec` against it first.

Record the issue URL and number as `<issue>` for use below. Present the planned phase list to the user and ask them to confirm before proceeding.

## Step 2 — Plan phase

Skip if the `plan` sticky comment already exists on the issue.

Dispatch the `slice-planner` agent (via the Agent tool with `subagent_type: "slice-planner"`), passing the issue URL/number `<issue>` in the prompt.

After the agent completes, verify the `plan` sticky comment now exists on the issue. If it does not, stop and report the failure to the user.

## Step 3 — Implement phase

Skip if the `learnings` sticky comment already exists on the issue.

Dispatch the `slice-implementer` agent (via the Agent tool with `subagent_type: "slice-implementer"`), passing the issue URL/number `<issue>` in the prompt.

After the agent completes, verify the `learnings` sticky comment now exists on the issue. If it does not, stop and report the failure to the user.

## Step 4 — Review phase

Skip if the `review` sticky comment already exists on the issue.

Dispatch the `slice-reviewer` agent (via the Agent tool with `subagent_type: "slice-reviewer"`), passing the issue URL/number `<issue>` in the prompt.

After the agent completes, verify the `review` sticky comment now exists on the issue. If it does not, stop and report the failure to the user.

## Step 5 — Resolve findings interactively

Read the `review` sticky comment on the issue (see `.claude/docs/sticky-comments.md`) and collect all findings in document order: Blockers (B1, B2, …), then Suggestions (S1, S2, …), then Nitpicks (N1, N2, …). Skip any finding whose `**Decision:**` line is already set to `Accept` or `Decline`.

For each remaining finding, look at the referenced file to make the proposed fix concrete.

Present all findings at once as a single table:

| ID | Severity | Title | File | Issue | Proposed Fix |
|----|----------|-------|------|-------|--------------|
| B1 | Blocker | … | `path/to/file:line` | … | … |
| S1 | Suggestion | … | `path/to/file:line` | … | … |
| N1 | Nitpick | … | `path/to/file:line` | … | … |

Then ask: **"Reply with the IDs you want resolved (e.g. `B1 S2`), the IDs you want ignored, or `all` to resolve everything. Any ID not mentioned will be skipped."**

Wait for a single reply, then apply all accepted fixes in one batch. After all changes are made, update the `review` sticky comment in place:

- Update each accepted finding's `**Decision:**` line to `Accept`.
- Update each declined finding's `**Decision:**` line to `Decline`.
- Leave skipped findings as `*(pending)*`.

To edit in place: read the current body of the `review` sticky comment, apply the line changes, and write the full updated body back to the same comment via the create-or-update flow in `.claude/docs/sticky-comments.md`. Do not append a second `review` sticky.

## Step 6 — Hand off

Tell the user:
- How many findings were resolved, ignored, and skipped
- Whether any unresolved Blockers remain (they must be addressed before merging)
- Next step: run `/pr` to create the pull request

Do not commit, push, or open a PR — the user triggers that.
