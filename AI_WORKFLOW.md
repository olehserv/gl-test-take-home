# AI Workflow

This document explains how to use AI to build this solution, and -- more
important -- how to control the design, accuracy, security, etc.

> Why this file exists: using AI well is a senior skill. Generating code is easy.
> The real skill is directing the AI, reviewing its output, catching its mistakes,
> and owning the final decisions. This file gives evidence of that.

## Tools

- **Claude Code** -- used for analysis, refactoring, generating code and tests.
- The repo has a `CLAUDE.md` so the AI follows our architecture rules automatically
  (for example: "`Domain` must not reference any web/UI framework").
- Context between sessions is carried in `docs/handoffs/` (one note per GitHub issue).

- **Chat GPT** -- used for chatting, searching for existing solutions, approaches, best practices.
- Using it as alternative to Claude Code.

- **Copilot** -- for inline code suggestions in VS Code.

## How I work with AI

1. **Plan first.** I ask the AI to analyze and propose a plan. I review and adjust it.
2. **Small steps.** One issue → one branch → one handoff note. Small commits.
3. **Add temp comments in code.** Inline instructions as a lighthouses for AI agent.
4. **Generate, then review.** The AI writes a first version. I read every line. 
5. **Verify with evidence.** After each change I run `dotnet build` and `dotnet test`
   and check the real output. I do not trust "it should work".
6. **Own the decisions.** Architecture and trade-offs are mine. The AI advises.

## Example prompts I used

These are shortened, real examples. Each one shows the *intent*, not just "write code".

**1. Analysis (before any code):**
> Context: .Net 8 project, WPF, Asp.
> Role: you are senior software .net architect.
> Task: analyze this solution, fileA (Method1, Method2), fileB, fileC (Method5). Find code smells and violations of clean
> architecture, especially where domain logic is mixed with web/UI concerns.
> Generate me summary of your investigation, prorityze and specify founding,
> suggest variants of resolution, suggest usefull improvements.

**2. Targeted refactor (with constraints):**
> Context: .Net 8 project, WPF, Asp.
> Role: you are senior backend .net dev.
> Task: check `Measurement`, `MeasurementValidator`, and the store into a new `Domain`
> class library. `Domain` must not reference ASP.NET or WPF, it should be agnostic to above layers.
> Also check comments in code I have put special for you on this task.
> Update the API to depend on it.
> Verify: Keep `dotnet build .` green and `dotnet test .` all passed.

**3. Tests (asking for edge cases):**
> Context: .Net 8 project, WPF, Asp, XUnit, Moq, FLuentAssertion, TestContainers.
> Role: you are senior backend .net dev.
> Task: write unit tests for `MeasurementValidator.IsValid`.
> Cover one valid case and every invalid case (empty Id, default
> timestamp, empty DeviceId, empty Type etc).
> Verify: `dotnet build .` is green, `dotnet test .` - all passed.

**4. Self-review (AI as a critic):**
> Context: .Net 8 project, WPF, Asp.
> Role: you are senior backend .net dev.
> Task: "Review the diff on this branch as a strict senior reviewer. Look for: swallowed
> exceptions, blocking calls on the UI thread, broken layering, and hardcoded secrets.
> List problems with file and line -- do not fix yet."

## Where AI was wrong, and how I corrected it

> This is the most important section. It proves I review AI output, not just accept it.
> Developer is a source of truth and takes full responsibility of the results.
> Every generated code should be verified carefully  by manual debugging and unit/integration tests added when needed.

## Verification

Every change is verified before commit:

```bash
dotnet restore .
dotnet build .
dotnet test .
```

I paste the real result into the pull request "How to test" section. Claims without
evidence do not count as done.

## Division of responsibility

| AI does | I do |
|---|---|
| First-draft code, boilerplate (MVVM plumbing) | Architecture and layering decisions |
| Generate test cases and edge cases | Decide what "correct" means |
| Draft docs, ADRs, commit messages | Review every line; accept or reject |
| Suggest refactors | Own correctness, security, trade-offs |
