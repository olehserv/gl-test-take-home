# Handoff — Issue #17: Real integration tests for the IngestionApi

- **Issue:** #17 (link) — part of #3 (step 5)
- **Branch:** `test/api-integration-tests`
- **Owner:** Oleh Shevtsiv
- **Status:** ready for review

## Goal
Replace the placeholder integration test with real end-to-end tests that exercise the
HTTP stack via `WebApplicationFactory<Program>`: auth (401), validation (400), the happy
path (202), and a POST→GET round-trip. Each test runs against a fresh in-memory store so
tests don't leak state into each other.

## Scope
- [x] Inject the API key from the test (not the shipped `local-dev` dev default) via the
      factory, per the note carried over from #11.
- [x] Fresh `InMemoryMeasurementStore` per test (the store is a singleton, so isolation
      comes from a new factory/server per test).
- [x] Tests:
- [x] `GET /healthz` (no key) → 200.
- [x] `GET /api/v1/measurements` (no key) → 200 (endpoint is intentionally open).
- [x] `POST` valid measurement + key → 202 Accepted, `Location` header set.
- [x] `POST` without key → 401.
- [x] `POST` malformed JSON without key → 401 (auth runs before body binding).
- [x] `POST` invalid measurement + key → 400 with a per-field `errors` map (ProblemDetails).
- [x] POST→GET round-trip: a posted measurement is returned by the subsequent GET.

## Done so far
- [x] Implemented 7 tests in
      `tests/IngestionApi.IntegrationTests/MeasurementApiIntegrationTests.cs` (replacing
      the placeholder); covers 200 (health/GET) / 202 / 401 (missing key + malformed body)
      / 400 / round-trip.
- [x] API key injected via in-memory configuration; per-test isolation via a fresh factory
      per test (xUnit instance-per-test + `IDisposable`).
- [x] `dotnet build` + `dotnet test` green: 7 integration + 16 Domain unit tests pass.

## Next steps
1. Commit (`test: add API integration tests`) and open a PR.
2. Tick step 5 / add `#17` to the Sub-issues list in `issue-3-test-task.md`.

## Key decisions
- Configure the API key through the factory (in-memory configuration added last, so it
  overrides `appsettings.Development.json` regardless of environment) rather than relying
  on the shipped `local-dev` value — see the #11 risk note.
- Per-test isolation via a new factory per test (xUnit creates one test-class instance per
  test): a new server → a new singleton store. Simpler and more robust than resetting a
  shared store.
- `Measurement.Value` is `object`, so it deserializes to a `JsonElement` on the GET side.
  The round-trip test asserts on stable fields (e.g. `MeasurementId`/`Type`), not full
  record equality, to avoid `object` round-trip type mismatch.

## Open questions / risks
- `WebApplicationFactory` default environment may load `appsettings.Development.json`;
  adding the key via `ConfigureAppConfiguration`/in-memory (highest precedence) avoids any
  dependence on that. If flaky, pin the environment with `UseEnvironment("Testing")`.

## How to verify
```bash
dotnet build
dotnet test
```
Expected:
- Build green.
- `IngestionApi.IntegrationTests` covers 202 / 401 / 400 / round-trip and passes.
- `Domain.UnitTests` still passes (18).

## Notes for the next session (AI or human)
Endpoints live in `src/IngestionApi/Endpoints/MeasurementEndpoints.cs`; auth in
`src/IngestionApi/Auth/ApiKeyMiddleware.cs` (reads `Api:Key` once at startup, marks the
POST with `RequireApiKeyMetadata`, GET/health stay open). `Program` is `public` so
`WebApplicationFactory<Program>` works. Parent context: `issue-3-test-task.md`.
