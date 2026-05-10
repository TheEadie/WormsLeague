# Learnings: Dark Mode

## Implementation Notes

### Everything went exactly as planned

All steps in the plan executed without deviation:

- `npm install @mui/icons-material@^9.0.1` added 1 package cleanly (it was the only missing peer).
- The `main.tsx` theme update compiled and built without TypeScript errors.
- The `ColourSchemePicker` component was accepted by TypeScript with no type issues — `useColorScheme()` is correctly typed under the CSS-variables theme.
- Prettier left all new and modified files unchanged on the `--write` pass, indicating the hand-written formatting already matched Prettier's output.
- `make web.build` succeeded (478 modules transformed), and `make web.lint` passed (ESLint, tsc, and Prettier all clean).

No surprises, no deviations from the plan, no decisions were needed beyond what the plan specified.
