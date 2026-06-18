# Handoff — Issue #15: Add unit tests for the `Domain` library

- **Issue:** #15 (link) — part of #3 (step 4)
- **Branch:** `test/domain-unit-tests`
- **Owner:** Oleh Shevtsiv
- **Status:** ready for review

## Goal
Introduce the `tests/Domain.UnitTests` xUnit project and cover the `Domain` rules:
`MeasurementValidator` validation and `InMemoryMeasurementStore` query behaviour.
Pure tests — no production code changes.

## Scope
- [x] Create `tests/Domain.UnitTests` (references `src/Domain`; xunit + FluentAssertions
      and global usings come from `tests/Directory.Build.props`); add it to `TakeHome.sln`.
- [x] `MeasurementValidatorTests`: valid measurement returns no errors; one error per bad
      required field (`MeasurementId`, `Timestamp`, `DeviceId`, `Type`) via
      `[Theory]`/`MemberData`; all-bad-fields returns an error per field; a guard test
      documenting that `PatientId`/`Value`/`Unit` are intentionally **not** validated.
- [x] `InMemoryMeasurementStoreTests`: add-then-query round-trip; `since` excludes older
      items; case-insensitive `type` filter; null/empty/whitespace `type` does not filter
      (`[Theory]`/`[InlineData]`); the 500-item cap keeps the most recent.

## Done so far
- [x] Added the project and 18 tests; `dotnet test` → 18 passed.
- [x] Fixed the store tests to the current type name `InMemoryMeasurementStore` (they were
      written against the old `InMemoryStore` and did not compile) and renamed the
      file/class to `InMemoryMeasurementStoreTests` so it mirrors the type under test.
- [x] `dotnet build` and `dotnet test` green (18 Domain unit tests + 1 integration test).

## Next steps
1. Commit the changes (none committed yet on this branch).
2. Open a PR with a Conventional Commits title, e.g. `test: add Domain unit tests`.
3. Tick step 4 / add `#15` to the **Sub-issues** list in `issue-3-test-task.md`.

## Key decisions
- Invalid-field cases use `TheoryData`/`MemberData`, not `[InlineData]`: `Guid` and record
  values are not compile-time constants, so they cannot be inline attribute arguments. The
  store's blank-`type` cases (`null`/`""`/`"   "`) are plain strings, so they use `[InlineData]`.
- A deliberate test asserts `PatientId`/`Value`/`Unit` are not validated. It fails on purpose
  if a rule is ever added there, forcing the change to be conscious (documents the contract).
- Test class/file names mirror the type under test (`MeasurementValidatorTests`,
  `InMemoryMeasurementStoreTests`), consistent with `MeasurementApiIntegrationTests`.

## Open questions / risks
- None significant. The 500-cap test adds 501 measurements; it is fast and deterministic.
- PR title should be `test:` (adding tests), matching the `test/` branch prefix. The CI
  gate checks the PR **title** against the Conventional Commits set, not the branch name.

## How to verify
```bash
dotnet build
dotnet test
```
Expected:
- Build green.
- `Domain.UnitTests`: 18 passed, 0 failed.
- `IngestionApi.IntegrationTests` still passes (1).

## Notes for the next session (AI or human)
Tests target `Domain` only (no web/HTTP), per the layering rule in `CLAUDE.md`. Step 5
(real integration tests via `WebApplicationFactory`) is a separate issue. Parent context:
`issue-3-test-task.md`.
