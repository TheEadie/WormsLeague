---
description: Run JetBrains ReSharper InspectCode over Worms.sln locally — matches the `Jetbrains` CI job. Reports any warnings grouped by rule and file. Does not modify code.
effort: low
---

You run the same JetBrains inspection that CI runs in `.github/workflows/code-scanning.yml` (the `Jetbrains` job), so the user can reproduce and triage warnings without pushing.

Run everything **from the repo root**. This is **report only** — do not edit files to fix warnings unless the user asks for that as a separate follow-up.

## Step 1 — Build the solution

Build first so any compile error surfaces fast and clearly, before the slower inspection:

```bash
dotnet build Worms.sln
```

If this fails, report the build errors and stop — InspectCode can't run against a solution that doesn't build.

## Step 2 — Run InspectCode

```bash
dotnet jb inspectcode Worms.sln --output=jetbrains.sarif --format=sarif
```

The `jb` tool is pinned in `dotnet-tools.json` at the repo root. If it isn't restored yet, run `dotnet tool restore` once and retry. This step takes roughly 90 seconds — use a Bash timeout of 600000ms. The `jetbrains.sarif` output is gitignored.

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

Do not propose fixes, do not edit files, do not commit. The user will decide what to do with the report. If they ask for fixes afterwards, treat that as a new request and handle each warning on its own merits — many R# suggestions (e.g. `MemberCanBePrivate.Global`, `AutoPropertyCanBeMadeGetOnly.Global`) need a judgement call about whether the broader visibility/setter is intentional. If it isn't actually used, tighten the member (make it `private`, get-only) rather than masking it with `[PublicAPI]` — that attribute claims an external consumer exists and is a lie on unused code.

## Notes

- The CI workflow does **not** filter suppressed JetBrains results, so any warning here will fail the `Jetbrains` job.
- If you need to silence a warning legitimately, prefer a tightly-scoped `[SuppressMessage]` attribute or a `.DotSettings` rule, not a blanket disable.
- If a previously-passing inspection suddenly fails its internal build with package-resolution errors (`NETSDK1064`) or a `LoggerException`, a stale MSBuild build-server node may be the cause — run `dotnet build-server shutdown` and retry.
