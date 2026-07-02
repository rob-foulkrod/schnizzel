# BudgetApp

A simple .NET console application for tracking spending from CSV files and manual transaction entry.

BudgetApp helps you:
- Load transactions from a CSV file.
- Add new transactions interactively.
- View total, category, and monthly spending summaries.
- Save your in-memory transaction set back to CSV on exit.

## Features

- Interactive text menu for common budgeting workflows.
- CSV import with header validation.
- Spending summaries:
	- Total spending
	- Average monthly spending
	- Top spending category
	- Category and monthly breakdowns
- Manual transaction entry with validation:
	- Date (`MM/DD/YYYY`) or blank for today
	- Required description
	- Predefined or custom category
	- Decimal amount (positive, negative, or zero)
- Save-on-exit flow to persist all in-memory transactions.

## Tech Stack

- .NET 10 (`net10.0`)
- C# console app
- xUnit test suite

## Prerequisites

- .NET 10 SDK installed

Verify installation:

```bash
dotnet --version
```

## Quick Start

From the repository root:

```bash
dotnet restore BudgetApp.slnx
dotnet build BudgetApp.slnx
dotnet run --project src/BudgetApp/BudgetApp.csproj
```

## Running Tests

Run all tests:

```bash
dotnet test BudgetApp.slnx
```

Run only app tests:

```bash
dotnet test tests/BudgetApp.Tests/BudgetApp.Tests.csproj
```

## Usage

When the app starts, you can:

1. Load a spending CSV file
2. View budget summary
3. View spending by category
4. View spending by month
5. View all transactions
6. Add transaction
7. Exit (with optional save)

Sample data is available at `samples/spending.csv`.

## CSV Format

Expected header (exact order):

```csv
Date,Description,Category,Amount
```

Notes:
- `Date` must be parseable as a date.
- `Amount` must be parseable as a decimal.
- Text fields support quoted CSV values.

Example:

```csv
Date,Description,Category,Amount
07/01/2026,Coffee,Dining Out,4.50
07/02/2026,Rent,Rent,1200.00
07/02/2026,"Groceries, weekly",Food,82.17
```

## Project Structure

```text
src/BudgetApp/
	Program.cs
	BudgetMenu.cs
	Models/
	Services/

tests/BudgetApp.Tests/
	BudgetCalculatorTests.cs
	BudgetMenuTests.cs
	CsvParserTests.cs

docs/
	transaction-entry-spec.md

samples/
	spending.csv
```

## Key Files

- Entry point: `src/BudgetApp/Program.cs`
- Interactive workflow: `src/BudgetApp/BudgetMenu.cs`
- CSV parsing: `src/BudgetApp/Services/CsvParser.cs`
- Aggregations: `src/BudgetApp/Services/BudgetCalculator.cs`
- Feature spec: `docs/transaction-entry-spec.md`

## Troubleshooting

- File not found errors:
	- Confirm the CSV path is correct.
	- Use an absolute path if running from a different working directory.
- Invalid header errors:
	- Ensure the first row is exactly `Date,Description,Category,Amount`.
- Date parse errors:
	- Use a valid date format in CSV values.
	- For manual entry, use `MM/DD/YYYY` or press Enter for today's date.

## Roadmap

Potential future enhancements:
- Edit/delete existing transactions
- Unsaved-changes tracking
- Category normalization and management
- Export formats beyond CSV

## Contributing

Contributions are welcome. Open an issue or submit a pull request with:
- A clear problem statement
- Repro steps (if bugfix)
- Tests for behavior changes

## License

This project is licensed under the MIT License. See `LICENSE` for details.