# Learnings: Authenticated Route Gate

## Implementation Notes

### Prettier expanded the RequireAuth wrapping in App.tsx to multi-line

The plan showed the `/authenticated` route inline as:

```tsx
{ path: 'authenticated', element: <RequireAuth><AuthenticatedPage /></RequireAuth> },
```

Prettier expanded this to a multi-line form with the JSX element on its own lines inside parentheses. This is expected Prettier behaviour for longer JSX expressions and required no manual adjustment — running Prettier before `make web.lint` handled it cleanly, as in slice 08.

### Everything else proceeded exactly as planned

- `RequireAuth.tsx` created without deviation from the plan's code sample.
- `App.tsx` updated with the import and wrapper exactly as described.
- `make web.build` and `make web.lint` both passed on the first attempt with no TypeScript or ESLint errors.
