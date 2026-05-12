# Plan: Authenticated Route Gate

## Context

This slice adds a single `RequireAuth` wrapper component that guards every protected route in the SPA. It builds directly on slice 08 (Browser Sign-In), which established `react-oidc-context`'s `AuthProvider`, the `useAuth()` hook, `auth.ts`, and the routes `/`, `/callback`, and `/authenticated`. The wrapper reads the `isLoading` and `isAuthenticated` fields already exposed by `useAuth()` and either renders children normally, renders nothing while auth state is re-hydrating, or redirects to `/` for an unauthenticated visitor. No new dependencies are needed. `App.tsx` is the only file that changes in terms of routing — `RequireAuth` is placed around the `/authenticated` route element, and all future slices will wrap their route elements in the same way.

---

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `src/Worms.Hub.Web/src/components/RequireAuth.tsx` | Wrapper component that checks OIDC auth state; renders `null` while loading, redirects to `/` when signed out, renders children when signed in |

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Web/src/App.tsx` | Wrap `<AuthenticatedPage />` in `<RequireAuth>` inside the router config |

---

## Implementation Details

### 1. `RequireAuth` component

Create `src/Worms.Hub.Web/src/components/RequireAuth.tsx`.

The component uses `useAuth()` from `react-oidc-context` and `Navigate` from `react-router`. Three states are handled:

- `auth.isLoading === true` — OIDC state is still being re-hydrated from `localStorage`. Return `null` so no redirect fires before the session is confirmed or denied.
- `auth.isAuthenticated === false` (and not loading) — visitor is signed out. Return `<Navigate to="/" replace />` to redirect to the landing page.
- otherwise (`isAuthenticated === true`) — render `{children}`.

Note: `react-oidc-context` also sets `auth.isLoading = true` while an active navigator request (e.g. `signinRedirect`) is in flight, indicated by `auth.activeNavigator`. The `isLoading` check already covers this case, so no separate handling is required.

```tsx
import { useAuth } from 'react-oidc-context'
import { Navigate } from 'react-router'

interface RequireAuthProps {
    children: React.ReactNode
}

function RequireAuth({ children }: RequireAuthProps) {
    const auth = useAuth()

    if (auth.isLoading) {
        return null
    }

    if (!auth.isAuthenticated) {
        return <Navigate to="/" replace />
    }

    return <>{children}</>
}

export default RequireAuth
```

Key decisions:
- `return null` during loading is exactly what the spec requires ("renders nothing rather than immediately redirecting"). No spinner or skeleton is in scope.
- `<Navigate to="/" replace />` uses `replace` so the protected URL is not pushed onto the browser history stack — the user's back button will not loop them back into the redirect.
- `children` typed as `React.ReactNode` because `RequireAuth` wraps page components (JSX elements), not component types. No `React.FC` wrapper — consistent with the rest of the project (plain function declarations with named export).
- `withAuthenticationRequired` from `react-oidc-context` is deliberately not used: it redirects directly to Auth0 rather than to the landing page, which contradicts the spec's requirement.

### 2. Wire `RequireAuth` into `App.tsx`

In `App.tsx`, import `RequireAuth` and wrap the `<AuthenticatedPage />` element:

```tsx
import RequireAuth from './components/RequireAuth'

const router = createBrowserRouter([
    {
        path: '/',
        element: <Layout />,
        children: [
            { index: true, element: <LandingPage /> },
            { path: 'callback', element: <CallbackPage /> },
            { path: 'authenticated', element: <RequireAuth><AuthenticatedPage /></RequireAuth> },
        ],
    },
])
```

`/` and `/callback` remain public — they are not wrapped. All routes introduced in later slices should also wrap their elements in `<RequireAuth>` when they are added.

### 3. TypeScript and ESLint considerations

- `React.ReactNode` is available globally in this project (strict mode, `@types/react` installed) — no additional import is needed beyond the component imports already used elsewhere.
- `react-refresh/only-export-components` applies only to the file's exports being component types at the module boundary. `RequireAuth` is the sole default export and is a valid React component, so no violation is expected.
- No `eslint-disable` comments should be required.

### 4. Prettier and lint pass

After writing all files, run:

```
cd src/Worms.Hub.Web && npx prettier --write src
```

Then verify with:

```
make web.build && make web.lint
```

Prettier may collapse JSX attributes onto a single line for short expressions — this is expected and safe.

---

## Verification

1. `make web.build` completes without errors — confirms `tsc -b` succeeds with no TypeScript errors and `vite build` produces a bundle.
2. `make web.lint` passes all three checks (ESLint, `tsc --noEmit`, Prettier) — confirms no type errors, lint violations, or formatting issues.
3. Start the SPA dev server: `cd src/Worms.Hub.Web && npm run dev`. Open `http://localhost:5173/authenticated` in a private/incognito window (where no session exists). The browser should redirect to `http://localhost:5173/` (the landing page).
4. Sign in via the landing page. After the callback completes, navigate manually to `http://localhost:5173/authenticated`. The page should render normally (games list or "No games found.") without any redirect.
5. Hard-refresh the browser while on `http://localhost:5173/authenticated` when signed in. The user should remain on `/authenticated` — confirming `isLoading` suppresses a spurious redirect before `localStorage` is read.
6. Hard-refresh the browser while on `http://localhost:5173/authenticated` in a private window (no session). The user should be redirected to `/`.
7. Navigate to `http://localhost:5173/` and `http://localhost:5173/callback` while signed out — both pages should be reachable without any redirect.
