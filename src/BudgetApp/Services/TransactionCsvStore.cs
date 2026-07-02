using BudgetApp.Models;
using System.Globalization;
using System.Text;

namespace BudgetApp.Services;

public class TransactionCsvStore
{
    /// <summary>
    /// Writes transactions to disk as CSV using the expected BudgetApp header order.
    /// </summary>
    /// <param name="outputPath">The file path where CSV content will be written.</param>
    /// <param name="transactions">The transactions to write.</param>
    public void Save(string outputPath, IEnumerable<Transaction> transactions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);
        ArgumentNullException.ThrowIfNull(transactions);

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var builder = new StringBuilder();
        builder.AppendLine("Date,Description,Category,Amount");

        foreach (var transaction in transactions)
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
