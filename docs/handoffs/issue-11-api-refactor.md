# Handoff — Issue #11: Refactor the API into a thin web layer

- **Issue:** #11 (link) — part of #3
- **Branch:** `feat/api-refactor`
- **Owner:** Oleh Shevtsiv/
- **Status:** ready for review

## Goal
Make `IngestionApi` a thin, idiomatic web layer. Move the API-key check out of the
endpoint and read the key from configuration. Return structured `ProblemDetails` on
invalid input. Keep `Program.cs` as a composition root with thin endpoints. Public
endpoint behavior (status codes, routes) stays the same unless noted.

## Current state (what to change)
In `src/IngestionApi/Program.cs`:
- API key is **hardcoded**: `v == "local-dev"` inside `ValidateApiKey`. The check is
  written inline in the POST endpoint.
- Invalid input returns `Results.BadRequest("invalid measurement")` — a plain string,
  not `ProblemDetails` (even though `AddProblemDetails()` is already registered).
- All endpoint logic lives directly in `Program.cs`.

## Scope
- [x] Read the API key from configuration (`Api:Key`). Dev default lives in
      `appsettings.Development.json` only — **not** in base `appsettings.json` — so
      Production must supply it (env / user-secrets) or the app fails fast at startup.
- [x] API-key check is `Auth/ApiKeyMiddleware.cs` (middleware), applied only to
      endpoints tagged with `RequireApiKeyMetadata` (the POST). It runs after routing
      but **before model binding**, so unauthenticated requests are rejected without
      parsing the body. GET stays open to preserve current behavior. Empty/whitespace
      `Api:Key` is rejected at startup.
- [x] Return a per-field `ProblemDetails` on validation failure (400) via
      `Results.ValidationProblem(...)`. `MeasurementValidator.Validate` returns
      `ValidationError(Field, Message)` items, so each field carries its real reason
      (not a blanket "Required") and the endpoint never hardcodes fields or messages.
- [x] Extracted endpoints into `Endpoints/MeasurementEndpoints.cs`
      (`MapMeasurementEndpoints`); `Program.cs` is now a thin composition root.

## Verified (runtime smoke test)
- `/healthz` no key → 200; GET no key → 200 (behavior preserved).
- POST no key → 401; POST valid + key → 202.
- POST **malformed JSON without key → 401** (auth runs before body binding).
- POST invalid + key → 400 with an `errors` map giving each field a reason, e.g.
  `{"MeasurementId":["Field should not be empty"],"DeviceId":["Field should not be empty"],"Type":["Field should not be empty"]}`.
- Production with no `Api:Key` configured → fails fast at startup
  (`InvalidOperationException: API key is not configured`).
- `dotnet build` and `dotnet test` green.

## Out of scope
- Unit tests (#future) and real integration tests (#future) — separate steps 4–5.
- DesktopApp and DeviceSimulator changes.

## Key decisions
- Auth as an **endpoint filter** (not global middleware) so `/healthz` and Swagger
  stay open.
- Validation rules live only in `MeasurementValidator`, which returns
  `ValidationError(Field, Message)` items (field + real reason). The API groups them
  per field and maps to `ValidationProblem`, so new rules/messages update the 400
  response automatically — no field list or reason duplicated in the API.
  Chose this over DataAnnotations because .NET 8 minimal APIs do not auto-validate
  attributes (that is .NET 9+ `AddValidation`), and `Measurement.Value` is `object`
  so range attributes do not fit; a plain validator also handles per-type rules later.

## Open questions / risks
- Dev key (`local-dev`) lives in `appsettings.Development.json`. Future integration
  tests should inject their own key via `WebApplicationFactory` (e.g. `UseSetting`),
  not rely on a shipped default.
- Returning `ValidationProblem` changes the 400 *body* shape (now an `errors` map).
  Existing tests do not assert the body, but note this in the PR.
- Deferred from the #11 review (now tracked as step 8 in `issue-3-test-task.md`):
  constant-time key comparison / real ASP.NET auth, and gating Swagger to non-Production.

## How to verify
```bash
dotnet build
dotnet test
```
Expected:
- Build green; all tests pass.
- No hardcoded API key remains in `Program.cs` (key comes from config).
- POST with a bad body returns 400 with a `ProblemDetails` payload.
- POST without `x-api-key` still returns 401; valid POST still returns 202.

## Notes for the next session (AI or human)
Read `CLAUDE.md` "Architecture rules" first (rule 4: `Program.cs` is the composition
root; read secrets from configuration). Keep endpoints thin. Do not break the
`Domain` layering. Parent context: `issue-3-test-task.md`.
