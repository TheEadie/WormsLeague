# Learnings: Gateway CORS

## Implementation Notes

### `corsPolicyName` and `allowedOrigins` declared outside the `if (runGateway)` block

The plan called for declaring `const string corsPolicyName` inside the `if (runGateway)` service-registration block and noted that it must be "naturally in scope" for the later `app` configuration block. In practice, `corsPolicyName` is used in both the `builder.Services.AddCors(...)` call and the `app.UseCors(corsPolicyName)` call. Both calls are inside `if (runGateway)` blocks, but they are separate blocks (the first before `builder.Build()`, the second after). To keep the variable in scope for both without introducing a field or repeating the string literal, it was declared at the top-level before the first `if (runGateway)` block — alongside the `allowedOrigins` read. This matches the plan's intent and is the simplest correct placement.

### `UseRouting()` was already present in Program.cs

The plan noted that the current code "does not call `UseRouting()` explicitly" and described adding it. In fact, `UseRouting()` was already present in `Program.cs` immediately before `UseAuthentication()`. `UseCors(corsPolicyName)` was inserted between the existing `UseRouting()` and `UseAuthentication()` calls with no other change needed. The plan's instruction to add explicit `UseRouting` was not required.

### Everything else went exactly as planned

The three file edits (Program.cs, appsettings.json, appsettings.Development.json) matched the plan exactly. The Gateway built cleanly on the first attempt with no warnings or errors.
