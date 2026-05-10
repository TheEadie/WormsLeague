# Plan: Public Landing Page

## Context

This slice replaces the placeholder `App.tsx` heading with a real public landing page, and introduces two foundational pieces that all future slices will build on: a client-side router and a persistent page shell (header + footer). Slices 01–04 have already delivered the React/TypeScript/Vite/MUI project scaffold, CI linting, Docker local-dev integration, and Gateway CORS. This slice adds no backend changes.

One concrete side-effect of introducing a client-side router: the nginx server (slice 03) must be configured to rewrite all paths to `index.html` so that React Router can handle them in the browser. A custom `nginx.conf` is therefore part of this slice.

---

## Files to Create / Modify

### New files

| Path | Purpose |
|---|---|
| `src/Worms.Hub.Web/public/worm.png` | Worm image static asset (openly-licensed, see §1) |
| `src/Worms.Hub.Web/src/components/Header.tsx` | Shared app header (branding, slot for future auth content) |
| `src/Worms.Hub.Web/src/components/Footer.tsx` | Shared app footer with dynamic copyright year |
| `src/Worms.Hub.Web/src/components/Layout.tsx` | Page shell: renders Header + `<Outlet />` + Footer |
| `src/Worms.Hub.Web/src/pages/LandingPage.tsx` | Public landing page (worm image, heading, Sign In button) |
| `build/web/nginx.conf` | Custom nginx config with `try_files` for SPA routing |

### Modified files

| Path | Change |
|---|---|
| `src/Worms.Hub.Web/package.json` | Add `react-router` v7 to `dependencies` |
| `src/Worms.Hub.Web/package-lock.json` | Regenerated after `npm install` |
| `src/Worms.Hub.Web/src/App.tsx` | Replace placeholder heading with router + route tree |
| `src/Worms.Hub.Web/index.html` | Update `<title>` from "Worms League" to "Worms Hub" |
| `build/web/Dockerfile` | Copy custom `nginx.conf` into the runtime image |

---

## Implementation Details

### 1. Worm image static asset

Source an openly-usable Worm image — a PNG or SVG with a licence that permits redistribution (e.g. Creative Commons, public domain). A suitable source is Wikimedia Commons or similar. Search for "worm cartoon" or "Worms Armageddon fan art" with a CC0 or CC-BY licence, or create a simple SVG directly. The image should be visually consistent with a Worms 2 art style (cartoony, colourful worm character).

Place the file at `src/Worms.Hub.Web/public/worm.png` (or `.svg` — either works; adjust the `<img>` `src` attribute accordingly). Vite serves everything in `public/` at the URL root, so it will be accessible as `/worm.png` at runtime without any import. The Dockerfile already copies `src/Worms.Hub.Web/.` in full, so `public/` is picked up automatically.

If using an SVG, name the file `worm.svg` and reference it as `/worm.svg`. PNG is preferred for photographic/complex images; SVG for illustration. Either format is acceptable.

Record the image source URL and licence in a comment at the top of `LandingPage.tsx` for traceability.

### 2. Add react-router v7

Install the unified `react-router` package (v7 merged `react-router-dom` into a single package — no separate `react-router-dom` needed):

```
npm install react-router@^7
```

This adds `react-router` to `dependencies` in `package.json` and updates `package-lock.json`. Commit both files.

No `@types/react-router` package is needed — v7 ships its own TypeScript types.

### 3. Router setup in App.tsx

Replace the current `App.tsx` (which renders a bare `<Typography>`) with a `BrowserRouter` wrapping a route tree. Use the `createBrowserRouter` + `RouterProvider` pattern (the v7 data router API):

```tsx
import { createBrowserRouter, RouterProvider } from 'react-router'
import Layout from './components/Layout'
import LandingPage from './pages/LandingPage'

const router = createBrowserRouter([
    {
        path: '/',
        element: <Layout />,
        children: [
            {
                index: true,
                element: <LandingPage />,
            },
        ],
    },
])

function App() {
    return <RouterProvider router={router} />
}

export default App
```

`Layout` uses `<Outlet />` from `react-router` to render the matched child route between the Header and Footer.

### 4. Layout component

`src/Worms.Hub.Web/src/components/Layout.tsx` renders the page shell:

```tsx
import { Outlet } from 'react-router'
import Header from './Header'
import Footer from './Footer'
import Box from '@mui/material/Box'

function Layout() {
    return (
        <Box sx={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
            <Header />
            <Box component="main" sx={{ flex: 1 }}>
                <Outlet />
            </Box>
            <Footer />
        </Box>
    )
}

export default Layout
```

The `minHeight: '100vh'` + `flex: 1` on `main` ensures the footer always sits at the bottom of the viewport.

### 5. Header component

`src/Worms.Hub.Web/src/components/Header.tsx` uses MUI `AppBar` and `Toolbar`. The structure accommodates future auth-state content (e.g. a Sign Out button or user name) by leaving a `Box` with `sx={{ flexGrow: 1 }}` after the branding — content added there will push to the right:

