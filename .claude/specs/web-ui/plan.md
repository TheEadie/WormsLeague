# Worms Hub Web UI — Delivery Plan

Companion to [`spec.md`](./spec.md). Each slice below is sized to ship as a single PR and described at a "what is delivered" level only — not how it is built.

## Slices

- [x] **SPA scaffolding** — a new React/TypeScript project lives in the repo, builds, lints, has a placeholder home page, and is wired into the makefile and CI alongside the existing .NET projects.
- [x] **Web linting in CI** — `code-scanning.yml` gains jobs for CodeQL (JavaScript/TypeScript), ESLint, and Prettier, each uploading results as SARIF where supported, consistent with the existing Roslyn and JetBrains jobs, and included in the `actions-timeline` needs.
- [x] **Local-dev integration** — `docker compose up` brings the SPA up alongside the existing Hub services so the full stack runs locally end-to-end.
- [x] **Gateway CORS** — the Gateway accepts browser requests from the UI's origin(s) without changing its auth contract.
- [x] **Public landing page** — the SPA serves a real anonymous landing page as the v1 entry surface for unauthenticated visitors.
- [x] **Dark mode** — the UI respects the user's OS colour scheme preference, applying MUI's dark or light palette automatically; all existing and future pages benefit.
- [x] **Mockup alignment** — the landing page, header, and footer match the Claude Design mockups in layout, typography, and visual treatment, establishing the shared chrome that subsequent pages will sit inside.
- [ ] **Browser sign-in** — a new SPA OIDC client is registered in the authority, the UI implements the interactive sign-in flow, and authenticated calls to the Gateway succeed with a bearer token.
- [ ] **Authenticated route gate** — every page other than the landing page requires a signed-in league member; signed-out visitors are redirected to sign in.
- [ ] **League list** — a signed-in member sees a list of leagues to navigate into, including any Gateway API extension needed to serve that list.
- [ ] **Per-league page** — a signed-in member can open a league and see its historic games list and the scheme it uses, including any Gateway API extension needed.
- [ ] **Game detail page** — a signed-in member can drill into a single game and see its participants, winner, scheme, and date.
- [ ] **Replay viewer** — the game detail page renders the replay's GIF(s) and log in the browser, including any Gateway API extension needed to retrieve those artifacts.
- [ ] **Production deployment** — the SPA is built and deployed to production alongside the rest of the Hub, with CORS and authority configuration applied to the production environment.
