using BudgetApp.Models;

namespace BudgetApp.Services;

public class BudgetCalculator
{
    /// <summary>
    /// Calculates the total spending across all transactions.
    /// </summary>
    /// <param name="transactions">The transactions to total.</param>
    /// <returns>The sum of all transaction amounts.</returns>
    public decimal GetTotalSpending(IEnumerable<Transaction> transactions)
        => transactions.Sum(t => t.Amount);

    /// <summary>
    /// Groups transactions by category and returns totals for each category.
    /// </summary>
    /// <param name="transactions">The transactions to group.</param>
    /// <returns>A list of category summaries ordered by total spent descending.</returns>
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

    /// <summary>
    /// Groups transactions by year and month and returns totals for each month.
    /// </summary>
    /// <param name="transactions">The transactions to group.</param>
    /// <returns>A list of monthly summaries ordered chronologically.</returns>
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

    /// <summary>
    /// Gets the category with the highest total spending.
    /// </summary>
    /// <param name="transactions">The transactions to evaluate.</param>
    /// <returns>The top spending category, or N/A when there are no transactions.</returns>
    public string GetTopSpendingCategory(IEnumerable<Transaction> transactions)
    {
        var categories = GetCategorySummaries(transactions);
        return categories.Count > 0 ? categories[0].Category : "N/A";
    }

    /// <summary>
    /// Calculates the average total spending per month.
    /// </summary>
    /// <param name="transactions">The transactions to evaluate.</param>
    /// <returns>The average monthly spending, or 0 when there are no transactions.</returns>
    public decimal GetAverageMonthlySpending(IEnumerable<Transaction> transactions)
    {
        var monthly = GetMonthlySummaries(transactions);
        if (monthly.Count == 0) return 0;
        return monthly.Average(m => m.TotalSpent);
    }
}
