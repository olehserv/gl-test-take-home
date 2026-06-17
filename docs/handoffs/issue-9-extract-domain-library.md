# Handoff — Issue #9: Extract domain logic into a `Domain` class library

- **Issue:** #9 (link) — part of #3
- **Branch:** `feat/domain-library`
- **Owner:** Oleh Shevtsiv
- **Status:** ready for review

## Goal
Move the domain logic out of the `IngestionApi` web project into a new `src/Domain`
class library that has **no** web/UI/HTTP dependencies, so it can be reused as the
solution evolves (for example by `DeviceSimulator`). `IngestionApi` behavior must
not change.

## Scope (what moves)
From `src/IngestionApi/` into `src/Domain/`:
- `Measurement` (record)
- `MeasurementValidator`
- `IMeasurementStore` + `InMemoryStore`

## Done so far
- [x] Created `src/Domain` class library; added to `TakeHome.sln`.
- [x] Moved `Measurement`, `MeasurementValidator`, `IMeasurementStore`, `InMemoryStore`
      into `Domain` (via `git mv`, history preserved); namespace → `Domain`.
- [x] Added project reference `IngestionApi → Domain`; added `using Domain;` to `Program.cs`.
- [x] `dotnet build` and `dotnet test` green; endpoints unchanged.

## Next steps
- Open a PR (Conventional Commits title).

## Key decisions
- Keep `InMemoryStore` in `Domain` for now — it is an in-memory domain detail with
  no framework dependency. Revisit if real persistence is added.

## How to verify
```bash
dotnet build
dotnet test
```
Expected:
- Solution builds; existing integration test project still compiles.
- `src/Domain/Domain.csproj` has **no** PackageReference to ASP.NET / WPF / HTTP.
- `IngestionApi` endpoints behave exactly as before.

## Notes for the next session (AI or human)
Read `CLAUDE.md` "Architecture rules" first — the layering rule (Domain depends on
nothing) is the whole point of this issue. This is a pure refactor: no behavior
change. Commit prefix: `feat:` (new project) or `refactor:` style under `chore:` —
the PR title must be Conventional Commits. Parent context: `issue-3-test-task.md`.
