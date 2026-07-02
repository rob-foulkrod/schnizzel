using BudgetApp.Models;

namespace BudgetApp.Services;

public class BudgetCalculator
{
    public decimal GetTotalSpending(IEnumerable<Transaction> transactions)
        => transactions.Sum(t => t.Amount);

    public IReadOnlyList<CategorySummary> GetCategorySummaries(IEnumerable<Transaction> transactions)
    {
        return transactions
            .GroupBy(t => t.Category, StringComparer.OrdinalIgnoreCase)
            .Select(g => new CategorySummary
            {
                Category = g.Key,
                TotalSpent = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .OrderByDescending(c => c.TotalSpent)
            .ToList();
    }

    public IReadOnlyList<MonthlySummary> GetMonthlySummaries(IEnumerable<Transaction> transactions)
    {
        return transactions
            .GroupBy(t => new { t.Date.Year, t.Date.Month })
            .Select(g => new MonthlySummary
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalSpent = g.Sum(t => t.Amount),
                TransactionCount = g.Count()
            })
            .OrderBy(m => m.Year)
            .ThenBy(m => m.Month)
            .ToList();
    }

    public string GetTopSpendingCategory(IEnumerable<Transaction> transactions)
    {
        var categories = GetCategorySummaries(transactions);
        return categories.Count > 0 ? categories[0].Category : "N/A";
    }

    public decimal GetAverageMonthlySpending(IEnumerable<Transaction> transactions)
    {
        var monthly = GetMonthlySummaries(transactions);
        if (monthly.Count == 0) return 0;
        return monthly.Average(m => m.TotalSpent);
    }
}
