namespace BudgetApp.Models;

public class MonthlySummary
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalSpent { get; set; }
    public int TransactionCount { get; set; }

    public string MonthLabel => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
}
