# Coding Guidelines

Cross-cutting conventions that apply to every project in this repo. Component-specific patterns live in the relevant doc under `.claude/docs/components/`.

## Build defaults

`Directory.Build.props` enables for every project:

- `Nullable=enable`, `ImplicitUsings=enable`
- `AnalysisLevel=latest-All` plus `Roslynator.Analyzers`
- `JetBrains.Annotations` (with `PrivateAssets="All"` — annotations only, no runtime dep)

Treat warnings as errors. CI builds with `--warnaserror`; if a warning surfaces locally, fix it rather than suppressing it.

Plan snippets must be written so the code compiles under the analyser set rather than leaving the implementer to discover each rule mid-flight. The rules this codebase routinely trips, with the canonical fix:

- **RCS1124 (inline local variable)** — fold a single-use local back into its consumer.
- **RCS1146 (use conditional access)** — `x?.Y()` over `if (x != null) x.Y();`.
- **RCS1077 (optimise LINQ)** — on a `List<T>`, prefer `list.ConvertAll(selector)` over `list.Select(selector).ToList()`.
- **CA1031 (catch general exception)** — when catching `Exception` is deliberate, the `[SuppressMessage]` goes on the *enclosing method*, not the `catch` clause.
- **CA1062 (validate public arguments)** — public methods with reference-type parameters need `ArgumentNullException.ThrowIfNull(...)`; this includes positional parameters on public records.
- **CA1305 (specify IFormatProvider)** — pass `CultureInfo.InvariantCulture` to `ToString`/`Parse`/`int.ToString()` etc., including for integer formatting where it looks redundant.
- **CA1852 (seal internal types)** — `internal` types must be `sealed` unless intentionally inherited from.

## Visibility

Default to `internal sealed` for new types. Promote to `public` only when the type is actually consumed from another assembly. Mark types that look unused but are wired up reflectively (e.g. via DI, JSON, attributes) with `[PublicAPI]`.

## Records and immutability

Use `record` for DTOs, queue messages, and value-like types. Prefer `IReadOnlyList<T>` / `IReadOnlyDictionary<,>` over the mutable interfaces in public-facing signatures.

Positional record parameters do not take default values, even when the parameter is nullable. Every construction site passes the value explicitly (e.g. `null` for an unknown name). This forces adding a new optional field to be a deliberate change at every caller rather than silently inheriting an implicit default.

When two or more callsites perform the same null-guarded comparison on the same record field (e.g. `team.ClaimedBy != null && team.ClaimedBy == subject`), extract the predicate as a method on the record (`IsClaimedBy(subject)`, `IsClaimedByAnother(subject)`). Reviewers should grep touched files for repeated property-access patterns before approving.

## Dependency injection

Each project exposes a single `ServiceRegistration` static class with one `Add<Project>Services(this IServiceCollection)` extension that returns `IServiceCollection` to allow chaining. Register services as `Scoped` unless there is a specific reason to use `Singleton` or `Transient`. Higher-level projects pull in the `Add*Services` of the projects they depend on.

When a plan snippet introduces an `await` into a previously synchronous method, or a void DI call (`TryAddScoped`, `services.AddX()`) into an expression-bodied method, the plan must restate the changed signature/body — return type becomes `Task<...>`, `async` keyword is added, expression-body collapses to a block — so reviewers catch the cascade in the plan rather than the implementer rediscovering it.

## Naming and abstractions

Identifiers exposed through domain records, DTOs, repository APIs, and SQL column names describe the concept, not the current provider or vendor. Use `AuthSubject` / `auth_subject`, not `Auth0Subject` / `auth0_subject`. Domain and schema names are long-lived; vendor names rot the moment the provider is swapped.

When a feature flag has already shaped the domain object (e.g. a collection is `null` when the flag is off, populated when it is on), the DTO projection mirrors the domain's nullability rather than re-checking the flag. Reviewers challenge any flag check that appears below the layer that first read the flag — re-gating at the DTO layer is duplication, and it conflates "data unavailable" with "available but empty".

When a new collection is added to a resource or DTO that overlaps an existing one (e.g. `Teams` and `Placements`, where every `Placement` carries its `Team`), verify each consumer still needs both. The default is to collapse to a single source of truth; carry both only with a stated reason.

## File system access

Take a dependency on `System.IO.Abstractions` (`IFileSystem`) rather than calling `File`/`Directory` statics directly. This keeps file-touching code unit-testable with `MockFileSystem`.

## Testing

- Test projects are named `<ProjectUnderTest>.Tests` and use **NUnit**.
- Test classes are named `<TypeUnderTest>Should` (e.g. `GifCreatorShould`, `ReplayTextReaderShould`); test methods describe a behaviour (e.g. `ThrowWhenNoFramesAreProduced`).
- Tests that require external infrastructure (Docker, a real WA install, etc.) are tagged `[Category("Integration")]`. Unit runs use `--filter "Category!=Integration"`; integration runs use `--filter Category=Integration`.

## Telemetry

OpenTelemetry is used across the hub. New code that performs a meaningful unit of work should start an `Activity` from the project's `Telemetry.Source` and tag it with relevant identifiers (`Activity.Current?.SetTag(...)`) rather than logging the same data ad-hoc.

## Planning and review

Acceptance criteria written in absolute terms ("no effect", "always", "never") must either be provably enforceable by the chosen algorithm or be rewritten to describe the algorithm's actual contract. Tests are named after what they assert, not the aspirational behaviour the spec wished for.

Any new domain field that represents a computed rank, position, score, or other derived value must enumerate the "cannot be computed" case (retired game, no data, tie unbreakable) at the spec stage. The nullability decision is made once at the domain layer and propagated through migration column constraints, DTO types, CLI rendering, and TS types in the same slice — not discovered later when a `NOT NULL` column meets a real-world null.

Any change to a public method signature, a shared positional record, or a shape illustrated in a component doc carries a doc edit in the same slice. Plans list the doc file alongside the code files; the doc update is not a follow-up. Shared positional records often have direct constructor callers outside their owning assembly — plans that change such records must grep across the CLI and Hub source trees for direct constructor calls before declaring the change self-contained.

Once a slice's `review` sticky comment is written and the slice is marked complete, additional scope landing in the same PR must either trigger a new spec/plan/review cycle within the PR or move to a follow-up slice. A bare DTO field added as `null` because "we'll fix it later" is a spec, not a drive-by.

## Formatting

Enforced by `.editorconfig`:
- 4-space indent, UTF-8, LF, trim trailing whitespace
- 120-character line length
- `using` directives sorted, `System.*` first, no blank line between groups
- Allman braces (new line before `{`, `catch`, `else`, `finally`)
- Always use braces on `if`, `else`, `for`, `foreach`, and `while` blocks, even for single-statement bodies. InspectCode enforces this rule and `dotnet build --warnaserror` will surface violations before pushing.
