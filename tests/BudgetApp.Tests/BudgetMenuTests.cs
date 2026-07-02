using BudgetApp;
using BudgetApp.Services;
using System.Globalization;
using System.Text;

namespace BudgetApp.Tests;

public class BudgetMenuTests
{
    [Fact]
    public void Run_AddTransactionWithCustomCategory_ShowsConfirmation()
    {
        using var cultureScope = new CultureScope("en-US");

        var output = RunMenu(
            "6\n" +
            "\n" +
            "Morning Coffee\n" +
            "12\n" +
            "Cafe\n" +
            "-4.50\n" +
            "n\n" +
            "7\n" +
            "n\n");

        Assert.Contains("Transaction added:", output);
        Assert.Contains("Description: Morning Coffee", output);
        Assert.Contains("Category: Cafe", output);
        Assert.Contains("Amount: -4.50", output);
    }

    [Fact]
    public void Run_InvalidDate_RePromptsUntilValidDate()
    {
        using var cultureScope = new CultureScope("en-US");

        var output = RunMenu(
            "6\n" +
            "13/40/2026\n" +
            "07/04/2026\n" +
            "Fireworks\n" +
            "1\n" +
            "10\n" +
            "n\n" +
            "7\n" +
            "n\n");

        Assert.Contains("Invalid date. Use MM/DD/YYYY.", output);
        Assert.Contains("Date: 07/04/2026", output);
    }

    [Fact]
    public void Run_ExitAndSave_WritesCsvThatCanBeParsed()
    {
        using var cultureScope = new CultureScope("en-US");
        var parser = new CsvParser();
        var tempFile = Path.Combine(Path.GetTempPath(), $"budgetmenu-{Guid.NewGuid():N}.csv");

        try
        {
            var output = RunMenu(
                "6\n" +
                "07/01/2026\n" +
                "Groceries\n" +
                "1\n" +
                "12.34\n" +
                "n\n" +
                "7\n" +
                "y\n" +
                tempFile + "\n");

            Assert.Contains("Saved 1 transaction(s)", output);
            Assert.True(File.Exists(tempFile));

            var parsed = parser.Parse(tempFile);
            Assert.Single(parsed);
            Assert.Equal("Groceries", parsed[0].Description);
            Assert.Equal("Food", parsed[0].Category);
            Assert.Equal(12.34m, parsed[0].Amount);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Run_ExitWithNoTransactions_DoesNotPromptToSave()
    {
        var output = RunMenu("7\n");

        Assert.DoesNotContain("Save transactions to CSV before exit?", output);
    }

    private static string RunMenu(string input)
    {
        var parser = new CsvParser();
        var calculator = new BudgetCalculator();
        var menuIo = new FakeMenuIo(input);
        var categoryProvider = new TransactionCategoryProvider();
        var transactionCsvStore = new TransactionCsvStore();
        var timeProvider = TimeProvider.System;

        var menu = new BudgetMenu(parser, calculator, menuIo, categoryProvider, transactionCsvStore, timeProvider);
        menu.Run();

        return menuIo.GetOutput();
    }

    private sealed class FakeMenuIo : IMenuIO
    {
        private readonly Queue<string?> _inputs;
        private readonly StringBuilder _output = new();

        public FakeMenuIo(string input)
        {
            ArgumentNullException.ThrowIfNull(input);
            _inputs = new Queue<string?>(input.Split('\n').Select(value => value.TrimEnd('\r')));
        }

        public string? ReadLine()
        {
            if (_inputs.Count == 0)
                return string.Empty;

            return _inputs.Dequeue();
        }

        public void Write(string value)
        {
            _output.Append(value);
        }

        public void WriteLine(string value = "")
        {
            _output.AppendLine(value);
        }

        public string GetOutput() => _output.ToString();
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture;
        private readonly CultureInfo _originalUiCulture;

        public CultureScope(string cultureName)
        {
            _originalCulture = CultureInfo.CurrentCulture;
            _originalUiCulture = CultureInfo.CurrentUICulture;

            var culture = CultureInfo.GetCultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _originalCulture;
            CultureInfo.CurrentUICulture = _originalUiCulture;
        }
    }
}
