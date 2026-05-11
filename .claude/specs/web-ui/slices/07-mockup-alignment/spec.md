# Mockup Alignment

## Overview

Restyle the landing page, header, and footer to match the Claude Design mockups in `.claude/specs/web-ui/design/`, establishing the shared chrome (brand, typography, layout) that subsequent authenticated pages will sit inside. The page remains read-only and the sign-in button remains non-functional — wiring it up is a later slice.

## Requirements

### Landing page

- A two-column layout: a hero column on the left and a sign-in column on the right.
- The hero column contains, in order:
  - The existing worm hero image (`public/worm.png`), placed above the headline as a decorative visual.
  - A headline reading "Every shot. Every kill. Archived." with prominent typography.
- The sign-in column contains a sign-in card with:
  - A heading "Sign in to continue".
  - Supporting body text indicating that the site is for league members only.
  - A primary "Sign in" button that is the visual focal point of the card but is not wired to any authentication flow in this slice.
  - A divider labelled "League access".
  - A single info row reading "Need an invite?" with the value "Contact Eadie on Slack".
- On narrow viewports the two columns stack vertically (hero on top, sign-in card below).

### Header

- Replaces the current plain `AppBar` + title with a brand block consisting of the worm image (`public/worm.png`, scaled down) alongside a "Worms Hub" wordmark.
- Retains the existing colour scheme picker on the right.
- Uses the visual treatment from the mockup's top bar (sticky position, bottom divider, paper background) so it works as the chrome for all future pages.

### Footer

- Retains the existing copyright line as its content.
- Restyled to be visually consistent with the new chrome (e.g. top border, muted typography), rather than the current bare centred text.

### Typography

- JetBrains Mono is added as a webfont and used for the accent typography called out in the mockups (e.g. the divider caption "LEAGUE ACCESS").
- Attribution for the JetBrains Mono font is included wherever the font's license requires it.

### Theming

- All changes work correctly under both light and dark MUI palettes (the existing OS-preference behaviour from the Dark Mode slice continues to apply).

## Out of Scope

- Wiring the "Sign in" button to any authentication flow (covered by the **Browser sign-in** slice).
- Any of the authenticated-only chrome shown in the mockup top bar: breadcrumbs, league menu, league-scoped nav items (Matches / Leaderboard / Players / Awards), the total-matches chip, and the profile chip.
- The mockup landing page's stats strip ("Matches / Players / Damage / Turns").
- The mockup landing page's "Season N · Week N live" status chip.
- The mockup landing page's version caption ("v0.4.0 · Private build · Updated 2 min ago").
- The mockup sign-in card's "Replay uploader" info row and Privacy / House rules links.
- The mockup landing page's body paragraph beneath the headline ("The private replay vault for…").
- The mockup landing page's WormBobber SVG decoration.
- Any new pages, routes, or components beyond the existing landing page, header, and footer.
- Any Gateway API changes.

## Acceptance Criteria

- Given a visitor opens the site, when the landing page renders on a wide viewport, then the hero and sign-in card appear side-by-side with the hero on the left.
- Given a visitor opens the site on a narrow viewport, when the landing page renders, then the hero and sign-in card stack vertically with the hero on top.
- Given the landing page is rendered, when the hero column is inspected, then it contains the worm hero image above the headline and the headline "Every shot. Every kill. Archived." — and nothing else.
- Given the landing page is rendered, when the sign-in card is inspected, then it contains the heading "Sign in to continue", supporting body text, a primary "Sign in" button, a "League access" divider, and a single row reading "Need an invite? — Contact Eadie on Slack" — and nothing else.
- Given the landing page is rendered, when the "Sign in" button is clicked, then nothing happens (it is not wired to an auth flow in this slice).
- Given any page in the SPA renders, when the header is inspected, then it shows the worm image, the "Worms Hub" wordmark, and the colour scheme picker, with the styling from the mockup's top bar.
- Given any page in the SPA renders, when the footer is inspected, then it shows the existing copyright line styled consistently with the new chrome.
- Given the user's OS prefers dark mode, when the landing page, header, and footer render, then they display correctly under the MUI dark palette; the same is true under the light palette.
- Given the site is loaded, when fonts are inspected, then JetBrains Mono is loaded and applied to the accent typography called out in the mockups, with any required attribution present.
