# Learnings: Game Detail Page

## Implementation Notes

### MUI v9 does not accept `fontWeight` or `display` as direct props on `<Typography>`

The plan's code examples (taken from the design mockup) pass `fontWeight={700}` and `display="block"` as direct JSX props on `<Typography>`. In MUI v9, these are not part of the typed `TypographyProps` interface and TypeScript rejects them. The correct approach — consistent with the rest of the codebase (e.g. `LeagueDetailPage.tsx`) — is to put them inside the `sx` prop: `sx={{ fontWeight: 700 }}` and `sx={{ display: 'block', ... }}`. This applies to any scalar MUI system shorthand that was usable as a direct prop in earlier MUI versions.

### `primaryTypographyProps` is removed from `<ListItemText>` in MUI v9

The plan specifies `primaryTypographyProps={{ fontSize: 13, fontWeight: ... }}` on `<ListItemText>`. This prop was removed in MUI v5→v6 in favour of `slotProps`. In MUI v9 the TypeScript type has no `primaryTypographyProps` property at all and the build fails. The fix is:

```tsx
slotProps={{
    primary: {
        style: { fontSize: 13, fontWeight: activePanel === i ? 700 : 400 },
    },
}}
```

Using `style` (inline React style) inside `slotProps.primary` is safe for simple scalar overrides where `sx` is not available.

### `AddWormsArmageddonFilesServices` was removed from `AddWorkerServices` and added to `Program.cs`

The plan correctly identified the duplicate-registration risk. Moving the call to `Program.cs` (unconditional, outside both `AddGatewayServices` and `AddWorkerServices`) ensures exactly one registration regardless of which mode(s) are active. The `using Worms.Armageddon.Files;` import also had to be removed from `ServiceRegistration.cs` (it was no longer needed there) and added to `Program.cs`.

### Prettier reformatted `GameDetailPage.tsx` after initial write

The authored file had minor formatting deviations from Prettier's output (brace placement, trailing commas, line wrapping). Running `npx prettier --write src/pages/GameDetailPage.tsx` was required to pass `make web.lint`. As noted in the slice 11 learnings, Prettier must be run on any new `.tsx` file before committing.

### Gateway fully qualified namespace used for `ReplayResource` in controller

The controller uses `ReplayResource` from `Worms.Armageddon.Files.Replays` namespace. Rather than adding a `using` directive that could conflict with other types in scope, the fully qualified name `Worms.Armageddon.Files.Replays.ReplayResource? parsed` was used in the action method body. This is a minor style deviation from the plan snippet but avoids any ambiguity.

## Files Added (not in plan)

None — all files created or modified were listed in the plan.
