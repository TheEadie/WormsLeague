# Infrastructure Component

Project: `deployment/Worms.Hub.Infrastructure`
Solution: `deployment/Infrastructure.sln`

Infrastructure-as-code using **Pulumi** (C#, net10.0) targeting Azure.

## Stack configuration

Two stacks defined in `Pulumi.*.yaml`:
- `Pulumi.prod.yaml` — production
- `Pulumi.test.yaml` — test/staging

Key config values:
- `azure-native:location: northeurope`
- `worms-hub:domain: davideadie.dev`
- `worms-hub:gateway-image: theeadie/worms-hub-gateway:<version>`
- `worms-hub:wa-runner-image: theeadie/worms-hub-wa-runner:<version>`
- `worms-hub:database-version: <flyway-version>`

## Pulumi providers in use

- `Pulumi.AzureNative` v2 — Azure resources
- `Pulumi.Cloudflare` — DNS / CDN
- `Pulumi.Command` — run commands as part of stack (e.g. database migrations)
- `Pulumi.Random` — generate passwords/secrets

## Deployment workflows

CI/CD is managed by GitHub Actions:

- `deploy-main.yml` — deploys to production on merge to main
- `deploy-pr.yml` — deploys to test stack on PR

Releases of the hub Docker images are handled by `zz-release-hub.yml`, which calls `make gateway.release` and `make wa-runner.release`. These push to Docker Hub (`theeadie/worms-hub-*`) then update the Pulumi stack config with the new image version.

The `release-wormshub.sh` script handles deploying a new CLI version to the hub (uploads the binary to blob storage).

## Database migrations

Migrations are in `src/database/migrations/` as Flyway SQL files (versioned `V<x.y>__Description.sql`). The Pulumi stack applies migrations during deployment via `Pulumi.Command` calling the Flyway Docker image.

Local dev seeds live in `src/database/local-dev/` — these run automatically in `docker compose up` via the `flyway-init` service but are not applied in CI or production.

## Adding a new image version

Update the `worms-hub:gateway-image` or `worms-hub:wa-runner-image` value in the appropriate `Pulumi.*.yaml` and run `pulumi up`. The release workflow does this automatically when triggered.
