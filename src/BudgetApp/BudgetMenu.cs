using BudgetApp.Models;
using BudgetApp.Services;
using System.Globalization;
using System.Text;

namespace BudgetApp;

public class BudgetMenu
{
    private static readonly string[] DefaultCategories =
    {
        "Food",
        "Rent",
        "Utilities",
        "Transport",
        "Entertainment",
        "Other",
        "Groceries",
        "Transportation",
        "Health",
        "Dining Out",
        "Shopping"
    };

    private readonly CsvParser _parser;
    private readonly BudgetCalculator _calculator;
    private readonly List<Transaction> _transactions = new();
    private string _loadedFile = string.Empty;

    public BudgetMenu(CsvParser parser, BudgetCalculator calculator)
    {
        _parser = parser;
        _calculator = calculator;
    }

    public void Run()
    {
        Console.WriteLine("╔══════════════════════════════════════╗");
        Console.WriteLine("║      💰 Budget Tracker App 💰        ║");
        Console.WriteLine("╚══════════════════════════════════════╝");
        Console.WriteLine();

        bool running = true;
        while (running)
        {
            PrintMenu();
            var choice = Console.ReadLine()?.Trim();

            switch (choice)
            {
                case "1":
                    LoadFile();
                    break;
                case "2":
                    ShowBudgetSummary();
                    break;
                case "3":
                    ShowByCategory();
                    break;
                case "4":
                    ShowByMonth();
                    break;
                case "5":
                    ShowAllTransactions();
                    break;
                case "6":
                    AddTransactions();
                    break;
                case "7":
                    PromptSaveOnExit();
                    running = false;
                    Console.WriteLine();
                    Console.WriteLine("Goodbye! 👋");
                    break;
                default:
                    Console.WriteLine("Invalid option. Please choose 1-7.");
                    break;
            }

            if (running) Console.WriteLine();
        }
    }

    private void PrintMenu()
    {
        var status = string.IsNullOrEmpty(_loadedFile)
            ? "No file loaded"
            : $"Loaded: {Path.GetFileName(_loadedFile)} ({_transactions.Count} transactions)";

        Console.WriteLine($"[{status}]");
        Console.WriteLine("─────────────────────────────────────");
        Console.WriteLine("1. Load spending CSV file");
        Console.WriteLine("2. View budget summary");
        Console.WriteLine("3. View spending by category");
        Console.WriteLine("4. View spending by month");
        Console.WriteLine("5. View all transactions");
        Console.WriteLine("6. Add transaction");
        Console.WriteLine("7. Exit");
        Console.WriteLine("─────────────────────────────────────");
        Console.Write("Select an option: ");
    }

    private void LoadFile()
    {
        Console.Write("Enter CSV file path: ");
        var path = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(path))
        {
            Console.WriteLine("No path entered.");
            return;
        }

        try
        {
            var parsed = _parser.Parse(path);
            _transactions.Clear();
            _transactions.AddRange(parsed);
            _loadedFile = path;
            Console.WriteLine($"✅ Loaded {_transactions.Count} transaction(s) from '{path}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error loading file: {ex.Message}");
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

        Console.WriteLine();
        Console.WriteLine("══════════════════════════════════════");
        Console.WriteLine("           BUDGET SUMMARY             ");
        Console.WriteLine("══════════════════════════════════════");
        Console.WriteLine($"  Total Spending:       {total,12:C}");
        Console.WriteLine($"  Avg Monthly Spending: {avgMonthly,12:C}");
        Console.WriteLine($"  Top Category:         {topCategory}");
        Console.WriteLine($"  Months Tracked:       {months.Count,12}");
        Console.WriteLine($"  Categories:           {categories.Count,12}");
        Console.WriteLine($"  Total Transactions:   {_transactions.Count,12}");
        Console.WriteLine("══════════════════════════════════════");
    }

    private void ShowByCategory()
    {
        if (!EnsureDataLoaded()) return;

        var summaries = _calculator.GetCategorySummaries(_transactions);
        var total = _calculator.GetTotalSpending(_transactions);

        Console.WriteLine();
        Console.WriteLine("══════════════════════════════════════════════════════════");
        Console.WriteLine(" SPENDING BY CATEGORY");
        Console.WriteLine("══════════════════════════════════════════════════════════");
        Console.WriteLine($"  {"Category",-25} {"Total",10}  {"% of Total",10}  {"Txns",5}");
        Console.WriteLine("  " + new string('─', 55));

        foreach (var s in summaries)
        {
            var pct = total > 0 ? s.TotalSpent / total * 100 : 0;
            Console.WriteLine($"  {s.Category,-25} {s.TotalSpent,10:C}  {pct,9:F1}%  {s.TransactionCount,5}");
        }

        Console.WriteLine("  " + new string('─', 55));
        Console.WriteLine($"  {"TOTAL",-25} {total,10:C}  {"100.0%",10}  {_transactions.Count,5}");
        Console.WriteLine("══════════════════════════════════════════════════════════");
    }

