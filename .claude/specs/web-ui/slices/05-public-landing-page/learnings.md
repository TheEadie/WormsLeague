# Learnings: Public Landing Page

## Implementation Notes

### No `public/` directory existed — had to create it

The plan assumed `src/Worms.Hub.Web/public/` already existed (Vite scaffolds create it by default). In this repo the directory was absent, so it had to be created before placing `worm.svg` there. Vite's `public/` serving still worked correctly once the directory was present.

### SVG chosen over PNG; original artwork avoids all licence complexity

The plan offered a choice between PNG and SVG and said to source an openly-licensed image. Rather than searching for an external image and tracking a third-party licence, an original SVG worm illustration was created inline. The image reference in `LandingPage.tsx` uses `/worm.svg` (not `/worm.png`) accordingly.

### Prettier reformatted the `<img>` tag in LandingPage.tsx

The initial `LandingPage.tsx` wrote the `<img>` element over three lines. Prettier's `--write` pass collapsed it onto a single line. The reformatted version passes `--check` cleanly; no manual adjustment was needed.

### `make web.lint` output only shows the Prettier banner — ESLint and tsc are silent on success

The ESLint and `tsc --noEmit` steps produce no output when they pass, so the only visible output from a clean `make web.lint` run is "All matched files use Prettier code style!". This is expected behaviour, not a sign that the other checks were skipped.

### `components/` and `pages/` directories also needed creating

The plan listed the new files but did not call out that their parent directories (`src/components/`, `src/pages/`) did not exist. Both had to be `mkdir -p`'d before writing the component files.

### `build/web/Dockerfile.dockerignore` uses a `**` catch-all — new files in `build/web/` must be whitelisted

`build/web/Dockerfile.dockerignore` starts with `**` (exclude everything), then re-allows only `!/src/Worms.Hub.Web`. Any file added to `build/web/` that needs to reach the Docker runtime stage (e.g. `nginx.conf`) must also be explicitly whitelisted with `!/build/web/<filename>`, otherwise the `COPY` instruction in the Dockerfile will fail with "not found" at build time.

### MUI `Stack` does not accept `alignItems` as a direct prop — use `sx` instead

Passing `alignItems="center"` directly to `<Stack>` causes a TypeScript error ("Property 'alignItems' does not exist on type ..."). Use `sx={{ alignItems: 'center' }}` instead.

## Files Added (not in plan)

None — all files created match the plan exactly (accounting for `.svg` instead of `.png` as permitted by the plan).
