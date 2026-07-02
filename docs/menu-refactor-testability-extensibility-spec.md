# BudgetMenu Refactor Spec (Testability + Extensibility)

## Goal
Refactor the menu layer so behavior stays the same for users while making it easier to unit test and safer to extend with new menu options.

## Scope
- In scope:
  - Introduce an input/output seam for menu interaction.
  - Replace hardcoded switch dispatch with command-based menu action registration.
  - Extract CSV save persistence from `BudgetMenu` into a dedicated service.
  - Extract default categories from `BudgetMenu` into a dedicated provider.
  - Add focused unit tests that no longer require global `Console` redirection.
- Out of scope:
  - Changing core summary calculations in `BudgetCalculator`.
  - Changing CSV parse behavior in `CsvParser`.
  - Adding new end-user menu features.

## Functional Requirements
1. Existing menu options (1-7) must remain available and behave consistently.
2. Exit flow must still offer save-on-exit when transactions exist.
3. Add transaction flow must preserve existing validation behavior.
4. CSV output must preserve header order `Date,Description,Category,Amount`.

## Non-Functional Requirements
1. Unit tests should run without mutating process-global console streams.
2. Menu option additions should not require editing a large `switch` block.
3. Persistence concerns should be separated from interaction flow.
4. Category defaults should be replaceable without changing menu orchestration code.

## Design
- `IMenuIO` + `ConsoleMenuIO`
  - `BudgetMenu` reads and writes through `IMenuIO`.
  - Production uses `ConsoleMenuIO`; tests use a fake implementation.
- Command-based menu options
  - Register menu options as key/label/handler entries.
  - Dispatch by lookup rather than `switch`.
- `TransactionCsvStore`
  - Handles writing transactions to CSV and escaping fields.
  - `BudgetMenu` calls this service for save-on-exit.
- `TransactionCategoryProvider`
  - Owns default category list.
  - `BudgetMenu` uses provider output for category prompts.

## Risks and Mitigations
- Risk: output text regressions break tests.
  - Mitigation: keep user-facing text stable where practical; verify with focused assertions.
- Risk: constructor changes ripple to app startup/tests.
  - Mitigation: provide explicit wiring in `Program.cs` and update tests in same change.

## Acceptance Criteria
1. `BudgetMenu` compiles with `IMenuIO`, category provider, and CSV store dependencies.
2. Menu dispatch is dictionary/command-based (no top-level menu `switch`).
3. Save-to-CSV logic is not implemented directly in `BudgetMenu`.
4. New or updated tests validate:
  - invalid date re-prompt behavior,
  - custom category add flow,
  - save prompt + persistence call behavior,
  - no save prompt when no transactions exist.
5. `dotnet test BudgetApp.slnx` passes.

## Implementation Checklist
- [x] Create spec document and checklist.
- [x] Step 1: Add `IMenuIO` seam and migrate menu/tests to use it.
- [x] Step 2: Replace menu `switch` dispatch with command-based registration and lookup.
- [x] Step 3: Extract CSV persistence and category defaults into dedicated services.
- [x] Update `Program.cs` wiring for new dependencies.
- [x] Add/update focused tests for testability and extensibility flows.
- [x] Run full test suite and fix regressions.
- [x] Mark checklist complete.
