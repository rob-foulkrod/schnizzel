using BudgetApp.Models;

namespace BudgetApp.Services;

public class CsvParser
{
    private static readonly string[] ExpectedHeaders = { "Date", "Description", "Category", "Amount" };

    /// <summary>
    /// Reads a CSV file from disk and parses it into budget transactions.
    /// </summary>
    /// <param name="filePath">The path to the CSV file to parse.</param>
    /// <returns>A read-only list of <see cref="Transaction"/> values parsed from the file.</returns>
    /// <exception cref="FileNotFoundException">
    /// Thrown when <paramref name="filePath"/> does not exist.
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// Thrown when the CSV content is empty, has invalid headers, or contains invalid row values.
    /// </exception>
    /// <remarks>
    /// This is the entry point when your data already lives in a file. The method reads the full file text,
    /// then delegates to <see cref="ParseContent(string)"/> for the actual parsing and validation.
    /// </remarks>
    public IReadOnlyList<Transaction> Parse(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"CSV file not found: {filePath}");

        var content = File.ReadAllText(filePath);
        return ParseContent(content);
    }

    /// <summary>
    /// Parses CSV text into budget transactions.
    /// </summary>
    /// <param name="csvContent">The raw CSV text, including a header row.</param>
    /// <returns>A read-only list of <see cref="Transaction"/> values parsed from the CSV content.</returns>
    /// <exception cref="InvalidDataException">
    /// Thrown when content is empty, headers are invalid, or one or more data rows cannot be parsed.
    /// </exception>
    /// <remarks>
    /// Expected header order is: <c>Date,Description,Category,Amount</c>.
    /// Values are parsed row by row after header validation. Quoted fields are supported through
    /// <see cref="ParseLine(string)"/>, including escaped quotes represented as <c>""</c>.
    /// </remarks>
    public IReadOnlyList<Transaction> ParseContent(string csvContent)
    {
        if (string.IsNullOrWhiteSpace(csvContent))
            throw new InvalidDataException("CSV content is empty.");

        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
            throw new InvalidDataException("CSV content is empty.");

        var headers = ParseLine(lines[0].Trim());
        ValidateHeaders(headers);

        return ParseDataLines(lines);
    }

    private static IReadOnlyList<Transaction> ParseDataLines(string[] lines)
    {
        var transactions = new List<Transaction>();

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = ParseLine(line);
            if (fields.Length < 4)
                throw new InvalidDataException($"Line {i + 1} does not have enough columns: '{line}'");

            if (!DateTime.TryParse(fields[0], out var date))
                throw new InvalidDataException($"Line {i + 1} has an invalid date: '{fields[0]}'");

            if (!decimal.TryParse(fields[3], out var amount))
                throw new InvalidDataException($"Line {i + 1} has an invalid amount: '{fields[3]}'");

            transactions.Add(new Transaction
            {
                Date = date,
                Description = fields[1].Trim(),
                Category = fields[2].Trim(),
                Amount = amount
            });
        }

        return transactions;
    }

    private static void ValidateHeaders(string[] headers)
    {
        for (int i = 0; i < ExpectedHeaders.Length; i++)
        {
            if (i >= headers.Length || !string.Equals(headers[i].Trim(), ExpectedHeaders[i], StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(
                    $"Invalid CSV header. Expected '{string.Join(",", ExpectedHeaders)}' but got '{string.Join(",", headers)}'.");
        }
    }

    /// <summary>
    /// Splits a single CSV row into fields while handling quoted values.
    /// </summary>
    /// <param name="line">A single CSV line.</param>
    /// <returns>An array of fields in the order they appear in the line.</returns>
    /// <remarks>
    /// Parsing behavior:
    /// - Commas outside quotes are treated as field separators.
    /// - Text inside quotes is treated as one field, even if it contains commas.
    /// - Escaped quotes inside quoted text use two quote characters (<c>""</c>), which are unescaped to <c>"</c>.
    ///
    /// Example input:
    /// <c>2026-01-01,"Coffee, Large",Food,4.95</c>
    /// returns four fields where the description is <c>Coffee, Large</c>.
    /// </remarks>
    public static string[] ParseLine(string line)
    {
        var fields = new List<string>();
        bool inQuotes = false;
        var current = new System.Text.StringBuilder();

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields.ToArray();
    }
}
