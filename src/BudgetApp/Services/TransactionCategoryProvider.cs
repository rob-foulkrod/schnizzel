namespace BudgetApp.Services;

public class TransactionCategoryProvider
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

    private readonly IReadOnlyList<string> _categories;

    public TransactionCategoryProvider(IEnumerable<string>? categories = null)
    {
        var normalizedCategories = categories?
            .Where(category => !string.IsNullOrWhiteSpace(category))
            .Select(category => category.Trim())
            .ToList();

        if (normalizedCategories is null || normalizedCategories.Count == 0)
        {
            _categories = DefaultCategories;
            return;
        }

        _categories = normalizedCategories;
    }

    public IReadOnlyList<string> GetCategories() => _categories;
}
