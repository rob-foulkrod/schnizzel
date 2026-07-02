using BudgetApp.Models;
using BudgetApp.Services;

namespace BudgetApp.Tests;

public class BudgetCalculatorTests
{
    private readonly BudgetCalculator _calc = new BudgetCalculator();

    private static List<Transaction> SampleTransactions() =>
        new List<Transaction>
        {
            new Transaction { Date = new DateTime(2024, 1, 5),  Description = "Whole Foods",  Category = "Groceries",      Amount = 85.42m },
            new Transaction { Date = new DateTime(2024, 1, 10), Description = "Shell Gas",    Category = "Transportation", Amount = 62.00m },
            new Transaction { Date = new DateTime(2024, 1, 15), Description = "Chipotle",     Category = "Dining Out",     Amount = 13.75m },
            new Transaction { Date = new DateTime(2024, 2, 3),  Description = "Trader Joe's", Category = "Groceries",      Amount = 91.15m },
            new Transaction { Date = new DateTime(2024, 2, 14), Description = "BP Gas",       Category = "Transportation", Amount = 55.00m },
            new Transaction { Date = new DateTime(2024, 2, 20), Description = "Target",       Category = "Shopping",       Amount = 43.25m },
            new Transaction { Date = new DateTime(2024, 3, 2),  Description = "Aldi",         Category = "Groceries",      Amount = 55.20m },
        };

    // ── GetTotalSpending ──────────────────────────────────────────────────────

    [Fact]
    public void GetTotalSpending_AllTransactions_ReturnsCorrectTotal()
    {
        var total = _calc.GetTotalSpending(SampleTransactions());
        Assert.Equal(405.77m, total);
    }

    [Fact]
    public void GetTotalSpending_EmptyList_ReturnsZero()
    {
        Assert.Equal(0m, _calc.GetTotalSpending(new List<Transaction>()));
    }

    // ── GetCategorySummaries ──────────────────────────────────────────────────

    [Fact]
    public void GetCategorySummaries_ReturnsCorrectNumberOfCategories()
    {
        var summaries = _calc.GetCategorySummaries(SampleTransactions());
        Assert.Equal(4, summaries.Count);
    }

    [Fact]
    public void GetCategorySummaries_OrderedByTotalDescending()
    {
        var summaries = _calc.GetCategorySummaries(SampleTransactions());
        for (int i = 1; i < summaries.Count; i++)
            Assert.True(summaries[i - 1].TotalSpent >= summaries[i].TotalSpent);
    }

    [Fact]
    public void GetCategorySummaries_GroceriesTotal_IsCorrect()
    {
        var summaries = _calc.GetCategorySummaries(SampleTransactions());
        var groceries = summaries.Single(s => s.Category == "Groceries");
        Assert.Equal(231.77m, groceries.TotalSpent);
    }

    [Fact]
    public void GetCategorySummaries_GroceriesTransactionCount_IsCorrect()
    {
        var summaries = _calc.GetCategorySummaries(SampleTransactions());
        var groceries = summaries.Single(s => s.Category == "Groceries");
        Assert.Equal(3, groceries.TransactionCount);
    }

    [Fact]
    public void GetCategorySummaries_EmptyList_ReturnsEmptyList()
    {
        var summaries = _calc.GetCategorySummaries(new List<Transaction>());
        Assert.Empty(summaries);
    }

    [Fact]
    public void GetCategorySummaries_CategoryMatchIsCaseInsensitive()
    {
        var transactions = new List<Transaction>
        {
            new Transaction { Date = DateTime.Today, Description = "A", Category = "groceries", Amount = 10m },
            new Transaction { Date = DateTime.Today, Description = "B", Category = "Groceries", Amount = 20m },
        };
        var summaries = _calc.GetCategorySummaries(transactions);
        Assert.Single(summaries);
        Assert.Equal(30m, summaries[0].TotalSpent);
    }

    // ── GetMonthlySummaries ───────────────────────────────────────────────────

    [Fact]
    public void GetMonthlySummaries_ReturnsCorrectNumberOfMonths()
    {
        var summaries = _calc.GetMonthlySummaries(SampleTransactions());
        Assert.Equal(3, summaries.Count);
    }

    [Fact]
    public void GetMonthlySummaries_OrderedChronologically()
    {
        var summaries = _calc.GetMonthlySummaries(SampleTransactions());
        for (int i = 1; i < summaries.Count; i++)
        {
            var prev = summaries[i - 1];
            var curr = summaries[i];
            Assert.True(prev.Year < curr.Year || (prev.Year == curr.Year && prev.Month <= curr.Month));
        }
    }

    [Fact]
    public void GetMonthlySummaries_JanuaryTotal_IsCorrect()
    {
        var summaries = _calc.GetMonthlySummaries(SampleTransactions());
        var jan = summaries.Single(s => s.Year == 2024 && s.Month == 1);
        Assert.Equal(161.17m, jan.TotalSpent);
    }

    [Fact]
    public void GetMonthlySummaries_MonthLabel_FormattedCorrectly()
    {
        var summaries = _calc.GetMonthlySummaries(SampleTransactions());
        var jan = summaries.Single(s => s.Year == 2024 && s.Month == 1);
        Assert.Equal("January 2024", jan.MonthLabel);
    }

    [Fact]
    public void GetMonthlySummaries_EmptyList_ReturnsEmptyList()
    {
        var summaries = _calc.GetMonthlySummaries(new List<Transaction>());
        Assert.Empty(summaries);
    }

    // ── GetTopSpendingCategory ────────────────────────────────────────────────

    [Fact]
    public void GetTopSpendingCategory_ReturnsHighestCategory()
    {
        Assert.Equal("Groceries", _calc.GetTopSpendingCategory(SampleTransactions()));
    }

    [Fact]
    public void GetTopSpendingCategory_EmptyList_ReturnsNA()
    {
        Assert.Equal("N/A", _calc.GetTopSpendingCategory(new List<Transaction>()));
    }

    // ── GetAverageMonthlySpending ─────────────────────────────────────────────

    [Fact]
    public void GetAverageMonthlySpending_ReturnsCorrectAverage()
    {
        var avg = _calc.GetAverageMonthlySpending(SampleTransactions());
        // Jan: 161.17, Feb: 189.40, Mar: 55.20 → avg ≈ 135.26
        Assert.Equal(135.26m, Math.Round(avg, 2));
    }

    [Fact]
    public void GetAverageMonthlySpending_EmptyList_ReturnsZero()
    {
        Assert.Equal(0m, _calc.GetAverageMonthlySpending(new List<Transaction>()));
    }

    [Fact]
    public void GetAverageMonthlySpending_SingleMonth_ReturnsThatMonthTotal()
    {
        var transactions = new List<Transaction>
        {
            new Transaction { Date = new DateTime(2024, 1, 5), Description = "A", Category = "Cat", Amount = 100m },
            new Transaction { Date = new DateTime(2024, 1, 15), Description = "B", Category = "Cat", Amount = 50m },
        };
        Assert.Equal(150m, _calc.GetAverageMonthlySpending(transactions));
    }
}
