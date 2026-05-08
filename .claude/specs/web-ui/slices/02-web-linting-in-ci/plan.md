# Plan: Web Linting in CI

## Context

Slice 01 delivered the SPA scaffold with `make web.lint` (ESLint, tsc, Prettier) as a standalone
target. This slice wires that linting into `code-scanning.yml` as three separate jobs consistent
with the existing Roslyn and JetBrains jobs, and renames the `build-web` job in `zz-build-web.yml`
to reflect that linting no longer lives in the build workflow.

## Files to Create / Modify

### New files

None.

### Modified files

| Path | Change |
|---|---|
| `.github/workflows/zz-build-web.yml` | Rename job from `Build and lint` → `Build` |
| `.github/workflows/code-scanning.yml` | Add `codeql-javascript-typescript`, `eslint`, and `prettier` jobs; add all three to `actions-timeline.needs` |
| `src/Worms.Hub.Web/package.json` | Add `@microsoft/eslint-formatter-sarif` to `devDependencies` |
| `src/Worms.Hub.Web/package-lock.json` | Regenerate after adding the new dep (`npm install` from `src/Worms.Hub.Web/`) |

---

## Implementation Details

### 1. Rename the build-web job

In `.github/workflows/zz-build-web.yml`, change line 10:

```yaml
# before
    name: Build and lint
# after
    name: Build
```

### 2. Add `@microsoft/eslint-formatter-sarif` to package.json

This package converts ESLint output to the SARIF format required by
`github/codeql-action/upload-sarif`. Add it to `devDependencies` in
`src/Worms.Hub.Web/package.json`:

```json
"@microsoft/eslint-formatter-sarif": "^3.1.0"
```

After editing `package.json`, regenerate the lockfile:

```bash
cd src/Worms.Hub.Web && npm install
```

Commit both `package.json` and `package-lock.json`.

### 3. Three new jobs in `code-scanning.yml`

Insert the three jobs before the `actions-timeline` job. All jobs follow the existing structure:
`actions/checkout@v6`, no `fetch-depth: 0` (no git history needed).

#### `codeql-javascript-typescript`

```yaml
  codeql-javascript-typescript:
    name: CodeQL (javascript-typescript)
    runs-on: ubuntu-latest
    steps:
      - name: Checkout repository
        uses: actions/checkout@v6
      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: 22.x
      - name: Build
        run: make web.build
      - name: Initialize CodeQL
        uses: github/codeql-action/init@v4
        with:
          languages: javascript-typescript
          build-mode: none
      - name: Perform CodeQL Analysis
        uses: github/codeql-action/analyze@v4
        with:
          category: "/language:javascript-typescript"
```

`build-mode: none` is correct for JS/TS (no compilation step required for CodeQL). `make web.build`
runs `npm ci` so `node_modules` exists, consistent with the spec requirement and the CI pattern
that `web.build` must precede any lint step.

#### `eslint`

```yaml
  eslint:
    name: ESLint
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v6
      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: 22.x
      - name: Build
        run: make web.build
      - name: Run ESLint
        run: cd src/Worms.Hub.Web && npx eslint src --format @microsoft/eslint-formatter-sarif --output-file eslint.sarif
      - name: Upload ESLint SARIF
        if: always()
        uses: github/codeql-action/upload-sarif@v4
        with:
          sarif_file: src/Worms.Hub.Web/eslint.sarif
```

ESLint must be invoked from `src/Worms.Hub.Web/` so it picks up `eslint.config.js`. The SARIF
file lands at `src/Worms.Hub.Web/eslint.sarif`; the upload step references that path from the
repo root. `if: always()` on the upload mirrors the Roslyn and JetBrains pattern, ensuring the
SARIF is uploaded even when ESLint finds violations.

ESLint exits non-zero on rule violations, so the `Run ESLint` step itself will fail the job.

#### `prettier`

```yaml
  prettier:
    name: Prettier
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v6
      - name: Setup Node
        uses: actions/setup-node@v4
        with:
          node-version: 22.x
      - name: Build
        run: make web.build
      - name: Run Prettier
        run: cd src/Worms.Hub.Web && npx prettier --check src
```

Prettier does not produce SARIF; it exits non-zero on formatting violations. No upload step.

### 4. Update `actions-timeline` needs

Change the `needs` list of the `actions-timeline` job from:

```yaml
    needs: [roslyn, jetbrains, codeql-actions, codeql-csharp]
```

to:

```yaml
    needs: [roslyn, jetbrains, codeql-actions, codeql-csharp, codeql-javascript-typescript, eslint, prettier]
```

---

## Verification

1. **Rename check**: In `zz-build-web.yml`, confirm `name:` under the `build-web` job reads `Build`.
2. **Package dep check**: In `src/Worms.Hub.Web/package.json`, confirm `@microsoft/eslint-formatter-sarif` appears in `devDependencies`. Confirm `package-lock.json` is updated (contains the package entry).
3. **Local lint**: Run `make web.build && cd src/Worms.Hub.Web && npx eslint src --format @microsoft/eslint-formatter-sarif --output-file eslint.sarif` from the repo root; confirm `src/Worms.Hub.Web/eslint.sarif` is created and is valid JSON with a `runs` array.
4. **Prettier local**: Run `cd src/Worms.Hub.Web && npx prettier --check src`; confirm it exits 0.
5. **CI**: Push a PR; confirm the `code-scanning.yml` run shows seven scan jobs (`roslyn`, `jetbrains`, `codeql-actions`, `codeql-csharp`, `codeql-javascript-typescript`, `eslint`, `prettier`) and `actions-timeline` completes after all seven pass.
