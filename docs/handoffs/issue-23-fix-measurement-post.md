# Handoff — Issue #23: Fix the measurement POST Location header

- **Issue:** #23 (link) — part of #3 (step 8)
- **Branch:** `fix/measurement-post-location`
- **Owner:** Oleh Shevtsiv
- **Status:** ready for review

## Goal
Make the `POST /api/v1/measurements` 202 response point its `Location` header at a resource
that actually resolves. Today it returns `/api/v1/measurements/{MeasurementId}` — a route that
does not exist (the only GET is `/api/v1/measurements?type=`). Reference the measurement by
`Type` using the query form so the `Location` resolves via the existing GET.

## Current state (what to change) — `src/IngestionApi/Endpoints/MeasurementEndpoints.cs`
```csharp
return Results.Accepted($"/api/v1/measurements/{m.MeasurementId}", m); //todo: fix to use Type instead of MeasurementId
```
- The path `/measurements/{MeasurementId}` has no matching GET route → the `Location` is a 404.

## Scope
- [x] Change the `Location` to the resolvable query form and drop the `//todo`:
      `Results.Accepted($"/api/v1/measurements?type={Uri.EscapeDataString(m.Type)}", m)`.
- [x] Update the integration test `PostMeasurement_ValidWithKey_Returns202WithLocation` to
      assert the `Location` carries the type filter (`type=HeartRate`) instead of the id.

## Done so far
- [x] `MeasurementEndpoints.cs`: POST now returns `Location`
      `/api/v1/measurements?type={Uri.EscapeDataString(m.Type)}`; `//todo` removed.
- [x] Integration test asserts the `Location` contains `type=HeartRate`.
- [x] `dotnet build` + `dotnet test` green (16 unit + 7 integration).

## Next steps
1. Commit (`fix:`) and open a PR.
2. Tick step 8 / mark `#23` in the Sub-issues list in `issue-3-test-task.md`.

## Key decisions
- **Query form** `?type={Type}` (resolvable via the existing GET) over a literal
  `/measurements/{Type}` path swap, which would still 404. Confirmed with the user.
- `Uri.EscapeDataString(m.Type)` so any URL-special characters in a future type are encoded.
- Status code stays **202 Accepted** (write goes to an async-style store); only the `Location`
  target changes.

## Open questions / risks
- The `Location` is a *filter* URL (a collection query), not a single-item resource URL —
  acceptable here because the API has no get-by-id endpoint. If one is added later, revisit.
- Only the integration test asserts on `Location`; DesktopApp/DeviceSimulator ignore it.

## How to verify
```bash
dotnet build
dotnet test
```
Expected: build green; all integration tests pass, with the 202 test now asserting the
`Location` contains `type=HeartRate`.

## Notes for the next session (AI or human)
Endpoint is in `src/IngestionApi/Endpoints/MeasurementEndpoints.cs` (the versioned group from
#21). Parent context: `issue-3-test-task.md`.
