using BudgetApp.Models;
using BudgetApp.Services;
using System.Globalization;

namespace BudgetApp;

public class BudgetMenu
{
    private sealed record MenuCommand(string Key, string Label, Func<bool> Execute);

    private readonly CsvParser _parser;
    private readonly BudgetCalculator _calculator;
    private readonly IMenuIO _menuIo;
    private readonly TransactionCategoryProvider _categoryProvider;
    private readonly TransactionCsvStore _transactionCsvStore;
    private readonly TimeProvider _timeProvider;
    private readonly IReadOnlyList<MenuCommand> _menuCommands;
    private readonly IReadOnlyDictionary<string, MenuCommand> _menuCommandsByKey;
    private readonly List<Transaction> _transactions = new();
    private string _loadedFile = string.Empty;

    public BudgetMenu(
        CsvParser parser,
        BudgetCalculator calculator,
        IMenuIO menuIo,
        TransactionCategoryProvider categoryProvider,
        TransactionCsvStore transactionCsvStore,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(calculator);
        ArgumentNullException.ThrowIfNull(menuIo);
        ArgumentNullException.ThrowIfNull(categoryProvider);
        ArgumentNullException.ThrowIfNull(transactionCsvStore);

        _parser = parser;
        _calculator = calculator;
        _menuIo = menuIo;
        _categoryProvider = categoryProvider;
        _transactionCsvStore = transactionCsvStore;
        _timeProvider = timeProvider ?? TimeProvider.System;

        _menuCommands =
        [
            new MenuCommand("1", "Load spending CSV file", () =>
            {
                LoadFile();
                return true;
            }),
            new MenuCommand("2", "View budget summary", () =>
            {
                ShowBudgetSummary();
                return true;
            }),
            new MenuCommand("3", "View spending by category", () =>
            {
                ShowByCategory();
                return true;
            }),
            new MenuCommand("4", "View spending by month", () =>
            {
                ShowByMonth();
                return true;
            }),
            new MenuCommand("5", "View all transactions", () =>
            {
                ShowAllTransactions();
                return true;
            }),
            new MenuCommand("6", "Add transaction", () =>
            {
                AddTransactions();
                return true;
            }),
            new MenuCommand("7", "Exit", () =>
            {
                PromptSaveOnExit();
                _menuIo.WriteLine();
                _menuIo.WriteLine("Goodbye! 👋");
                return false;
            })
        ];

        _menuCommandsByKey = _menuCommands.ToDictionary(command => command.Key, StringComparer.Ordinal);
    }

    public void Run()
    {
        _menuIo.WriteLine("╔══════════════════════════════════════╗");
        _menuIo.WriteLine("║      💰 Budget Tracker App 💰        ║");
        _menuIo.WriteLine("╚══════════════════════════════════════╝");
        _menuIo.WriteLine();

        bool running = true;
        while (running)
        {
            PrintMenu();
            var choice = _menuIo.ReadLine()?.Trim();

            if (choice is not null && _menuCommandsByKey.TryGetValue(choice, out var command))
            {
                running = command.Execute();
            }
            else
            {
                _menuIo.WriteLine("Invalid option. Please choose 1-7.");
            }

            if (running) _menuIo.WriteLine();
        }
    }

    private void PrintMenu()
    {
        var status = string.IsNullOrEmpty(_loadedFile)
            ? "No file loaded"
            : $"Loaded: {Path.GetFileName(_loadedFile)} ({_transactions.Count} transactions)";

        _menuIo.WriteLine($"[{status}]");
        _menuIo.WriteLine("─────────────────────────────────────");
        foreach (var command in _menuCommands)
        {
            _menuIo.WriteLine($"{command.Key}. {command.Label}");
        }
        _menuIo.WriteLine("─────────────────────────────────────");
        _menuIo.Write("Select an option: ");
    }

    private void LoadFile()
    {
        _menuIo.Write("Enter CSV file path: ");
        var path = _menuIo.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(path))
        {
            _menuIo.WriteLine("No path entered.");
            return;
        }

        try
        {
            var parsed = _parser.Parse(path);
            _transactions.Clear();
            _transactions.AddRange(parsed);
            _loadedFile = path;
            _menuIo.WriteLine($"✅ Loaded {_transactions.Count} transaction(s) from '{path}'.");
        }
        catch (FileNotFoundException ex)
        {
            _menuIo.WriteLine($"❌ Error loading file: {ex.Message}");
        }
        catch (InvalidDataException ex)
        {
            _menuIo.WriteLine($"❌ Error loading file: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            _menuIo.WriteLine($"❌ Error loading file: {ex.Message}");
        }
        catch (IOException ex)
        {
            _menuIo.WriteLine($"❌ Error loading file: {ex.Message}");
        }
    }

