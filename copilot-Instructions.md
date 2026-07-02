# Copilot Instructions

## Application Overview
This is a schnizzel application. Before making suggestions or changes, understand:

- **Purpose**: BudgetApp is a console-based spending tracker for loading transactions from CSV, adding manual transactions, viewing spending summaries, and optionally saving the current in-memory transaction set back to CSV on exit.
- **Architecture**: Simple layered console architecture.
  - `Program.cs` wires dependencies (`CsvParser`, `BudgetCalculator`) and starts `BudgetMenu`.
  - `BudgetMenu` is the interaction layer: menu loop, input validation, output formatting, save-on-exit flow, and orchestration of services.
  - `Services/CsvParser` handles CSV parsing and header/row validation.
  - `Services/BudgetCalculator` handles aggregation logic (totals, category summaries, monthly summaries, top category, averages).
  - `Models` contains plain data objects (`Transaction`, `CategorySummary`, `MonthlySummary`).
- **Tech Stack**:
  - C#
  - .NET 10 (`net10.0`)
  - xUnit for tests
- **Key Directories**: 
  - `/src` - Main source code
  - `/tests` - Test files
  - `/docs` - Documentation
  - `/samples` - Example CSV data used for local validation

## Code Style & Standards
- Follow existing code patterns and conventions
- Maintain consistency with current codebase
- Keep classes focused: menu flow in `BudgetMenu`, parsing in `CsvParser`, calculations in `BudgetCalculator`
- Prefer clear guard clauses and explicit validation messages for user-facing input errors
- Preserve current formatting style for console output blocks and table-like displays
- Add comments for complex logic only when needed
- Include tests for new features
- Prefer deterministic tests that avoid environment dependence (culture, file paths, dates)

## Before Suggesting Changes
1. Check existing code patterns first
2. Review related files in `src/BudgetApp` and matching tests in `tests/BudgetApp.Tests`
3. Consider performance and maintainability
4. Ensure changes align with project goals (simple, reliable console budgeting workflows)
5. If behavior changes, update tests first or alongside code changes
6. Keep CSV compatibility with expected header order: `Date,Description,Category,Amount`
7. Always run the full test suite after changes to ensure no regressions. Continue to fix until all resolved.

## Common Tasks
- Build and run:
  - `dotnet restore BudgetApp.slnx`
  - `dotnet build BudgetApp.slnx`
  - `dotnet run --project src/BudgetApp/BudgetApp.csproj`
- Run tests:
  - `dotnet test BudgetApp.slnx`
  - `dotnet test tests/BudgetApp.Tests/BudgetApp.Tests.csproj`
- Typical workflow for feature work:
  - Update or add tests in `tests/BudgetApp.Tests`
  - Implement changes in `src/BudgetApp`
  - Run focused tests, then full suite
  - Verify manual console flow for menu options affected
- Common troubleshooting:
  - File not found when loading CSV: verify working directory and CSV path
  - CSV header errors: ensure exact logical header order (`Date,Description,Category,Amount`)
  - Parse errors for dates/amounts: confirm valid date strings and decimal values
  - Console output assertion failures in tests: check culture-sensitive formatting and date output

---
These instructions are project-specific and should be updated as architecture or workflows evolve.