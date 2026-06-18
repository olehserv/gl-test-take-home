# Handoff — Issue #3: Test task

- **Issue:** #3 (link)
- **Branch:** `feat/modernize-architecture`
- **Owner:** Oleh Shevtsiv
- **Status:** in progress

## Description
### Scenario
We build desktop software that integrates with medical diagnostic devices (vitals, ECG, spirometry). Today, several desktop apps (WPF) exchange data via local REST endpoints. We’re migrating from .NET Framework to modern .NET and stronger API contracts.  The provided Visual Studio solution is a simplified version of our software meant to illustrate proper implementation and good software engineering practices.  We have already updated the version of .NET to .NET 8 but need your help to modernize the architecture and maintainability of the system.

### Assignment

The team needs you to showcase how to
1.	Use MVVM (bindings, commands), DI, and an async HTTP client in our DesktopApp
1.	Move the domain logic out of our IngestionApi to a class library called Domain so it can be reused as the solution evolves
1.	Integration test the endpoints in our IngestionApi
1.	Unit test where applicable

### Further Context
We value automated testing across unit and integration levels for this effort, meaning E2E tests are optional.
Any additional architectural enhancements/refactoring that improve the performance, maintainability, or usability of the software are welcome.

## Goal
Modernize the architecture and maintainability of the .NET 8 solution. Showcase:
MVVM + DI + async HTTP client in `DesktopApp`; a reusable `Domain` class library
extracted from `IngestionApi`; integration tests for the API endpoints; and unit
tests where useful. Keep the code clean and idiomatic. Welcome small refactors that
improve performance, maintainability, or usability.

## Current problems (the work to undo)
- **DesktopApp:** uses blocking `WebClient`, no MVVM, no DI, swallows errors
  (`catch { }`), `List<dynamic>`, hardcoded URL, undisposed timer, Newtonsoft.
- **IngestionApi:** domain logic lives in the web project; API key hardcoded;
  validation returns a plain string instead of `ProblemDetails`; no logging.
- **Tests:** integration test is a fake; no unit tests.

## Target shape
```
src/Domain/            <- NEW lib, no web/UI/HTTP deps (model, validator, store)
src/IngestionApi/      <- thin web layer (routing, auth, DI), depends on Domain
src/DesktopApp/        <- MVVM + DI + typed async HttpClient
src/DeviceSimulator/   <- reuses Domain.Measurement
tests/Domain.UnitTests/                <- NEW
tests/IngestionApi.IntegrationTests/   <- real endpoint tests
```
Layering rule: everything depends on `Domain`; `Domain` depends on nothing.

## Next steps (small, verified commits)
0. **Introduce AI features** into solution.
1. **Prepare:** confirm `dotnet build` and `dotnet test` are green.
2. **Domain library:** create `src/Domain`, move `Measurement`,
   `MeasurementValidator`, `IMeasurementStore`, `InMemoryStore`; add reference
   `IngestionApi → Domain`; fix namespaces. → **tracked in #9**
   (`docs/handoffs/issue-9-extract-domain-library.md`).
3. **API refactor:** API-key check as an endpoint filter/middleware reading from
   config; return `ProblemDetails` on invalid input; keep `Program.cs` thin.
   → **tracked in #11** (`docs/handoffs/issue-11-api-refactor.md`).
4. **Unit tests:** `Domain.UnitTests` for `MeasurementValidator` (`[Theory]` +
   `[InlineData]`), optionally `InMemoryMeasurementStore` filtering.
5. **Integration tests:** real tests via `WebApplicationFactory<Program>` —
   202 / 401 / 400 / POST→GET round-trip; fresh store per test.
6. **DesktopApp:** typed `IMeasurementApiClient` (`IHttpClientFactory` +
   System.Text.Json), DI host in `App.xaml.cs`, `MainViewModel`
   (CommunityToolkit.Mvvm), bind `DataGrid` + `RefreshCommand`, show errors in a
   `Status` field. No logic in code-behind.
7. **Polish:** point `DeviceSimulator` at `Domain.Measurement`; write `SOLUTION.md`;
   confirm CI is green.
8. **Security hardening:** (raised in the #11 review)
   - Compare the API key in **constant time** (`CryptographicOperations.FixedTimeEquals`)
     and consider replacing the hand-rolled check with real ASP.NET
     authentication/authorization middleware (`AddAuthentication` + `RequireAuthorization`).
   - Serve Swagger only outside **Production** (`if (app.Environment.IsDevelopment())`).

## Sub-issues
- [x] #7 - **Introduce AI features** into solution.
- [x] **Prepare:** confirm `dotnet build` and `dotnet test` are green.
- [x] #9 — Extract domain logic into a `Domain` class library (step 2).
- [x] #11 — Refactor the API into a thin web layer (step 3).
- [x] #15 — Add unit tests (step 4).
- [x] #17 — Add real API integration tests (step 5). → ready for review
      (`docs/handoffs/issue-17-api-integration-tests.md`).

## Key decisions
- Keep `InMemoryMeasurementStore` in `Domain` for now (no framework dependency).

## Open questions / risks
- WPF runs only on Windows; verify the desktop app there before marking done.

## How to verify
```bash
dotnet build
dotnet test
```
Expected: solution builds; all unit + integration tests pass; `Domain` has no
ASP.NET/WPF package references.

## Notes for the next session (AI or human)
Read `CLAUDE.md` "Architecture rules" first, then this note. Work one numbered step
per commit, each leaving build + tests green. Never swallow exceptions — surface them.
