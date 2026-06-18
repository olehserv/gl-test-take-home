# Handoff — Issue #19: Modernize DesktopApp (MVVM + DI + async HTTP)

- **Issue:** #19 (link) — part of #3 (step 6)
- **Branch:** `feat/desktop-mvvm`
- **Owner:** Oleh Shevtsiv
- **Status:** ready for review

## Goal
Replace the DesktopApp's blocking/code-behind data access with the target architecture:
a typed `IMeasurementApiClient` (async, `IHttpClientFactory` + System.Text.Json), a
`MainViewModel` (CommunityToolkit.Mvvm) bound to the view, and a code-behind that only
wires up `InitializeComponent`/`DataContext`. Errors are surfaced in a `Status` field,
never swallowed.

## Current state (what to change) — `src/DesktopApp/MainWindow.xaml.cs`
- Blocking `WebClient.DownloadString` on a `System.Timers.Timer` tick; `catch { /* swallow */ }`.
- `Button_Click` shows `MessageBox` ("imperative UI logic"); deserializes to `List<dynamic>`
  with Newtonsoft.
- All logic lives in code-behind; the view has no bindings (`x:Name` + event handlers).
- `App.xaml.cs` already has a DI composition root (`ServiceCollection` + `ApiOptions` bound
  from `appsettings.json`); `MainWindow` is resolved from DI. (From the earlier IOptions work.)

## Scope
- [x] Add packages (central versions in `Directory.Packages.props`): `CommunityToolkit.Mvvm`,
      `Microsoft.Extensions.Http`. Add a `ProjectReference` to `Domain` (reuse `Measurement`).
      Remove the now-unused `Newtonsoft.Json` reference from `DesktopApp.csproj`.
- [x] `IMeasurementApiClient` + `MeasurementApiClient`: typed client, `GetMeasurementsAsync`
      via `GetFromJsonAsync<List<Measurement>>`. Registered with
      `AddHttpClient<IMeasurementApiClient, MeasurementApiClient>` and `BaseAddress` from `ApiOptions`.
- [x] `MainViewModel : ObservableObject`: `ObservableCollection<Measurement> Measurements`,
      `[ObservableProperty] Status`, `[RelayCommand] RefreshAsync` (try/catch → sets `Status`).
- [x] `App.xaml.cs`: register the typed client + `MainViewModel`.
- [x] `MainWindow.xaml`: bind `Button.Command` → `RefreshCommand`, `TextBlock.Text` → `Status`,
      `DataGrid.ItemsSource` → `Measurements`. Drop `x:Name`/event handlers.
- [x] `MainWindow.xaml.cs`: constructor takes `MainViewModel`, sets `DataContext`. No other logic.

## Done so far
- [x] New files `MeasurementApiClient.cs` (interface + impl) and `MainViewModel.cs`; slimmed
      `MainWindow.xaml.cs` to `InitializeComponent` + `DataContext`; XAML now data-bound.
- [x] `App.xaml.cs` registers the typed client (base address from `ApiOptions`) + view-model.
- [x] Removed `WebClient`, the polling timer, `Button_Click`/`MessageBox`, and Newtonsoft.
- [x] `dotnet build` green (0 warnings — the old `WebClient` SYSLIB0014 warning is gone).
- [x] Existing tests still pass (16 unit + 7 integration).

## Verification scope (what is and isn't proven)
- **Startup/DI wiring: verified** by launching the app — the window shows and the full chain
  (`MainWindow` → `MainViewModel` → `IMeasurementApiClient` → typed `HttpClient`) resolves;
  a broken registration would crash at launch.
- **HTTP + JSON deserialization (`GetFromJsonAsync<List<Measurement>>`, `object Value` →
  `JsonElement`): covered** by the IngestionApi integration tests, which exercise the same call.
- **The view-model `Refresh` body is not under automated test** (no UI tests requested). It was
  exercised only via the manual launch; behaviour with the API down is "Status shows an error",
  by construction (try/catch).

## Next steps
1. Commit (`feat:`) and open a PR.
2. Tick step 6 / add `#19` to the Sub-issues list in `issue-3-test-task.md`.

## Key decisions
- Reuse `Domain.Measurement` (DesktopApp → Domain reference) rather than a duplicate DTO —
  matches the layering rule (everything may depend on `Domain`). `Value` is `object`, so it
  deserializes to a `JsonElement`; fine for read-only `DataGrid` display.
- Manual refresh (button-bound `RefreshCommand`), not the old 1s polling timer. The timer was
  part of the debt being removed; auto/periodic refresh is out of scope (could be a follow-up).
- Errors surface in `Status` (CLAUDE.md rule 5) instead of `MessageBox` or a swallowed `catch`.

## Open questions / risks
- WPF runs on Windows only — verify by launching the app here.
- CommunityToolkit.Mvvm uses source generators (`[ObservableProperty]`/`[RelayCommand]`), so
  the view-model must be `partial`.

## How to verify
```bash
dotnet build
```
Then launch `src/DesktopApp/bin/Debug/net8.0-windows/DesktopApp.exe`: window shows, clicking
Refresh either fills the grid (if the API is running) or sets `Status` to an error message
(API down) — not a crash, not a silent failure.

## Notes for the next session (AI or human)
`App.xaml` has no `StartupUri` (window is shown from `App.OnStartup` via DI). The API base URL
comes from `appsettings.json` (`Api:BaseUrl`) through `ApiOptions`. Parent context:
`issue-3-test-task.md`. No business logic in `*.xaml.cs` (CLAUDE.md rule 2).
