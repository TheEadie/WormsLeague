# Web UI

Conventions for working in `src/Worms.Hub.Web/` — the React SPA that will serve as the league's web front-end.

## Stack

- **React 19** with **TypeScript** (strict mode)
- **Vite** as the build tool and dev server
- **Material UI (MUI v7)** for the component library and theming
- **Node 22.x** (matches `actions/setup-node` in CI)

## Project structure

```
src/Worms.Hub.Web/
  public/           static assets
  src/
    App.tsx         root component
    main.tsx        entry point — ThemeProvider + CssBaseline
  eslint.config.js
  vite.config.ts
  tsconfig*.json
  package.json
  package-lock.json
```

## Build output

Vite is configured to write the bundle to `.artifacts/web/` relative to the repo root:

```ts
// vite.config.ts
build: { outDir: '../../.artifacts/web/' }
```

This is picked up by `make web.build` and uploaded as the `web` artefact in CI.

## npm and the lockfile

`make web.build` runs `npm ci`, which requires `package-lock.json` to exist. The lockfile **must be committed** alongside `package.json`. If you add or update dependencies, regenerate it with `npm install` and commit both files.

## Dependencies: declare everything explicitly

Do not rely on transitive dependencies for packages that are imported directly in source or config files. Notable examples:

- `globals` — imported by `eslint.config.js` for `globals.browser`; must be listed in `devDependencies`
- Any other package imported in a config file counts as a direct dependency

## Make targets

| Target | What it does |
|---|---|
| `make web.build` | `npm ci` then `vite build` — produces bundle at `.artifacts/web/` |
| `make web.lint` | ESLint (`eslint src`), TypeScript type-check (`tsc --noEmit`), Prettier format check (`prettier --check src`) |

## Linting and formatting

Three tools run under `make web.lint`:

- **ESLint** — configured in `eslint.config.js` with `js.configs.recommended`, `tseslint.configs.recommended`, and React-specific rules
- **TypeScript** — `tsc --noEmit` against `tsconfig.app.json` (strict mode)
- **Prettier** — `prettier --check src`; format source with `npx prettier --write src`

`web.lint` is a standalone target for local use and Code Scanning. It is **not** wired into `make test` and is **not** a step in the build CI workflow. See [ci-patterns.md](../steering/ci-patterns.md).

## CI

The web build job (`zz-build-web.yml`) always runs — it is not gated on change detection. Change detection (`web-build` output from `zz-detect-changes.yml`) is reserved for a future release/deploy job.

In the build workflow, `make web.build` must run before `make web.lint` because `npm ci` (inside `web.build`) installs the local packages that linting tools depend on. See [ci-patterns.md](../steering/ci-patterns.md).
