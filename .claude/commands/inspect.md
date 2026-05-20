---
description: Run JetBrains ReSharper InspectCode over Worms.sln locally — matches the `Jetbrains` CI job byte-for-byte. Reports any warnings grouped by rule and file. Does not modify code.
effort: low
---

You run the same JetBrains inspection that CI runs in `.github/workflows/code-scanning.yml` (the `Jetbrains` / `InspectCode` job), so the user can reproduce and triage warnings without pushing.

This is **report only**. Do not edit files to fix warnings unless the user asks for that as a separate follow-up.

## Step 1 — Restore the tool

The `JetBrains.ReSharper.GlobalTools` package is pinned in `dotnet-tools.json`. Restore it (idempotent):

```bash
dotnet tool restore
```

## Step 2 — Run InspectCode against the full solution

Run the same command the CI workflow uses. Do not narrow the scope — the goal is parity with CI.

```bash
dotnet jb inspectcode Worms.sln --output=jetbrains.sarif --format=sarif
```

This takes roughly 90 seconds. Use a Bash timeout of 600000ms. The output file `jetbrains.sarif` is gitignored.

If the command itself fails (non-zero exit, missing tool, build error inside the inspector), report the failure as-is and stop — do not attempt to parse SARIF.

## Step 3 — Count and summarise warnings

```bash
jq '[.runs[].results[]] | length' jetbrains.sarif
```

- **0** → tell the user "InspectCode clean — matches CI." and stop.
- **>0** → CI will fail. Print a grouped summary:

```bash
jq -r '.runs[].results[] | "\(.ruleId)\t\(.locations[0].physicalLocation.artifactLocation.uri):\(.locations[0].physicalLocation.region.startLine)\t\(.message.text)"' jetbrains.sarif
```

Group the output by `ruleId` in your reply so the user can see which rules are firing and how often, with one bullet per occurrence underneath showing `path:line — message`. Keep it compact; do not paste the raw SARIF.

## Step 4 — Stop

Do not propose fixes, do not edit files, do not commit. The user will decide what to do with the report. If they ask for fixes afterwards, treat that as a new request and handle each warning on its own merits — many R# suggestions (e.g. `MemberCanBePrivate.Global`) need a judgement call about whether the broader visibility is intentional for testability or future consumers.

## Notes

- The CI workflow filters out suppressed results for Roslyn but **not** for JetBrains, so any warning here will fail the `Jetbrains` job.
- The companion `InspectCode` check on GitHub is the same SARIF surfaced as a code-scanning alert — fixing the warnings clears both.
- If you need to silence a warning legitimately, prefer a `[SuppressMessage]` attribute or a `.DotSettings` rule scoped tightly, not a blanket disable.
