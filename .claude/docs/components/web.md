# Web UI

Conventions for working in `src/Worms.Hub.Web/` — the React SPA that will serve as the league's web front-end.

## Stack

- **React 19** with **TypeScript** (strict mode)
- **Vite** as the build tool and dev server
- **Material UI (MUI v9)** for the component library and theming
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
| `make web.lint` | ESLint (`eslint src`), TypeScript type-check (`tsc -b`), Prettier format check (`prettier --check src`) |
| `make web.test` | Vitest unit tests (Vitest + React Testing Library) |

## Linting and formatting

Three tools run under `make web.lint`:

- **ESLint** — configured in `eslint.config.js` with `js.configs.recommended`, `tseslint.configs.recommended`, and React-specific rules
- **TypeScript** — `tsc -b` (not `tsc --noEmit`). The root `tsconfig.json` has `files: []` combined with `references`, so `tsc --noEmit` against it type-checks nothing and exits zero. Always use `tsc -b`.
- **Prettier** — `prettier --check src`; format source with `npx prettier --write src`

`make web.lint` must pass before committing any SPA changes, including follow-up commits made in response to review feedback. Prettier formatting drift is a CI failure; catch it locally before pushing.

`web.lint` is a standalone target for local use and Code Scanning. It is **not** wired into `make test` and is **not** a step in the build CI workflow. See [ci-patterns.md](../steering/ci-patterns.md).

## CI

The web build job (`zz-build-web.yml`) always runs — it is not gated on change detection. Change detection (`web-build` output from `zz-detect-changes.yml`) is reserved for a future release/deploy job.

In the build workflow, `make web.build` must run before `make web.lint` because `npm ci` (inside `web.build`) installs the local packages that linting tools depend on. See [ci-patterns.md](../steering/ci-patterns.md).

## Dependencies

When adding or updating npm packages, resolve the current latest version at implementation time (e.g. `npm show <pkg> version`) rather than using versions drawn from training data. Versions in `package.json` should reflect the latest available at the time of writing.

## Docker

All base images in `build/web/Dockerfile` must be pinned with `@sha256:` digests, consistent with the convention in the other Dockerfiles in the repo (e.g. gateway and wa-runner). Floating tags are not acceptable.

`build/web/Dockerfile.dockerignore` uses a `**` deny-all pattern and re-allows only the SPA source directory. Any file in `build/web/` that a `COPY` instruction needs (e.g. `nginx.conf`) must be explicitly whitelisted by adding `!/build/web/<filename>` to that file, or the Docker build will fail silently with a "not found" error.

## Testing

Web unit tests use **Vitest** and **React Testing Library**. Run them with `make web.test`.

Any component that is the single enforcement point for a security or routing invariant (e.g. an auth guard) must have automated tests covering all branches as part of its slice spec. Security-critical components are not candidates for deferred testing.

When a slice introduces web test infrastructure for the first time (Vitest, a new CI job, a new make target), the spec must explicitly include that infrastructure in scope.

## Layout

Page components rendered inside `Layout` must size themselves with `flex: 1` on their outermost element to consume remaining viewport space. Do not use `minHeight: calc(100vh - Npx)` — this hardcodes an assumed pixel height for sibling components and breaks when those components change size or the footer needs to remain visible.

## MUI v9 API notes

- Scalar style shorthands (`fontWeight`, `display`, `color`, etc.) are not accepted as direct JSX props on MUI components. Place them in the `sx` prop: `<Typography sx={{ fontWeight: 700 }}>`.
- `primaryTypographyProps` and `secondaryTypographyProps` on `<ListItemText>` are removed in MUI v9. Use `slotProps={{ primary: { ... }, secondary: { ... } }}` instead.

## React purity

Values that depend on impure functions (e.g. `Math.random()`, `Date.now()`) and should be computed once on mount must be placed in a lazy `useState` initialiser (`useState(() => ...)`) rather than `useMemo(() => ..., [])`. The `react-compiler/react-compiler` ESLint rule rejects impure function calls anywhere in the render path, including inside `useMemo`.
