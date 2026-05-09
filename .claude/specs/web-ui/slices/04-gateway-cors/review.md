# Review — Gateway CORS

## Verdict

The implementation fully satisfies the spec. All three planned files are modified exactly as described, the build exits clean with zero warnings (confirmed via `dotnet build --warnaserror`), and every acceptance criterion is met. There is one minor note on the `corsPolicyName`/`allowedOrigins` placement relative to the plan, which `learnings.md` already explains. No blockers. Ready to merge.

## Acceptance Criteria

| Criterion | Status | Evidence |
|---|---|---|
| Requests from `http://localhost:3000` or `http://localhost:5173` receive `Access-Control-Allow-Origin` in Development | MET | `appsettings.Development.json` lists both origins; `Program.cs` registers and applies the policy |
| Preflight `OPTIONS` with `Authorization` in `Access-Control-Request-Headers` → 204 with `Authorization` in `Access-Control-Allow-Headers` | MET | `Program.cs:34` — `.WithHeaders("Authorization", "Content-Type")` |
| Request from an unlisted origin does not receive `Access-Control-Allow-Origin` | MET | `appsettings.json:10-12` — base config has an empty `AllowedOrigins` array; no fallback |
| `WORMS_CORS__ALLOWEDORIGINS` env var drives the allowed origins at runtime | MET | `Program.cs:26` — `builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()` reads from the merged config; `AddEnvironmentVariables("WORMS_")` is already wired and maps `__` to `:` |
| Base `appsettings.json` with no env var override emits no CORS headers | MET | `appsettings.json:10-12` — `"AllowedOrigins": []`; `.WithOrigins()` called with empty array → policy matches nothing |
| Existing authenticated API calls (JWT bearer from CLI) continue to succeed | MET | Auth, authorization, and controller registration are untouched; `UseCors` is inserted between `UseRouting` and `UseAuthentication`, which is the correct ASP.NET Core order and does not affect the auth pipeline |

## Scope

The diff touches exactly the three files listed in the plan's "Files to Create / Modify" table:

- `src/Worms.Hub.Gateway/Program.cs`
- `src/Worms.Hub.Gateway/appsettings.json`
- `src/Worms.Hub.Gateway/appsettings.Development.json`

No new files were created. No files outside the plan were modified.

One deviation from the plan is documented in `learnings.md`: `corsPolicyName` and `allowedOrigins` are declared at the top level before the first `if (runGateway)` block rather than inside it. The plan acknowledged they needed to be in scope for both the service-registration and app-configuration blocks; the implementation chose the simplest correct placement. This is resolved — no issue.

A second deviation is also documented: the plan described adding an explicit `UseRouting()` call, noting the existing code lacked one. In fact `UseRouting()` was already present, so only `UseCors(corsPolicyName)` was inserted between it and `UseAuthentication()`. This is resolved — no issue.

## Blockers

None.

## Suggestions

None.

## Nitpicks

None.

## Tests

No tests were added. The plan and `learnings.md` both note this is intentional: CORS behaviour lives in ASP.NET Core middleware and requires a running HTTP stack, making pure unit testing impractical. The testing strategy doc confirms that Gateway behaviour is exercised via integration tests and smoke testing. Verification is via `curl` against the local dev stack as described in the plan's Verification section. This is appropriate for the scope of the change.

## Recommended Actions

No findings to act on.
