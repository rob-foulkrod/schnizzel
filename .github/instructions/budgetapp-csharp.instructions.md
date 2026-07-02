---
description: "Use when modifying BudgetApp C# source or tests, especially CSV loading/parsing, budget summaries, menu flows, validation messages, and console output formatting."
name: "BudgetApp C# Conventions"
applyTo: ["src/BudgetApp/**/*.cs", "tests/BudgetApp.Tests/**/*.cs"]
---
# BudgetApp C# Conventions

- Treat these as hard requirements unless the user explicitly asks for a different approach.
- Keep responsibilities separated:
  - put menu flow and console interaction in `BudgetMenu`
  - put CSV parsing and row/header validation in `CsvParser`
  - put aggregation logic in `BudgetCalculator`
- Prefer guard clauses and explicit user-facing validation messages for bad input.
- Preserve existing console output layout and table-style formatting unless the task explicitly asks to change UX text.
- Maintain CSV compatibility with header order `Date,Description,Category,Amount`.
- Write deterministic tests (avoid culture, time, and machine-path dependence).
- When behavior changes, update or add tests in `tests/BudgetApp.Tests` in the same change.
- After code changes, run the full suite with `dotnet test BudgetApp.slnx` and fix regressions before finishing.
