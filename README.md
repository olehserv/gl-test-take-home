# Medical-Device Data System (.NET 8)

A simplified medical-device telemetry system used to showcase a clean, modern, well-tested
.NET 8 architecture. Devices (real or simulated) push **measurements** (e.g. HeartRate) to a
REST **ingestion API**; a **WPF desktop app** reads them back for display. The work in this
repository modernizes the original solution — extracting a reusable domain, thinning the web
layer, applying MVVM in the desktop app, adding tests, versioning the API, and hardening
security — without changing the product's purpose.

## Solution structure

| Project | Type | Role |
|---|---|---|
| `src/Domain` | Class library | Domain model + rules. **No web/UI/HTTP/IO dependencies.** `Measurement`, `MeasurementValidator`, `ValidationError`, `IMeasurementStore` + `InMemoryMeasurementStore`. |
| `src/IngestionApi` | ASP.NET Core Minimal API | Thin web layer: routing, API-key auth, versioning, Swagger, DI. Depends on `Domain`. |
| `src/DesktopApp` | WPF | "Vitals Monitor" UI. MVVM + DI + typed async `HttpClient`. Depends on `Domain`. |
| `src/DeviceSimulator` | Console | Posts fake measurements to the API on a loop (dev aid). |
| `tests/Domain.UnitTests` | xUnit | Unit tests for `MeasurementValidator` and `InMemoryMeasurementStore`. |
| `tests/IngestionApi.IntegrationTests` | xUnit + `WebApplicationFactory` | End-to-end endpoint tests. |

**Layering rule:** everything may depend on `Domain`; `Domain` depends on nothing. This keeps
the business rules reusable and framework-free.

## Architecture decisions

- **Domain isolation.** Model, validation, and the in-memory store live in `Domain` with zero
  framework references, so they are reusable and unit-testable in isolation.
- **Thin web layer.** `Program.cs` is a composition root only; endpoints live in
  `Endpoints/MeasurementEndpoints.cs`. Invalid input returns RFC-7807 `ProblemDetails`
  (per-field errors) rather than ad-hoc strings.
- **Config-driven secrets.** The API key is read from configuration (`Api:Key`); the dev
  value lives in `appsettings.Development.json` only, and the app **fails fast at startup** if
  no key is configured in other environments. The key is compared in **constant time**
  (`CryptographicOperations.FixedTimeEquals`). Auth is applied per-endpoint via
  `RequireApiKeyMetadata`, so `/healthz` and the read endpoint stay open.
- **Real API versioning.** `Asp.Versioning` declares a version set and a
  `/api/v{version:apiVersion}` route group, so the version is defined once (not a hardcoded
  string) and a future `v2` is a clean add. Existing `/api/v1/...` URLs are preserved.
  Swagger shows a document per version and is served **only outside Production**.
- **MVVM desktop.** `DesktopApp` uses CommunityToolkit.Mvvm (`MainViewModel`), a DI host in
  `App.xaml.cs`, and a typed `IMeasurementApiClient` over `IHttpClientFactory` +
  System.Text.Json. The API base URL comes from `IOptions<ApiOptions>`. No logic in
  code-behind; errors surface in a bound `Status` field instead of being swallowed.
- **Build hygiene.** Central Package Management (`Directory.Packages.props`), shared
  `Directory.Build.props`, pinned SDK (`global.json`), MinVer versioning, and CI that builds +
  tests and enforces Conventional Commit PR titles.

## What was modernized / fixed

| Area | Before | After |
|---|---|---|
| Domain | Logic lived inside the web project | Extracted to a dependency-free `Domain` library (one type per file) |
| API auth | API key hardcoded (`== "local-dev"`) inline | From config, fail-fast, **constant-time** comparison, endpoint-scoped middleware |
| API errors | Plain `BadRequest("invalid")` string | Per-field `ProblemDetails` from `MeasurementValidator` |
| API shape | All logic in `Program.cs`; hardcoded `v1` segment | Thin composition root + extracted endpoints; real `Asp.Versioning` |
| API docs | Swagger served everywhere | Per-version Swagger, gated to non-Production |
| POST response | `Location` pointed at a non-existent `/measurements/{id}` route | `Location` points at the resolvable `?type=` filter |
| DesktopApp | Blocking `WebClient`, no MVVM/DI, `catch {}`, `List<dynamic>`, hardcoded URL, undisposed timer | MVVM + DI + typed async client, errors surfaced, URL from config |
| Tests | Fake integration test, no unit tests | 16 unit + 7 integration tests (auth, validation, round-trip) |

**Known remaining item:** `DeviceSimulator` is still a minimal loop that POSTs an anonymous
object with hardcoded values; it has not been pointed at `Domain.Measurement` or given a typed
client. It exists as a dev aid, not production code.

## Build, test, run

```bash
dotnet build      # build the whole solution
dotnet test       # run all unit + integration tests
```

Run the API and simulator (two terminals); the API key dev default is `local-dev`:

```bash
dotnet run --project src/IngestionApi        # Swagger at /swagger (non-Production only)
dotnet run --project src/DeviceSimulator     # posts fake measurements to the API
```

The WPF `DesktopApp` runs on Windows only; launch it from Visual Studio or
`dotnet run --project src/DesktopApp`. Its API base URL is configured in
`src/DesktopApp/appsettings.json` (`Api:BaseUrl`).

### API at a glance

| Method | Route | Auth | Result |
|---|---|---|---|
| GET | `/healthz` | none | `200` health status |
| GET | `/api/v1/measurements?type=&since=` | none | `200` list (capped at 500, default last 5 min) |
| POST | `/api/v1/measurements` | `x-api-key` | `202` (+ `Location`), `400` invalid, `401` missing/bad key |

## Development workflow

This repository uses an AI-assisted, traceable workflow:

- **`CLAUDE.md`** — project rules for AI assistants and contributors (architecture, tech
  choices, testing, commit conventions).
- **`AI_WORKFLOW.md`** — how AI was used, with example prompts, corrections, and verification.
- **`docs/handoffs/`** — one short handoff note per open GitHub issue (goal, plan, decisions,
  risks), so any session can continue the work. See `docs/handoffs/README.md`.
- **`docs/adr/`** — architecture decision records for choices that are hard to reverse.
