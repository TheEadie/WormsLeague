---
description: Orchestrate planning, implementation, an automated review-fix loop, and interactive resolution of any leftovers for a slice issue
effort: medium
---

You coordinate the full slice workflow: plan → implement → (review ↔ fix)* → resolve. The plan and implement phases run in their own sub-agents (`slice-planner`, `slice-implementer`). The review-fix loop dispatches `reviewer-spec`, `reviewer-csharp`, and `reviewer-react` in parallel, merges their findings inline, then alternates with `slice-fixer` until the slice converges — no Blockers and no Accept-recommended Suggestions remain — or a hard cap is hit. The resolution phase then runs interactively in this session for whatever's left (Nitpicks, Declines, and any auto-fixable findings the cap cut off). Each sub-agent's frontmatter pins the model it runs on.

## Step 1 — Identify the slice issue

Scan the user's request for a GitHub issue reference (a full GitHub issue URL, or a `#NNN` token). If none is present, stop and ask the user which issue to work on. Do not proceed without an explicit issue reference.

Fetch the issue and inspect its sticky comments (see `.claude/docs/sticky-comments.md`) to determine which phases need to run:

- Issue body exists (spec) but no `plan` sticky → planning + implementing + review-fix loop + interactive resolution
- `plan` sticky exists but no `learnings` sticky → implementing + review-fix loop + interactive resolution
- `learnings` sticky exists but no `review` sticky → review-fix loop + interactive resolution
- All three stickies exist → only Step 5 (interactive resolution) runs against the existing `review` sticky

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

## Step 4 — Review-and-fix loop

Skip this entire step if the `review` sticky comment already exists on the issue.

You will run up to **3 total review iterations** (one initial review + up to 2 fix-then-rereview passes). The loop exits as soon as the latest review contains no auto-fixable findings, or when the iteration cap is hit. Track the iteration count explicitly.

### File classification

Determine the base branch (default `main`) and current branch. Run:

```bash
git diff --name-only <base>...<current-branch>
```

Classify the results:

- **C# files** — `*.cs`, `*.csproj`, `*.razor`, `*.sln`, `Directory.*.props`/`targets`, anywhere under `src/` outside `src/Worms.Hub.Web/`.
- **Web files** — anything under `src/Worms.Hub.Web/`.
- **Other** — migrations, Docker, infra, docs, CI workflow files.

Also note which component(s) the C# files belong to (map: `Worms.Cli*` → `cli`; `Worms.Hub.Gateway` → `hub-gateway`; `Worms.Hub.Storage` → `hub-storage`; `Worms.Hub.Queues` → `hub-queues`; `Worms.Hub.Armageddon.Runner*` → `wa-runner`; `Worms.Armageddon.Files*` → `armageddon-files`; `Worms.Armageddon.Game*` → `armageddon-game`; `Worms.Armageddon.Gifs*` → `armageddon-gifs`; infrastructure code → `infrastructure`).

Record the base branch, current branch, C# file list, web file list, and component list — reuse these in every review iteration without re-running the diff.

### Iteration 1 — initial review

Dispatch reviewer sub-agents in a **single message** (parallel):

1. **Always** spawn `reviewer-spec` (via the Agent tool with `subagent_type: "reviewer-spec"`) with: the GitHub issue URL, the base branch, and the current branch.
2. **If any C# files changed**, spawn `reviewer-csharp` (via the Agent tool with `subagent_type: "reviewer-csharp"`) with: the base branch, the current branch, the C# file list, and the component(s).
3. **If any web files changed**, spawn `reviewer-react` (via the Agent tool with `subagent_type: "reviewer-react"`) with: the base branch, the current branch, and the web file list.

After all sub-agents have returned, merge their findings into the `review` sticky comment using the template below. Preserve each sub-agent's findings verbatim within its section — do not rerank across axes. Renumber findings only if needed to keep IDs unique (e.g. `Spec B1`, `C# B1`, `Web B1` are fine as-is). Write a one-paragraph verdict that summarises both axes honestly: a Spec-pass / Standards-fail reads differently from the reverse.