    private void ShowByMonth()
    {
        if (!EnsureDataLoaded()) return;

        var summaries = _calculator.GetMonthlySummaries(_transactions);

        Console.WriteLine();
        Console.WriteLine("════════════════════════════════════════════════");
        Console.WriteLine(" SPENDING BY MONTH");
        Console.WriteLine("════════════════════════════════════════════════");
        Console.WriteLine($"  {"Month",-20} {"Total",12}  {"Txns",5}");
        Console.WriteLine("  " + new string('─', 42));

        foreach (var s in summaries)
        {
            Console.WriteLine($"  {s.MonthLabel,-20} {s.TotalSpent,12:C}  {s.TransactionCount,5}");
        }

        Console.WriteLine("════════════════════════════════════════════════");
    }

    private void ShowAllTransactions()
    {
        if (!EnsureDataLoaded()) return;

        Console.WriteLine();
        Console.WriteLine("══════════════════════════════════════════════════════════════════════");
        Console.WriteLine(" ALL TRANSACTIONS");
        Console.WriteLine("══════════════════════════════════════════════════════════════════════");
        Console.WriteLine($"  {"Date",-12} {"Description",-30} {"Category",-20} {"Amount",10}");
        Console.WriteLine("  " + new string('─', 76));

        foreach (var t in _transactions.OrderBy(t => t.Date))
        {
            var desc = t.Description.Length > 28 ? t.Description[..28] + ".." : t.Description;
            Console.WriteLine($"  {t.Date.ToShortDateString(),-12} {desc,-30} {t.Category,-20} {t.Amount,10:C}");
        }

        Console.WriteLine("══════════════════════════════════════════════════════════════════════");
    }

    private bool EnsureDataLoaded()
    {
        if (_transactions.Count == 0)
        {
            Console.WriteLine("⚠️  No data loaded. Please load a CSV file first (option 1).");
            return false;
        }
        return true;
    }

    private void AddTransactions()
    {
        Console.WriteLine();
        Console.WriteLine("Add Transaction");
        Console.WriteLine("───────────────");

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

    private static DateTime PromptForDate()
    {
        while (true)
        {
            Console.Write("Date (MM/DD/YYYY, press Enter for today): ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
                return DateTime.Today;

            if (DateTime.TryParseExact(input, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                return parsedDate;

            Console.WriteLine("Invalid date. Use MM/DD/YYYY.");
        }
    }

    private static string PromptForRequiredText(string label)
    {
        while (true)
        {
            Console.Write($"{label}: ");
            var input = Console.ReadLine()?.Trim();
            if (!string.IsNullOrWhiteSpace(input))
                return input;

            Console.WriteLine($"{label} is required.");
        }
    }

    private static string PromptForCategory()
    {
        while (true)
        {
            Console.WriteLine("Category options:");
            for (int i = 0; i < DefaultCategories.Length; i++)
                Console.WriteLine($"{i + 1}. {DefaultCategories[i]}");
            Console.WriteLine($"{DefaultCategories.Length + 1}. Custom");
            Console.Write("Choose category number: ");

            var input = Console.ReadLine()?.Trim();
            if (!int.TryParse(input, out var choice))
            {
                Console.WriteLine($"Invalid category selection. Choose 1-{DefaultCategories.Length + 1}.");
                continue;
            }

            if (choice >= 1 && choice <= DefaultCategories.Length)
                return DefaultCategories[choice - 1];

            if (choice == DefaultCategories.Length + 1)
                return PromptForRequiredText("Custom category");

            Console.WriteLine($"Invalid category selection. Choose 1-{DefaultCategories.Length + 1}.");
        }
    }

    private static decimal PromptForAmount()
    {
        while (true)
        {
            Console.Write("Amount: ");
            var input = Console.ReadLine()?.Trim();

            if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out var amount))
                return amount;

            Console.WriteLine("Invalid amount. Enter a numeric value.");
        }
    }

    private static void PrintAddedTransaction(Transaction transaction)
    {
        Console.WriteLine();
        Console.WriteLine("Transaction added:");
        Console.WriteLine($"  Date: {transaction.Date:MM/dd/yyyy}");
        Console.WriteLine($"  Description: {transaction.Description}");
        Console.WriteLine($"  Category: {transaction.Category}");
        Console.WriteLine($"  Amount: {transaction.Amount:F2}");
        Console.WriteLine();
    }

    private static bool PromptYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine()?.Trim();

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

            Console.WriteLine("Please answer y or n.");
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

            Console.Write(prompt);
            var inputPath = Console.ReadLine()?.Trim();
            var outputPath = string.IsNullOrWhiteSpace(inputPath) ? _loadedFile : inputPath;

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                Console.WriteLine("Output path is required.");
                continue;
            }

            try
            {
                WriteTransactionsToCsv(outputPath);
                Console.WriteLine($"✅ Saved {_transactions.Count} transaction(s) to '{outputPath}'.");
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to save file: {ex.Message}");
                if (!PromptYesNo("Try a different path? (y/n): "))
                    return;
            }
        }
    }

    private void WriteTransactionsToCsv(string outputPath)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var builder = new StringBuilder();
        builder.AppendLine("Date,Description,Category,Amount");

        foreach (var transaction in _transactions)
        {
            builder.AppendLine(string.Join(",", new[]
            {
                transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                EscapeCsvField(transaction.Description),
                EscapeCsvField(transaction.Category),
                transaction.Amount.ToString(CultureInfo.InvariantCulture)
            }));
        }

        File.WriteAllText(outputPath, builder.ToString());
    }

    private static string EscapeCsvField(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n') && !value.Contains('\r'))
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
