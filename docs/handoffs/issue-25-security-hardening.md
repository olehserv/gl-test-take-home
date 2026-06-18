# Handoff — Issue #25: Security hardening (constant-time key + gate Swagger)

- **Issue:** #25 (link) — part of #3 (step 9)
- **Branch:** `feat/security-hardening`
- **Owner:** Oleh Shevtsiv
- **Status:** ready for review

## Goal
Address the two concrete security items raised in the #11 review:
1. Compare the API key in **constant time** to remove the timing side-channel in the
   current `provided != _expectedKey` check.
2. **Serve Swagger only outside Production**, so the API surface isn't exposed in prod.

Scope (confirmed with user): keep the existing `ApiKeyMiddleware` — do **not** replace it with
a full ASP.NET authentication scheme. That "consider real auth" item is deferred (see below).

## Current state (what to change)
- `src/IngestionApi/Auth/ApiKeyMiddleware.cs`: `provided != _expectedKey` — ordinary string
  comparison, which can short-circuit and leak timing.
- `src/IngestionApi/Program.cs`: `app.UseSwagger()` / `app.UseSwaggerUI(...)` run
  unconditionally (Swagger is served in every environment, including Production).

## Scope
- [x] `ApiKeyMiddleware`: compare with `CryptographicOperations.FixedTimeEquals` over the
      UTF-8 bytes of the provided vs expected key (precompute the expected bytes in the ctor).
      Behaviour is unchanged (valid key → pass, missing/wrong → 401); only timing changes.
- [x] `Program.cs`: wrap `UseSwagger`/`UseSwaggerUI` in `if (!app.Environment.IsProduction())`
      so Swagger is served in Development/Staging but not Production.

## Done so far
- [x] `ApiKeyMiddleware` now precomputes the expected key bytes and validates via
      `CryptographicOperations.FixedTimeEquals` (`IsValidApiKey` helper); no plaintext `!=`.
- [x] `Program.cs` gates `UseSwagger`/`UseSwaggerUI` behind `!app.Environment.IsProduction()`.
- [x] `dotnet build` + `dotnet test` green (16 unit + 7 integration; 401/202 tests exercise
      the new comparison).

## Verification
- Build green; all 23 tests pass — the `POST no key → 401` and `POST valid + key → 202`
  integration tests confirm the constant-time comparison preserves behaviour.
- Ran the API and confirmed Swagger gating: **Production** → `/swagger/v1/swagger.json` = 404;
  **Development** → 200; `/healthz` = 200 in both (app otherwise unchanged).

## Next steps
1. Commit (`fix:`) and open a PR.
2. Tick step 9 / mark `#25` in the Sub-issues list in `issue-3-test-task.md`.

## Key decisions
- **Keep `ApiKeyMiddleware`**; just harden the comparison. Full ASP.NET
  `AddAuthentication`/`RequireAuthorization` was considered and **deferred** — it changes how
  endpoints are marked and how 401 is produced (more churn for the same external behaviour);
  can be a separate issue if real auth (multiple schemes, scopes) is ever needed.
- `FixedTimeEquals` returns false for differing lengths; the key length is not sensitive, so
  the residual length-based timing is acceptable.
- Gate on `!IsProduction()` (not `IsDevelopment()`) so Staging/other non-prod envs keep Swagger.

## Open questions / risks
- `WebApplicationFactory` runs the integration tests outside Production, so Swagger stays on
  there — the tests don't assert Swagger, so gating won't affect them.
- The middleware still fail-fasts at startup if `Api:Key` is unset (unchanged).

## How to verify
```bash
dotnet build
dotnet test
```
Plus runtime: run the API in Production (`/swagger/v1/swagger.json` → 404) and Development
(→ 200); confirm `POST` with/without the key still returns 202/401.

## Notes for the next session (AI or human)
Auth is in `src/IngestionApi/Auth/ApiKeyMiddleware.cs` (keyed off `RequireApiKeyMetadata`);
Swagger wiring is in `Program.cs` (the versioned setup from #21). Parent: `issue-3-test-task.md`.