```tsx
import AppBar from '@mui/material/AppBar'
import Toolbar from '@mui/material/Toolbar'
import Typography from '@mui/material/Typography'
import Box from '@mui/material/Box'

function Header() {
    return (
        <AppBar position="static">
            <Toolbar>
                <Typography variant="h6" component="div">
                    Worms Hub
                </Typography>
                <Box sx={{ flexGrow: 1 }} />
                {/* Future: auth-state content (sign out button, user name, etc.) */}
            </Toolbar>
        </AppBar>
    )
}

export default Header
```

### 6. Footer component

`src/Worms.Hub.Web/src/components/Footer.tsx` displays a copyright statement. The year is computed dynamically so the footer never requires a code change to stay current:

```tsx
import Box from '@mui/material/Box'
import Typography from '@mui/material/Typography'

function Footer() {
    return (
        <Box component="footer" sx={{ py: 2, textAlign: 'center' }}>
            <Typography variant="body2" color="text.secondary">
                &copy; {new Date().getFullYear()} Worms Hub
            </Typography>
        </Box>
    )
}

export default Footer
```

### 7. LandingPage component

`src/Worms.Hub.Web/src/pages/LandingPage.tsx` centres the worm image, the "Worms Hub" heading, and the Sign In button vertically and horizontally. The Sign In button has no `onClick` handler — clicking it is inert. The next slice will wire it up:

```tsx
// Image source: <URL> — <Licence>
import Box from '@mui/material/Box'
import Button from '@mui/material/Button'
import Typography from '@mui/material/Typography'
import Stack from '@mui/material/Stack'

function LandingPage() {
    return (
        <Box
            sx={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                justifyContent: 'center',
                minHeight: '60vh',
                gap: 3,
            }}
        >
            <Stack spacing={3} alignItems="center">
                <img
                    src="/worm.png"
                    alt="Worm"
                    style={{ maxWidth: 300, height: 'auto' }}
                />
                <Typography variant="h2" component="h1">
                    Worms Hub
                </Typography>
                <Button variant="contained" size="large">
                    Sign in
                </Button>
            </Stack>
        </Box>
    )
}

export default LandingPage
```

Adjust `src="/worm.png"` to match the actual filename if using SVG.

### 8. nginx.conf for SPA routing

Slice 03 learnings noted that `try_files $uri $uri/ /index.html;` is required once client-side routing is introduced. Create `build/web/nginx.conf`:

```nginx
server {
    listen       80;
    server_name  localhost;

    root   /usr/share/nginx/html;
    index  index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }
}
```

This replaces nginx's default config, which would return 404 for deep routes.

### 9. Dockerfile update

Copy the custom nginx config into the runtime image. Add a `COPY` instruction to the runtime stage in `build/web/Dockerfile`:

```dockerfile
#### Runtime ####
FROM nginx:alpine@sha256:5616878291a2eed594aee8db4dade5878cf7edcb475e59193904b198d9b830de
COPY --from=build /repo/.artifacts/web /usr/share/nginx/html
COPY build/web/nginx.conf /etc/nginx/conf.d/default.conf
```

The `COPY` for `nginx.conf` uses a path relative to the Docker build context (the repo root), consistent with how other build files are referenced.

### 10. Update index.html title

Change `<title>Worms League</title>` to `<title>Worms Hub</title>` in `src/Worms.Hub.Web/index.html`. This is a cosmetic fix that aligns the browser tab with the app name used everywhere else in this slice.

### 11. Prettier formatting

All new `.tsx` files must pass `npx prettier --check src`. Run `npx prettier --write src` from `src/Worms.Hub.Web/` after writing all components to ensure consistent formatting before verifying lint. The project uses Prettier defaults (no `.prettierrc` overrides beyond what was committed in slice 01 — verify with `cat src/Worms.Hub.Web/.prettierrc` if present).

---

## Verification

1. Run `npm install react-router@^7` from `src/Worms.Hub.Web/` and confirm `package.json` shows `"react-router": "^7.x.x"` in `dependencies`.
2. Run `make web.build` from the repo root — must complete with no errors (tsc + vite build).
3. Run `make web.lint` from the repo root — ESLint, tsc --noEmit, and Prettier must all pass with no errors.
4. Run `docker compose build web` — the multi-stage Docker build must succeed with the nginx.conf COPY in place.
5. Run `docker compose up web` and navigate to `http://localhost:3000/` in a browser:
   - The worm image is visible and centred.
   - "Worms Hub" appears as the primary heading below the image.
   - A "Sign in" button is visible below the heading; clicking it produces no navigation and no error.
   - The header bar is visible at the top, showing "Worms Hub" branding.
   - The footer is visible at the bottom, showing a copyright statement with the current year.
6. Navigate directly to `http://localhost:3000/some/nonexistent-path` — nginx should serve `index.html` (React Router renders; no nginx 404 page).
