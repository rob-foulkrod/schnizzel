namespace BudgetApp.Models;

public class CategorySummary
{
    public string Category { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int TransactionCount { get; set; }
}