Render the merged review to `/tmp/sticky-review.md` with `<!-- claude:sticky:review -->` as the first line, then create or update the `review` sticky comment on the issue using the flow in `.claude/docs/sticky-comments.md`.

```markdown
<!-- claude:sticky:review -->

# Review — [Slice Name]

## Verdict

[One paragraph. Summarise both axes: does the implementation satisfy the spec? Does it follow the standards? Any blockers the user must address before merging?]

## Spec

[Verbatim from reviewer-spec: Acceptance Criteria table, Blockers, Suggestions, Nitpicks. If a sub-section is empty, write "None".]

## C# Standards

[Verbatim from reviewer-csharp, including the build PASS/FAIL line. Omit this whole section if reviewer-csharp was not dispatched.]

## Web Standards

[Verbatim from reviewer-react, including the lint PASS/FAIL line. Omit this whole section if reviewer-react was not dispatched.]

## Recommended Actions

[For every finding across all axes, state your recommended action and a one-line reason. Reference findings by axis-prefixed ID:]

- **Spec B1** — Accept — [reason]
- **C# B1** — Accept — [reason]
- **C# S1** — Decline — [reason]
- **Web N1** — Accept — [reason]

Valid actions: `Accept` or `Decline`. Cover every finding.
```

Verify the `review` sticky comment now exists. If it does not, stop and report the failure to the user.

### Loop

Repeat the following until either the auto-fixable list is empty or you have completed 3 review iterations:

1. **Read the latest `review` sticky comment** (see `.claude/docs/sticky-comments.md`) and parse its `## Recommended Actions` section.
2. **Collect the auto-fixable findings** — every finding whose severity is `Blocker` or `Suggestion` *and* whose recommendation is `Accept`. **Do not include Nitpicks. Do not include Declines.** These stay for the user in Step 5.
3. If the auto-fixable list is **empty**, exit the loop and proceed to Step 5.
4. If you have already completed **3 review iterations** in total, exit the loop and proceed to Step 5 — note in your hand-off that the cap was hit so the user knows there may still be auto-fixable items left.
5. Dispatch the `slice-fixer` agent (via the Agent tool with `subagent_type: "slice-fixer"`), passing the issue URL/number `<issue>` plus the auto-fixable findings list. Each item should include its ID, severity, file:line, the issue, and the proposed fix — copied verbatim from the relevant section of the `review` sticky comment so `slice-fixer` doesn't have to re-parse it.
6. After `slice-fixer` returns, dispatch the same reviewer sub-agents in parallel (using the recorded base branch, current branch, file lists, and component list) to produce a fresh review against the updated code. Merge their outputs and update the `review` sticky comment using the same template. This is the next review iteration — increment your counter. Verify the `review` sticky comment has been updated.
7. Go back to step 1.

Briefly tell the user when each iteration begins and ends, including which findings went to `slice-fixer` and what `slice-fixer` reported back (Fixed / Deviated / Skipped). Do not surface the full review body — the user can read the sticky if they want detail.

## Step 5 — Resolve remaining findings interactively

By the time you reach this step, the loop has driven the slice to either zero auto-fixable findings or the iteration cap. Step 5 handles whatever the loop did not auto-resolve: Nitpicks, Declines, and (if the cap was hit) any leftover Accept-recommended Blockers/Suggestions.

Read the latest `review` sticky comment and collect all findings in document order: Blockers (B1, B2, …), then Suggestions (S1, S2, …), then Nitpicks (N1, N2, …). Skip any finding whose `**Decision:**` line is already set to `Accept` or `Decline`.

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
- How many review iterations ran and whether the loop converged or hit the 3-iteration cap
- How many findings the loop auto-fixed (and any that `slice-fixer` reported as Deviated or Skipped)
- How many findings Step 5 resolved, ignored, and skipped
- Whether any unresolved Blockers remain (they must be addressed before merging)
- Next step: run `/pr` to create the pull request

Do not commit, push, or open a PR — the user triggers that.