    private void ShowBudgetSummary()
    {
        if (!EnsureDataLoaded()) return;

        var total = _calculator.GetTotalSpending(_transactions);
        var avgMonthly = _calculator.GetAverageMonthlySpending(_transactions);
        var topCategory = _calculator.GetTopSpendingCategory(_transactions);
        var months = _calculator.GetMonthlySummaries(_transactions);
        var categories = _calculator.GetCategorySummaries(_transactions);

        _menuIo.WriteLine();
        _menuIo.WriteLine("══════════════════════════════════════");
        _menuIo.WriteLine("           BUDGET SUMMARY             ");
        _menuIo.WriteLine("══════════════════════════════════════");
        _menuIo.WriteLine($"  Total Spending:       {total,12:C}");
        _menuIo.WriteLine($"  Avg Monthly Spending: {avgMonthly,12:C}");
        _menuIo.WriteLine($"  Top Category:         {topCategory}");
        _menuIo.WriteLine($"  Months Tracked:       {months.Count,12}");
        _menuIo.WriteLine($"  Categories:           {categories.Count,12}");
        _menuIo.WriteLine($"  Total Transactions:   {_transactions.Count,12}");
        _menuIo.WriteLine("══════════════════════════════════════");
    }

    private void ShowByCategory()
    {
        if (!EnsureDataLoaded()) return;

        var summaries = _calculator.GetCategorySummaries(_transactions);
        var total = _calculator.GetTotalSpending(_transactions);

        _menuIo.WriteLine();
        _menuIo.WriteLine("══════════════════════════════════════════════════════════");
        _menuIo.WriteLine(" SPENDING BY CATEGORY");
        _menuIo.WriteLine("══════════════════════════════════════════════════════════");
        _menuIo.WriteLine($"  {"Category",-25} {"Total",10}  {"% of Total",10}  {"Txns",5}");
        _menuIo.WriteLine("  " + new string('─', 55));

        foreach (var s in summaries)
        {
            var pct = total > 0 ? s.TotalSpent / total * 100 : 0;
            _menuIo.WriteLine($"  {s.Category,-25} {s.TotalSpent,10:C}  {pct,9:F1}%  {s.TransactionCount,5}");
        }

        _menuIo.WriteLine("  " + new string('─', 55));
        _menuIo.WriteLine($"  {"TOTAL",-25} {total,10:C}  {"100.0%",10}  {_transactions.Count,5}");
        _menuIo.WriteLine("══════════════════════════════════════════════════════════");
    }

    private void ShowByMonth()
    {
        if (!EnsureDataLoaded()) return;

        var summaries = _calculator.GetMonthlySummaries(_transactions);

        _menuIo.WriteLine();
        _menuIo.WriteLine("════════════════════════════════════════════════");
        _menuIo.WriteLine(" SPENDING BY MONTH");
        _menuIo.WriteLine("════════════════════════════════════════════════");
        _menuIo.WriteLine($"  {"Month",-20} {"Total",12}  {"Txns",5}");
        _menuIo.WriteLine("  " + new string('─', 42));

        foreach (var s in summaries)
        {
            _menuIo.WriteLine($"  {s.MonthLabel,-20} {s.TotalSpent,12:C}  {s.TransactionCount,5}");
        }

        _menuIo.WriteLine("════════════════════════════════════════════════");
    }

    private void ShowAllTransactions()
    {
        if (!EnsureDataLoaded()) return;

        _menuIo.WriteLine();
        _menuIo.WriteLine("══════════════════════════════════════════════════════════════════════");
        _menuIo.WriteLine(" ALL TRANSACTIONS");
        _menuIo.WriteLine("══════════════════════════════════════════════════════════════════════");
        _menuIo.WriteLine($"  {"Date",-12} {"Description",-30} {"Category",-20} {"Amount",10}");
        _menuIo.WriteLine("  " + new string('─', 76));

        foreach (var t in _transactions.OrderBy(t => t.Date))
        {
            var desc = t.Description.Length > 28 ? t.Description[..28] + ".." : t.Description;
            _menuIo.WriteLine($"  {t.Date.ToShortDateString(),-12} {desc,-30} {t.Category,-20} {t.Amount,10:C}");
        }

        _menuIo.WriteLine("══════════════════════════════════════════════════════════════════════");
    }

    private bool EnsureDataLoaded()
    {
        if (_transactions.Count == 0)
        {
            _menuIo.WriteLine("⚠️  No data loaded. Please load a CSV file first (option 1).");
            return false;
        }
        return true;
    }

