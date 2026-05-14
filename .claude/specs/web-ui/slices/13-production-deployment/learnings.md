# Learnings: Production Deployment

## Implementation Notes

### Plan went exactly as described — no deviations

Every step in the plan was accurate and complete. The file paths, content, and ordering all matched reality. The `vite.config.ts` `outDir` of `../../.artifacts/web/` resolves correctly to `/repo/.artifacts/web/` given the `WORKDIR /repo` in the node-build stage. The existing `!/src` rule in `Dockerfile.dockerignore` already covered `src/Worms.Hub.Web/` so no additional allow rule was needed there. The `node:24-alpine` pinned digest in the existing `build/web/Dockerfile` matched what the plan specified.

## Files Added (not in plan)

None.
