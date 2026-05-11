# Browser Sign-In

## Overview

Wires up interactive OIDC sign-in and sign-out for the SPA using the existing Auth0 authority. The landing page's "Sign in" button triggers an Authorization Code + PKCE redirect; after returning from Auth0 the user lands on a temporary authenticated page that makes a real call to the Gateway API to confirm the token works end-to-end.

## Requirements

- An OIDC client library is added to the SPA that supports Authorization Code + PKCE.
- The OIDC configuration (authority, client ID, audience, scopes) is hardcoded in source — not supplied via environment variables — consistent with how the CLI stores its auth constants.
  - Authority: `https://eadie.eu.auth0.com/`
  - Audience: `worms.davideadie.dev`
  - Scopes: `openid profile`
  - Client ID: the SPA client already registered in Auth0 (separate from the CLI device-flow client)
- Clicking "Sign in" on the landing page redirects the browser to Auth0 to authenticate.
- After successful authentication Auth0 redirects back to a `/callback` route, which then navigates the user to a temporary authenticated page.
- The temporary authenticated page calls `GET /api/v1/games` on the Hub Gateway, attaching the acquired bearer token, and renders the response (a list of games or an empty state).
- When signed in, the header displays the authenticated user's username.
- Clicking the username in the header opens a menu containing a "Sign out" option; triggering it clears the OIDC session and returns the user to the landing page.
- The acquired access token is accessible via a standard context or hook so that future slices can attach it to Gateway requests without reimplementing auth.
- Sign-in state persists across hard browser refreshes — a user who was signed in before refreshing the page does not need to sign in again.
- Access tokens are silently renewed before they expire so a signed-in session remains valid indefinitely without user interaction.

## Out of Scope

- The authenticated route gate (next slice) — unsigned-in users can still navigate to the temporary page directly in this slice.
- Any permanent post-sign-in destination; the temporary authenticated page exists only to prove the flow works and will be replaced by the league list page in a later slice.
- Error UI for failed sign-in (e.g. user cancels at Auth0) — not in scope; the OIDC library's default behaviour is sufficient.

## Acceptance Criteria

- Clicking "Sign in" on the landing page redirects the browser to Auth0.
- After completing authentication at Auth0, the browser lands on the temporary authenticated page (not on `/callback` or the landing page).
- The temporary page displays the result of `GET /api/v1/games` — either a list of games or an empty state — without a 401 error, confirming the bearer token is valid and accepted by the Gateway.
- When signed in, the header shows the user's username.
- Clicking the username in the header opens a menu with a "Sign out" option; triggering it clears the session and returns the user to the landing page with the "Sign in" button visible again.
- Hard-refreshing the browser while on the temporary authenticated page keeps the user signed in and does not redirect them to sign in again.
- A session remains valid beyond the initial access token's lifetime without requiring the user to sign in again.
- `make web.build` and `make web.lint` both pass with the new code in place.

## Open Questions

None.
