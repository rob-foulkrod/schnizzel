using BudgetApp;
using BudgetApp.Services;
using System.Globalization;

namespace BudgetApp.Tests;

public class BudgetMenuTests
{
    private static readonly object ConsoleLock = new();

    [Fact]
    public void Run_AddTransactionWithCustomCategory_ShowsConfirmation()
    {
        lock (ConsoleLock)
        {
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
    }

    [Fact]
    public void Run_InvalidDate_RePromptsUntilValidDate()
    {
        lock (ConsoleLock)
        {
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
    }

    [Fact]
    public void Run_ExitAndSave_WritesCsvThatCanBeParsed()
    {
        lock (ConsoleLock)
        {
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
    }

    [Fact]
    public void Run_ExitWithNoTransactions_DoesNotPromptToSave()
    {
        lock (ConsoleLock)
        {
            var output = RunMenu("7\n");
            Assert.DoesNotContain("Save transactions to CSV before exit?", output);
        }
    }

    private static string RunMenu(string input)
    {
        var originalIn = Console.In;
        var originalOut = Console.Out;
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUICulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");

            using var reader = new StringReader(input);
            using var writer = new StringWriter();

            Console.SetIn(reader);
            Console.SetOut(writer);

            var menu = new BudgetMenu(new CsvParser(), new BudgetCalculator());
            menu.Run();

            return writer.ToString();
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUICulture;
            Console.SetIn(originalIn);
            Console.SetOut(originalOut);
        }
    }
}
