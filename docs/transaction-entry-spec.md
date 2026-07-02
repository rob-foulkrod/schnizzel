# Transaction Entry Feature Spec (v1)

## Document Status
- Owner: BudgetApp Team
- Date: 2026-07-02
- Status: Draft ready for implementation
- Scope: Console app transaction entry and save-on-exit workflow

## Background
The app currently loads transactions from CSV and calculates summaries, but users cannot enter transactions manually inside the app. This feature adds guided data entry for individual transactions and allows saving all in-memory transactions to CSV when exiting.

## Goals
- Add an interactive menu option to enter individual transactions.
- Support continuous entry (add many in one session).
- Validate required inputs with clear reprompt behavior.
- Combine loaded CSV transactions and manually entered transactions in one in-memory dataset.
- Prompt to save all in-memory transactions to CSV on exit.

## Non-Goals (v1)
- Editing existing transactions.
- Deleting transactions.
- Transaction IDs, deduplication, or conflict resolution.
- Auto-save after every entry.
- Advanced category management UI.

## User Experience

### Main Menu Changes
Add a new menu option:
- Option 6: Add transaction
- Option 7: Exit

(Adjust existing option numbering accordingly.)

### Entry Workflow
When user selects Add transaction:
1. Start a loop for transaction entry.
2. Prompt for fields in this order:
   - Date
   - Description
   - Category
   - Amount
3. Validate each field immediately. Invalid field input reprompts only that field.
4. On success, append transaction to in-memory collection.
5. Display full confirmation summary of the added transaction.
6. Ask user whether to add another transaction:
   - Yes: continue loop
   - No: return to main menu

## Field Rules

### Date
- Required, but blank input means "use today".
- Explicit user-entered format must be MM/DD/YYYY.
- Invalid format or impossible dates must be rejected with a clear error and reprompt.

### Description
- Required non-empty text.
- Preserve user input text as entered (after trim).

### Category
- Required.
- Present predefined list:
  - Food
  - Rent
  - Utilities
  - Transport
  - Entertainment
  - Other
- Also allow custom category entry.

### Amount
- Required decimal value.
- Allow positive, negative, and zero values.
- Invalid numeric input must be rejected with clear error and reprompt.

## Data Behavior
- If a file is loaded, manual transactions are added to that same in-memory dataset.
- All existing summary views include manually entered transactions automatically.

## Exit and Save Behavior
When user selects Exit:
1. If there are no transactions in memory, exit immediately.
2. If there are transactions in memory, prompt to save to CSV.
3. If user chooses save:
   - Prompt for output file path (optionally prefill current loaded file path if available).
   - Write all current in-memory transactions to CSV using overwrite behavior.
4. If user chooses not to save, exit without writing.

## CSV Output Contract
- Header must be exactly:
  Date,Description,Category,Amount
- Output must be parseable by the current CSV parser.
- Preserve comma-safe CSV behavior for text fields (quote when needed).

## Quality Requirements
- Keep changes minimal and aligned with current architecture.
- Keep validation logic testable (prefer helper methods/services over deeply inline console logic).
- Maintain existing parser and calculator behavior.

## Acceptance Criteria
- User can add one or many transactions from the menu without restarting app.
- Date accepts blank for today and MM/DD/YYYY for explicit entry.
- Category supports predefined selection plus custom value.
- Amount accepts positive, negative, and zero values.
- Added transactions appear in All Transactions and affect all summaries.
- Exit prompts for save and writes complete in-memory dataset to CSV in overwrite mode.
- Saved CSV can be reloaded by current Load file flow with no parsing errors.
- Existing tests remain green; new tests cover data entry and save flow.

## Implementation Notes
- Primary implementation target: src/BudgetApp/BudgetMenu.cs
- Consider extracting:
  - Input parsing/validation helpers
  - CSV export helper/service
- Keep console prompts deterministic for testability.

## TODO List

### Phase 1: Menu and State
- [x] Update menu numbering and add Add transaction option.
- [x] Update input validation text for new menu range.
- [x] Ensure in-memory transactions collection is mutable for append behavior.
- [x] Add exit hook to run save prompt workflow.

### Phase 2: Entry Flow
- [x] Implement continuous Add transaction loop.
- [x] Implement date prompt and strict MM/DD/YYYY validation with blank=today behavior.
- [x] Implement required description prompt and validation.
- [x] Implement predefined category selection with custom category fallback.
- [x] Implement amount prompt with decimal validation (allow negative and zero).
- [x] Add confirmation output after successful add.
- [x] Add add-another prompt behavior.

### Phase 3: Save on Exit
- [x] Prompt user to save when exiting and data exists.
- [x] Prompt for output path.
- [x] Write all in-memory transactions to CSV (overwrite mode).
- [x] Ensure output header and field formatting match parser expectations.
- [x] Handle file write failures with clear user messaging.

### Phase 4: Tests
- [x] Add validation-focused tests for date, amount, and category behavior.
- [x] Add menu interaction tests for add loop flow.
- [x] Add tests for exit save prompt branching.
- [x] Add CSV export compatibility tests (roundtrip parse).
- [x] Run full test suite and fix regressions.

## Open Questions (Optional Future Refinement)
- Should the save prompt appear only when there are unsaved manual changes, or whenever any in-memory data exists?
- Should the app default save path to loaded file path automatically, or ask every time without default?
- Should category values be normalized (for example title case) before storing?
