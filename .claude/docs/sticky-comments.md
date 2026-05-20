# Sticky comments on GitHub issues

The spec / plan / implement / review / retro pipeline writes its artefacts to a GitHub issue. The issue body holds the spec; named **sticky comments** on the same issue hold every later artefact (plan, learnings, review, retrospective).

Each sticky comment is identified by a hidden HTML-comment marker on its first line. Subsequent runs find and update the existing comment by marker, so the same artefact stays at one stable URL.

## Marker format

The very first line of the comment body MUST be:

```
<!-- claude:sticky:<name> -->
```

Followed by a blank line and then the rendered markdown. `<name>` is one of the well-known names listed below.

## Well-known sticky names

| Name | Written by | Contains |
|---|---|---|
| `plan` | `/plan-spec` | Implementation plan for the slice |
| `learnings` | `/implement-slice` | Notes captured during implementation |
| `review` | `/review` | Two-axis review findings and recommended actions |
| `retrospective` | `/retro-epic` | Cross-slice retrospective written to the parent epic issue |

The slice **spec** itself is not a sticky comment — it replaces the issue body (see `/spec`).

## Reading a sticky comment

```bash
gh api "repos/{owner}/{repo}/issues/<number>/comments" --paginate \
  --jq '.[] | select(.body | startswith("<!-- claude:sticky:<name> -->")) | {id, body, url}'
```

Resolve `{owner}/{repo}` from the current git remote (`gh repo view --json nameWithOwner -q .nameWithOwner`) unless the issue URL provides it.

If no matching comment exists, the artefact has not been produced yet.

## Writing (create-or-update) a sticky comment

1. Render the full body to a temp file (e.g. `/tmp/sticky-<name>.md`) with the marker line at the top.
2. Look up the existing comment id with the read query above.
3. If an id was found, update:

   ```bash
   gh api -X PATCH "repos/{owner}/{repo}/issues/comments/<id>" \
     -F body=@/tmp/sticky-<name>.md
   ```

4. Otherwise, create:

   ```bash
   gh issue comment <number> --body-file /tmp/sticky-<name>.md
   ```

Always pass the body via `--body-file` / `-F body=@…`, never inline, so multi-line content and special characters survive intact.

## Editing in place (partial updates)

When a workflow needs to flip a single line inside a sticky comment (e.g. `/implement` changing `**Decision:** *(pending)*` to `**Decision:** Accept` inside the `review` comment), read the current body, modify the string, then write the full updated body back via the same create-or-update flow. Do not append a second sticky with the same name.

## Required input

Every command in the pipeline requires a GitHub issue reference (a full GitHub issue URL or a `#NNN` token) in the user's invocation. If none is supplied, the command asks for one before doing any other work. There is no file-based fallback.
