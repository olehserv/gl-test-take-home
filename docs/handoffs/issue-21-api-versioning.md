# Handoff — Issue #21: Real API versioning for the IngestionApi

- **Issue:** #21 (link) — part of #3 (step 7)
- **Branch:** `feat/api-versioning`
- **Owner:** Oleh Shevtsiv
- **Status:** ready for review

## Goal
Replace the hardcoded `v1` URL segment with real API versioning using the official
`Asp.Versioning` library, so the version is declared once (not a magic string repeated per
route) and a future `v2` is a clean add. Existing URLs (`/api/v1/measurements`) and behaviour
must keep working; versions should be visible in Swagger.

## Current state (what to change)
- `src/IngestionApi/Endpoints/MeasurementEndpoints.cs` hardcodes the version in each route:
  `MapPost("/api/v1/measurements", …)` and `MapGet("/api/v1/measurements", …)`.
- No versioning mechanism: there is no way to declare/advertise versions or add `v2` cleanly.
- Auth is applied via `RequireApiKeyMetadata` on the POST; `/healthz` is unversioned and open.

## Scope
- [x] Add packages (central versions in `Directory.Packages.props`): `Asp.Versioning.Http`
      and `Asp.Versioning.Mvc.ApiExplorer` (the latter so Swagger groups by version and
      substitutes the version into the URL). Reference both from `IngestionApi.csproj`.
- [x] `Program.cs`: `AddApiVersioning` (default `1.0`, `AssumeDefaultVersionWhenUnspecified`,
      `ReportApiVersions`, `UrlSegmentApiVersionReader`) + `AddApiExplorer`
      (`GroupNameFormat = "'v'VVV"`, `SubstituteApiVersionInUrl = true`).
- [x] `MeasurementEndpoints.cs`: build a version set with `HasApiVersion(1.0)`, map a
      group `"/api/v{version:apiVersion}"` with `.WithApiVersionSet(...)`, and map the
      measurement endpoints **relatively** (`/measurements`) under it. Keep
      `RequireApiKeyMetadata` on the POST.
- [x] `/healthz` stays unversioned and open.
- [x] Configure Swagger so v1 renders cleanly (per-version docs via `ConfigureSwaggerOptions`
      iterating `IApiVersionDescriptionProvider`).

## Done so far
- [x] Added `Asp.Versioning.Http` + `Asp.Versioning.Mvc.ApiExplorer`; wired `AddApiVersioning`
      + `AddApiExplorer` in `Program.cs`.
- [x] `MeasurementEndpoints.cs` now maps a version set + `"/api/v{version:apiVersion}"` group;
      POST keeps `RequireApiKeyMetadata`. `/healthz` left unversioned.
- [x] `Swagger/ConfigureSwaggerOptions.cs` adds a Swagger doc per discovered version;
      `UseSwaggerUI` lists each version group.
- [x] `dotnet build` + `dotnet test` green (16 unit + 7 integration; integration tests hit
      `/api/v1/measurements` and pass — URL-segment versioning maps to 1.0).

## Verification
- Build green; all 23 tests pass (the integration suite is the regression net for the route
  change and the auth metadata move).
- Ran the API and confirmed at runtime: `GET /swagger/v1/swagger.json` → 200 and lists
  `/api/v1/measurements` at version `1.0` (so `SubstituteApiVersionInUrl` works);
  `GET /api/v1/measurements` (no key) → 200 (existing URL + open-endpoint behaviour preserved).

## Next steps
1. Commit (`feat:`) and open a PR.
2. Tick step 7 / mark `#21` in the Sub-issues list in `issue-3-test-task.md`.

## Key decisions
- **URL-segment versioning** (`/api/v{version:apiVersion}`) — keeps the existing
  `/api/v1/...` URLs working unchanged, is explicit, and is cache/proxy friendly. Chosen over
  header/media-type versioning to avoid changing how current clients (DesktopApp,
  DeviceSimulator) call the API.
- `AssumeDefaultVersionWhenUnspecified` + default `1.0` so any unversioned call still resolves
  (back-compat), and `ReportApiVersions` so responses advertise supported versions.
- `/healthz` is intentionally left outside the version set (infrastructure endpoint).

## Open questions / risks
- The POST's auth depends on endpoint metadata (`RequireApiKeyMetadata`). After moving to
  group mapping it must remain attached — the integration test `POST no key → 401` guards this.
- Route changes from a literal prefix to a template; the round-trip / 202 / 400 integration
  tests (all hitting `/api/v1/measurements`) are the regression net.
- Picking exact package versions: take a recent `Asp.Versioning.*` 8.x and let `dotnet restore`
  confirm (don't hand-pick blindly).

## How to verify
```bash
dotnet build
dotnet test
```
Expected:
- Build green.
- All integration tests still pass against `/api/v1/measurements` (202/401/400/round-trip).
- Swagger UI shows the v1 group with `/api/v1/measurements` routes.

## Notes for the next session (AI or human)
Endpoints are in `src/IngestionApi/Endpoints/MeasurementEndpoints.cs`
(`MapMeasurementEndpoints`); `Program.cs` is the composition root. Auth in
`src/IngestionApi/Auth/ApiKeyMiddleware.cs` keys off `RequireApiKeyMetadata`. Keep
`Program.cs` thin (CLAUDE.md rule 4). Parent context: `issue-3-test-task.md`.
