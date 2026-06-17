# CLAUDE.md — Project rules for AI assistants

This file tells AI assistants (and new contributors) how to work in this repository.
Follow it unless the human gives a different instruction in the conversation.

## What this project is

A simplified medical-device data system on **.NET 8**. We are modernizing the
architecture and maintainability. The goal is clean, idiomatic, well-tested code —
not new features.

| Project | Type | Role |
|---|---|---|
| `src/Domain` | Class library | Domain model + rules. **No** web/UI/HTTP dependencies. (Being introduced.) |
| `src/IngestionApi` | ASP.NET Core Minimal API | HTTP layer: routing, auth, DI wiring. Depends on `Domain`. |
| `src/DesktopApp` | WPF | UI. Uses MVVM + DI + async `HttpClient`. |
| `src/DeviceSimulator` | Console app | Sends fake measurements to the API. |
| `tests/Domain.UnitTests` | xUnit | Unit tests for domain rules. (Being introduced.) |
| `tests/IngestionApi.IntegrationTests` | xUnit + `WebApplicationFactory` | Endpoint tests. |

The active plan for each task lives in its handoff note under `docs/handoffs/`.

## Architecture rules (do not break these)

1. **Layering:** `Domain` must not reference ASP.NET, WPF, HTTP, or any I/O framework.
   Everything depends on `Domain`; `Domain` depends on nothing.
2. **DesktopApp:** no business logic in code-behind (`*.xaml.cs`). Use MVVM
   (ViewModel + bindings + commands). Resolve views/view-models through DI.
3. **HTTP:** always `async`/`await`. Use `IHttpClientFactory` and a typed client.
   Never use `WebClient` or blocking calls on the UI thread.
4. **IngestionApi:** `Program.cs` is the composition root only. Keep endpoints thin.
   Read secrets/keys (for example the API key) from configuration, never hardcode.
5. **Errors:** never swallow exceptions (`catch { }`). Surface them: log them, or
   show a clear status to the user. This is medical software — silence is dangerous.

## Build and test

```bash
dotnet build                 # build the whole solution
dotnet test                  # run all tests
```

## Commits and pull requests

- **Conventional Commits are enforced** (CI checks the PR title). Use one of:
  `feat:`, `fix:`, `feat!:`, `chore:`, `docs:`, `test:`.
- Keep commits small and focused — ideally one logical step per commit.
- For AI-paired commits, add the trailer:
  `Co-Authored-By: Claude <noreply@anthropic.com>`

## Handoff workflow (per GitHub issue)

We keep one handoff note per **open** GitHub issue in `docs/handoffs/`.
- File name: `issue-<number>-<short-slug>.md` (for example `issue-12-mvvm-desktop.md`).
- Start from `docs/handoffs/TEMPLATE.md`.
- Update the note as work progresses, so any session (human or AI) can continue.
- Full rules: `docs/handoffs/README.md`.

## How AI should work here

- Read this file and the relevant handoff note before starting.
- Make a short plan, then work in small verified steps.
- After any change, run `dotnet build` and `dotnet test` and report the result.
- If the AI output is wrong, the human fixes it and records the lesson in
  `AI_WORKFLOW.md`. The human owns the final decisions.
