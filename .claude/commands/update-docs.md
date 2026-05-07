---
description: Refresh CLAUDE.md and the docs under .claude/docs/ to match the current codebase
---

You are auditing and updating the project's Claude-facing documentation so it reflects the current state of the code. The docs in scope:

- `CLAUDE.md` — top-level orientation (what this is, build, local dev, links to other docs)
- `.claude/docs/components/*.md` — one per component, covering the internals of that component
- `.claude/docs/steering/architecture.md` — high-level overview of components and how they fit together
- `.claude/docs/steering/coding-guidelines.md` — cross-cutting conventions
- `.claude/docs/steering/testing-strategy.md` — testing tiers and what goes where

## Approach

Treat this as a full regeneration of the docs from the current codebase, not a diff-based update. The codebase is the source of truth — the existing docs are a hint about structure and tone, nothing more. If the docs were deleted, this command should be able to re-derive them from scratch. Do not rely on `git log` or `git diff` to decide what to check; you must inspect the actual source.

## What to do

1. **Survey the codebase end-to-end.** Build your own picture of what exists today before opening any doc:
   - Enumerate every project: `src/**/*.csproj`, `deployment/**/*.csproj`, plus the solution file.
   - For each project, identify its purpose by reading the entry points and public surface (`Program.cs`, `ServiceRegistration.cs`, top-level public types).
   - Map the inter-project dependency graph from `<ProjectReference>` entries.
   - Read `Directory.Build.props`, `.editorconfig`, `Worms.sln`, and every root-level `*.props` / `*.targets` / `global.json` that influences build behaviour.
   - Read every `makefile` under `build/` and every `Dockerfile` / `docker-bake.hcl` / `docker-compose*.yml`.
   - Read every `appsettings*.json` and capture the config keys / connection strings / env-var bindings each component reads.
   - Identify all queues, message types, HTTP routes, CLI verbs, and external integrations (Slack, Auth0/JWT authority, Azure services, Cloudflare, Pulumi providers).
   - Identify the test projects, their categories, and what infrastructure their integration tests stand up.

2. **Now read the existing docs in scope** to see how the project has chosen to describe itself, then for each doc:
   - List every concrete claim it makes.
   - Verify the claim against your survey from step 1. Anything you can't substantiate from the current code is wrong, even if it was true once.
   - Identify gaps: things present in the codebase that warrant mention but aren't in the doc.

3. **Update each doc.** Make the smallest edit that makes the doc both accurate and complete for its tier. When something has been removed from the code, remove it from the docs — do not leave "previously this lived in X" historical notes (git is authoritative for history). When something new exists in the code, add it where it belongs based on the tier (steering vs component).

4. **Sanity-check** that the set of component docs still covers all source projects, and that CLAUDE.md's component table lists each component doc exactly once.

## Rules

- **No file paths or line numbers in steering docs** (`architecture.md`, `coding-guidelines.md`, `testing-strategy.md`). Steering docs are high-level and must not rot when files move. Component docs may reference type names but should avoid line numbers.
- **No version numbers in CLAUDE.md or steering docs** (no "PostgreSQL 17", no ".NET 10.0.x", no "NUnit 4.6"). Versions belong in csproj / Dockerfile / `Directory.Build.props` and the docs should defer to those. Component docs may name a major framework (e.g. "ASP.NET Core") but not a minor version.
- **Don't duplicate content across tiers.** If something is in a component doc, the steering docs should not repeat it. If something is in the coding-guidelines, individual component docs should not repeat it. When you find duplication, keep it in the most specific applicable doc and remove it from the others.
- **Don't reference PRs, issue numbers, or commit hashes** in any doc — they rot.
- **Don't add docs the user didn't ask for.** Update existing files; do not create new component or steering docs unless the user explicitly requests one.
- **Preserve voice and structure.** Match the tone of the existing docs (concise, present-tense, declarative). Don't rewrite a doc that's still accurate just to "improve" it.

## Output

When you're done, report:
- A bullet list of files changed and the substantive change in each (one line per file).
- Any drift you noticed but deliberately did NOT fix, with a one-line reason.
- Anything ambiguous that needs the user's input before you can confidently edit.

Do not commit. The user will review and commit themselves.
