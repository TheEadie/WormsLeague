# Dark Mode

## Overview

The UI automatically applies MUI's dark or light palette based on the user's OS colour scheme preference, and exposes a three-way toggle in the Header so the user can override that preference. The chosen mode is persisted in `localStorage` so it survives page reloads and browser restarts. All existing and future pages benefit without any per-page changes.

## Requirements

- The UI defaults to the OS colour scheme preference (`prefers-color-scheme` media query) when no stored preference exists.
- The user can override the colour scheme via a single icon button in the Header that cycles through three states: light, dark, and system (follow OS).
- The button displays the icon for the current mode: a sun icon for light, a moon icon for dark, and a half-sun/half-moon icon for system.
- Each click advances to the next state in the cycle: light → dark → system → light.
- The selected mode is stored in `localStorage` and applied on subsequent page loads without reverting to the OS default.
- When the stored preference is "system", changes to the OS colour scheme are reflected immediately without requiring a page reload.
- MUI's built-in dark and light palettes are used; no custom brand colours are introduced.
- All existing pages (currently: the public landing page with its Header and Footer) and all future pages automatically use the active palette by virtue of being inside the existing `ThemeProvider`.

## Out of Scope

- A two-way toggle (light/dark only) — the system option is required.
- Custom or branded colour overrides beyond MUI's defaults.
- Storing the preference server-side or syncing it across devices.
- A colour scheme setting anywhere other than the Header.
- Animating or transitioning between themes.

## Acceptance Criteria

- Given a browser with no stored preference and an OS set to dark mode, when the app loads, the dark palette is applied.
- Given a browser with no stored preference and an OS set to light mode, when the app loads, the light palette is applied.
- Given the current mode is light, when the user clicks the button, the mode advances to dark, the moon icon is shown, and "dark" is written to `localStorage`.
- Given the current mode is dark, when the user clicks the button, the mode advances to system, the half-sun/half-moon icon is shown, and "system" is written to `localStorage`.
- Given the current mode is system, when the user clicks the button, the mode advances to light, the sun icon is shown, and "light" is written to `localStorage`.
- Given "light" is stored in `localStorage`, when the app loads on a device whose OS is set to dark mode, the light palette is applied (stored preference wins over OS).
- Given "system" is stored in `localStorage` and the OS colour scheme changes (e.g. automatic sunrise/sunset), the palette updates without a page reload.
- The Header shows a single icon button whose icon reflects the current mode.
- The landing page (Header, hero section, Footer) is readable and visually coherent in both light and dark palettes.
