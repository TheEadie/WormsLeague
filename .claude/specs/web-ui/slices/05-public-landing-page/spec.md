# Public Landing Page

## Overview

Replace the SPA's placeholder heading with a real public landing page — the v1 entry surface for unauthenticated visitors. This slice also introduces the page router and a persistent page shell (header + footer) that will be shared across all future pages.

## Requirements

- A client-side router is added to the SPA. The landing page is served at the root route (`/`).
- The landing page displays a large, centred image of a Worm in Worms 2 art style. The implementer must source an appropriate openly-usable image and add it as a static asset.
- Below the image, the page displays the app name "Worms Hub" as the primary heading.
- Below the heading, the page displays a "Sign in" call-to-action button. The button is rendered and visible but is not wired up to any action; clicking it has no effect. The sign-in flow is implemented in the next slice.
- A shared page shell wraps every page in the SPA, consisting of:
  - A **header** that displays the app name/branding. Its structure should accommodate content that differs between anonymous and authenticated states (which later slices will populate).
  - A **footer** that displays a copyright statement.
- The landing page is accessible without any authentication — no redirect, no auth check.

## Out of Scope

- Wiring the Sign in button to an actual OIDC/auth flow (next slice).
- Authenticated content in the header or footer (later slices).
- Any route other than `/` (later slices add further pages).
- 404 / unknown-route handling.
- Any calls to the Gateway API.

## Acceptance Criteria

- Navigating to `/` in a browser renders the landing page with the worm image, "Worms Hub" heading, and a Sign In button.
- Clicking the Sign In button produces no navigation and no visible error.
- The header is visible on the landing page and displays the app name/branding.
- The footer is visible on the landing page and contains a copyright statement.
- The worm image is committed as a static asset in the repository (not fetched from an external URL at runtime).
- `make web.build` completes with no errors.
- `make web.lint` passes (ESLint, TypeScript type-check, Prettier) with no errors.
