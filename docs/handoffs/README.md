# Handoff notes

This folder holds **one short handoff note per open GitHub issue**.

A handoff note captures the *current context* of a task so anyone — a teammate or
a new AI session — can continue the work without losing information. AI assistants
forget everything between sessions, so this note is how we carry context forward.

## Rules

1. **One note per open issue.** File name: `issue-<number>-<short-slug>.md`.
   - Example: `issue-12-mvvm-desktop.md` for issue #12.
2. **Start from the template:** copy `TEMPLATE.md`.
3. **Keep it short.** One page. It is a working note, not full documentation.
4. **Keep it current.** Update "Done" and "Next" as the work moves.
5. **Delete on close.** When the issue is closed (usually by a merged PR),
   **remove its handoff note in the same PR**. The folder should only ever contain
   notes for *active* issues.

## Why this design

- **Traceable:** every note maps to exactly one ticket.
- **Clean:** closed work leaves no clutter. Open the folder → see only live tasks.
- **Reviewer-friendly:** the handoff folder and the issue list always match.

## Optional automation

You can automate the cleanup with a GitHub Action that runs when an issue closes
and opens a small PR to delete `docs/handoffs/issue-<number>-*.md`. For this
take-home, manual deletion in the closing PR is enough — but mention the automation
idea to show you thought about it.

## Files

- `TEMPLATE.md` — copy this to start a new note. (It is not tied to an issue, so it stays.)
- `issue-*.md` — active handoff notes.
