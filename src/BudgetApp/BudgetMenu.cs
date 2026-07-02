using BudgetApp.Models;
using BudgetApp.Services;

namespace BudgetApp;

public class BudgetMenu
{
    private readonly CsvParser _parser;
    private readonly BudgetCalculator _calculator;
    private IReadOnlyList<Transaction> _transactions = Array.Empty<Transaction>();
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
                    running = false;
                    Console.WriteLine();
                    Console.WriteLine("Goodbye! 👋");
                    break;
                default:
                    Console.WriteLine("Invalid option. Please choose 1-6.");
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
        Console.WriteLine("6. Exit");
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
            _transactions = _parser.Parse(path);
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
}
