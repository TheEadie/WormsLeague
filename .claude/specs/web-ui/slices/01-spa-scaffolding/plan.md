# Plan: SPA Scaffolding (Slice 01)

## Context

The web-ui epic adds a React/TypeScript SPA to the WormsLeague repository. This first slice scaffolds the project: creates `src/Worms.Hub.Web/`, wires it into the build system, and adds CI coverage. No functional content is required beyond a placeholder home page.

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `src/Worms.Hub.Web/package.json` | npm project manifest with all dependencies |
| `src/Worms.Hub.Web/index.html` | Vite entry HTML |
| `src/Worms.Hub.Web/vite.config.ts` | Vite config; output dir → `../../.artifacts/web/` |
| `src/Worms.Hub.Web/tsconfig.json` | TypeScript config |
| `src/Worms.Hub.Web/tsconfig.app.json` | App-specific TS config (Vite default split) |
| `src/Worms.Hub.Web/tsconfig.node.json` | Node TS config for vite.config.ts |
| `src/Worms.Hub.Web/eslint.config.js` | ESLint flat config (v9) |
| `src/Worms.Hub.Web/.prettierrc` | Prettier config |
| `src/Worms.Hub.Web/src/main.tsx` | App entry; MUI ThemeProvider + CssBaseline |
| `src/Worms.Hub.Web/src/App.tsx` | Placeholder home page with "Worms League" heading |
| `src/Worms.Hub.Web/src/vite-env.d.ts` | Vite client type reference |
| `build/web/makefile` | `web.build`, `web.test`, `web.lint` targets |
| `.github/workflows/zz-build-web.yml` | Reusable CI workflow for web build + lint |

### Modified files

| Path | Change |
|---|---|
| `makefile` | Add `include build/web/makefile` |
| `.github/workflows/zz-detect-changes.yml` | Add `web` path filter + `web-build` output |
| `.github/workflows/code-branch.yml` | Add `build-web` job (conditional on `web-build`) |
| `.github/workflows/code-main.yml` | Add `build-web` job (conditional on `web-build`) |

---

## Implementation Details

### 1. `src/Worms.Hub.Web/` — Vite + React + TypeScript + MUI

Use a standard Vite React-TS scaffold layout. Key dependencies:
- `react`, `react-dom`
- `@mui/material`, `@emotion/react`, `@emotion/styled`
- Dev: `vite`, `typescript`, `eslint`, `@eslint/js`, `eslint-plugin-react-hooks`, `eslint-plugin-react-refresh`, `typescript-eslint`, `prettier`

**`vite.config.ts`** — set `build.outDir` to `../../.artifacts/web/` and `build.emptyOutDir: true`.

**`src/main.tsx`** — wrap `<App />` in `<ThemeProvider theme={createTheme()}>` and render `<CssBaseline />` before it.

**`src/App.tsx`** — single `<Typography variant="h1">Worms League</Typography>` (or plain `<h1>`).

**`.prettierrc`** — minimal config (singleQuote, semi, etc.) consistent with project style.

### 2. `build/web/makefile`

```makefile
build:: web.build
test:: web.test

web.build:
	@cd src/Worms.Hub.Web && npm ci
	@cd src/Worms.Hub.Web && npx vite build
	@echo ""

web.test: web.lint

web.lint:
	@cd src/Worms.Hub.Web && npx eslint src
	@cd src/Worms.Hub.Web && npx tsc --noEmit
	@echo ""
```

### 3. Root `makefile`

Add `include build/web/makefile` after the existing includes.

### 4. CI

**`zz-detect-changes.yml`** — add output `web-build` and filter:
```yaml
web:
  - 'src/Worms.Hub.Web/**'
  - 'build/web/**'
```
Output: `web-build: ${{ steps.filter.outputs.web == 'true' }}`

**`zz-build-web.yml`** — new reusable workflow; single job:
- `actions/checkout@v6`
- `actions/setup-node@v4` with node 22.x
- `make web.lint`
- `make web.build`
- `actions/upload-artifact@v7` uploading `.artifacts/web/`

**`code-branch.yml` and `code-main.yml`** — add:
```yaml
build-web:
  name: Build - Web
  needs: changes
  if: ${{ needs.changes.outputs.web-build == 'true' }}
  uses: ./.github/workflows/zz-build-web.yml
  secrets: inherit
```
Also add `build-web` to the `actions-timeline` `needs` array in each file.

---

## Verification

1. `cd src/Worms.Hub.Web && npm ci && npx vite build` — produces bundle under `.artifacts/web/`.
2. `make build` — completes without error; `.artifacts/web/` directory exists.
3. `make test` — ESLint and `tsc --noEmit` pass cleanly.
4. Introduce a lint violation → `make test` exits non-zero.
5. Introduce a type error → `make test` exits non-zero.
6. `make web.build` then serve `.artifacts/web/` statically → navigating to `/` shows "Worms League" heading.
7. On a branch touching `src/Worms.Hub.Web/src/App.tsx` → CI runs `build-web` job.
8. On a branch touching only `.NET` source → CI skips `build-web` job.
