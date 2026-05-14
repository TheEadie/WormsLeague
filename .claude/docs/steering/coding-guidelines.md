# Coding Guidelines

Cross-cutting conventions that apply to every project in this repo. Component-specific patterns live in the relevant doc under `.claude/docs/components/`.

## Build defaults

`Directory.Build.props` enables for every project:

- `Nullable=enable`, `ImplicitUsings=enable`
- `AnalysisLevel=latest-All` plus `Roslynator.Analyzers`
- `JetBrains.Annotations` (with `PrivateAssets="All"` — annotations only, no runtime dep)

Treat warnings as errors. CI builds with `--warnaserror`; if a warning surfaces locally, fix it rather than suppressing it.

## Visibility

Default to `internal sealed` for new types. Promote to `public` only when the type is actually consumed from another assembly. Mark types that look unused but are wired up reflectively (e.g. via DI, JSON, attributes) with `[PublicAPI]`.

## Records and immutability

Use `record` for DTOs, queue messages, and value-like types. Prefer `IReadOnlyList<T>` / `IReadOnlyDictionary<,>` over the mutable interfaces in public-facing signatures.

## Dependency injection

Each project exposes a single `ServiceRegistration` static class with one `Add<Project>Services(this IServiceCollection)` extension that returns `IServiceCollection` to allow chaining. Register services as `Scoped` unless there is a specific reason to use `Singleton` or `Transient`. Higher-level projects pull in the `Add*Services` of the projects they depend on.

## File system access

Take a dependency on `System.IO.Abstractions` (`IFileSystem`) rather than calling `File`/`Directory` statics directly. This keeps file-touching code unit-testable with `MockFileSystem`.

## Testing

- Test projects are named `<ProjectUnderTest>.Tests` and use **NUnit**.
- Test classes are named `<TypeUnderTest>Should` (e.g. `GifCreatorShould`, `ReplayTextReaderShould`); test methods describe a behaviour (e.g. `ThrowWhenNoFramesAreProduced`).
- Tests that require external infrastructure (Docker, a real WA install, etc.) are tagged `[Category("Integration")]`. Unit runs use `--filter "Category!=Integration"`; integration runs use `--filter Category=Integration`.

## Telemetry

OpenTelemetry is used across the hub. New code that performs a meaningful unit of work should start an `Activity` from the project's `Telemetry.Source` and tag it with relevant identifiers (`Activity.Current?.SetTag(...)`) rather than logging the same data ad-hoc.

## Formatting

Enforced by `.editorconfig`:
- 4-space indent, UTF-8, LF, trim trailing whitespace
- 120-character line length
- `using` directives sorted, `System.*` first, no blank line between groups
- Allman braces (new line before `{`, `catch`, `else`, `finally`)
- Always use braces on `if`, `else`, `for`, `foreach`, and `while` blocks, even for single-statement bodies. InspectCode enforces this rule and `dotnet build --warnaserror` will surface violations before pushing.