    private void AddTransactions()
    {
        _menuIo.WriteLine();
        _menuIo.WriteLine("Add Transaction");
        _menuIo.WriteLine("───────────────");

        bool keepAdding = true;
        while (keepAdding)
        {
            var date = PromptForDate();
            var description = PromptForRequiredText("Description");
            var category = PromptForCategory();
            var amount = PromptForAmount();

            var transaction = new Transaction
            {
                Date = date,
                Description = description,
                Category = category,
                Amount = amount
            };

            _transactions.Add(transaction);
            PrintAddedTransaction(transaction);

            keepAdding = PromptYesNo("Add another transaction? (y/n): ");
        }
    }

    private DateTime PromptForDate()
    {
        while (true)
        {
            _menuIo.Write("Date (MM/DD/YYYY, press Enter for today): ");
            var input = _menuIo.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                return _timeProvider.GetLocalNow().DateTime.Date;

            if (DateTime.TryParseExact(input, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                return parsedDate;

            _menuIo.WriteLine("Invalid date. Use MM/DD/YYYY.");
        }
    }

    private string PromptForRequiredText(string label)
    {
        while (true)
        {
            _menuIo.Write($"{label}: ");
            var input = _menuIo.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(input))
                return input;

            _menuIo.WriteLine($"{label} is required.");
        }
    }

    private string PromptForCategory()
    {
        var categories = _categoryProvider.GetCategories();

        while (true)
        {
            _menuIo.WriteLine("Category options:");
            for (int i = 0; i < categories.Count; i++)
                _menuIo.WriteLine($"{i + 1}. {categories[i]}");
            _menuIo.WriteLine($"{categories.Count + 1}. Custom");
            _menuIo.Write("Choose category number: ");

            var input = _menuIo.ReadLine()?.Trim();
            if (!int.TryParse(input, out var choice))
            {
                _menuIo.WriteLine($"Invalid category selection. Choose 1-{categories.Count + 1}.");
                continue;
            }

            if (choice >= 1 && choice <= categories.Count)
                return categories[choice - 1];

            if (choice == categories.Count + 1)
                return PromptForRequiredText("Custom category");

            _menuIo.WriteLine($"Invalid category selection. Choose 1-{categories.Count + 1}.");
        }
    }

    private decimal PromptForAmount()
    {
        while (true)
        {
            _menuIo.Write("Amount: ");
            var input = _menuIo.ReadLine()?.Trim();

            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out var amount))
                return amount;

            _menuIo.WriteLine("Invalid amount. Enter a numeric value.");
        }
    }

    private void PrintAddedTransaction(Transaction transaction)
    {
        _menuIo.WriteLine();
        _menuIo.WriteLine("Transaction added:");
        _menuIo.WriteLine($"  Date: {transaction.Date:MM/dd/yyyy}");
        _menuIo.WriteLine($"  Description: {transaction.Description}");
        _menuIo.WriteLine($"  Category: {transaction.Category}");
        _menuIo.WriteLine($"  Amount: {transaction.Amount:F2}");
        _menuIo.WriteLine();
    }

    private bool PromptYesNo(string prompt)
    {
        while (true)
        {
            _menuIo.Write(prompt);
            var input = _menuIo.ReadLine()?.Trim();

            if (string.Equals(input, "y", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "yes", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(input, "n", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(input, "no", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            _menuIo.WriteLine("Please answer y or n.");
        }
    }

    private void PromptSaveOnExit()
    {
        if (_transactions.Count == 0)
            return;

        if (!PromptYesNo("Save transactions to CSV before exit? (y/n): "))
            return;

        while (true)
        {
            var prompt = string.IsNullOrWhiteSpace(_loadedFile)
                ? "Enter output CSV file path: "
                : $"Enter output CSV file path [{_loadedFile}]: ";

            _menuIo.Write(prompt);
            var inputPath = _menuIo.ReadLine()?.Trim();
            var outputPath = string.IsNullOrWhiteSpace(inputPath) ? _loadedFile : inputPath;

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                _menuIo.WriteLine("Output path is required.");
                continue;
            }

            try
            {
                _transactionCsvStore.Save(outputPath, _transactions);
                _menuIo.WriteLine($"✅ Saved {_transactions.Count} transaction(s) to '{outputPath}'.");
                return;
            }
            catch (UnauthorizedAccessException ex)
            {
                _menuIo.WriteLine($"❌ Failed to save file: {ex.Message}");
                if (!PromptYesNo("Try a different path? (y/n): "))
                    return;
            }
            catch (IOException ex)
            {
                _menuIo.WriteLine($"❌ Failed to save file: {ex.Message}");
                if (!PromptYesNo("Try a different path? (y/n): "))
                    return;
            }
        }
    }
}
