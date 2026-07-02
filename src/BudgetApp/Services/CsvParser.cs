using BudgetApp.Models;

namespace BudgetApp.Services;

public class CsvParser
{
    private static readonly string[] ExpectedHeaders = { "Date", "Description", "Category", "Amount" };

    public IReadOnlyList<Transaction> Parse(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"CSV file not found: {filePath}");

        var content = File.ReadAllText(filePath);
        return ParseContent(content);
    }

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
