# Authenticated Route Gate

## Overview

Every page in the SPA other than the landing page and the OIDC callback requires a signed-in league member. Signed-out visitors who navigate to a protected route are redirected to the landing page (`/`).

## Requirements

- A protected route wrapper exists that any route in the app can be placed behind.
- Routes currently in the app that are protected: `/authenticated`.
- Routes that remain public (no sign-in required): `/` (landing page) and `/callback`.
- When a signed-out visitor navigates to a protected route, they are redirected to `/`.
- While OIDC auth state is still loading (e.g. re-hydrating from localStorage on a hard refresh), a protected route renders nothing rather than immediately redirecting.
- When a signed-in user navigates to a protected route, the page renders normally with no interruption.
- The wrapper is the single place where this redirect logic lives — individual pages do not implement their own auth checks.

## Out of Scope

- Preserving the originally-requested URL and redirecting back to it after sign-in.
- Automatically triggering the Auth0 sign-in redirect (rather than redirecting to the landing page, where the user can click "Sign in" themselves).
- Any loading spinner or skeleton UI during the OIDC re-hydration period.
- Protecting routes introduced in later slices — each later slice is responsible for placing its new routes behind the wrapper when it adds them.

## Acceptance Criteria

- A signed-out visitor who navigates directly to `/authenticated` is redirected to `/` (the landing page).
- A signed-in user who navigates to `/authenticated` sees the page render normally.
- `/` (landing page) and `/callback` remain accessible without signing in.
- Hard-refreshing the browser on `/authenticated` while signed in keeps the user on that page (the loading state does not cause a spurious redirect).
- Hard-refreshing the browser on `/authenticated` while signed out redirects to `/`.
- `make web.build` and `make web.lint` both pass with the changes in place.
