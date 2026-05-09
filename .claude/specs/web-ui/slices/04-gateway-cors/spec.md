# Gateway CORS

## Overview

The Hub Gateway is configured to accept cross-origin browser requests from the web UI's origin(s), enabling the SPA to call the existing API without changing the Gateway's auth contract.

## Requirements

- The Gateway applies a CORS policy globally to all API endpoints.
- The allowed origins are driven by configuration (`WORMS_CORS__ALLOWEDORIGINS`) so the list can differ between local dev and production without code changes.
- The standard HTTP methods used by the API (GET, POST, PUT, DELETE, PATCH) are permitted by the policy.
- The `Authorization` and `Content-Type` headers are permitted, so that JWT bearer tokens and JSON request bodies are accepted.
- The local dev configuration (`appsettings.Development.json`) includes both `http://localhost:3000` (docker-compose web container) and `http://localhost:5173` (Vite dev server) as allowed origins.
- The base `appsettings.json` contains no hardcoded origins; the production value is supplied via environment variable at deploy time.
- The existing auth contract is unchanged: `[Authorize]` attributes, JWT bearer validation, and all existing authentication/authorization behaviour remain exactly as they are.

## Out of Scope

- Production origin configuration — the production URL is not yet known and is wired up in the production deployment slice.
- Per-endpoint CORS policies — a single global policy is sufficient.
- Credential-mode cookies — the UI authenticates via JWT bearer in the `Authorization` header; `AllowCredentials()` is not required.
- Any changes to existing controllers, DTOs, or other Gateway logic.

## Acceptance Criteria

- Given a request from `http://localhost:3000` or `http://localhost:5173` arrives at the Gateway in the Development environment, the response includes the appropriate `Access-Control-Allow-Origin` header.
- Given a preflight `OPTIONS` request from either local-dev origin with `Authorization` in `Access-Control-Request-Headers`, the Gateway responds with a 204 and includes `Authorization` in `Access-Control-Allow-Headers`.
- Given a request from an origin not in the configured list, the Gateway does not include `Access-Control-Allow-Origin` in the response.
- Given the `WORMS_CORS__ALLOWEDORIGINS` environment variable is set to a list of origins, those origins are used as the allowed origins at runtime.
- Given the base `appsettings.json` (no env var override), the allowed origins list is empty and no CORS headers are emitted — confirming there is no hardcoded fallback.
- Existing authenticated API calls (JWT bearer from the CLI) continue to succeed without any change in behaviour.
