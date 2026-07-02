using BudgetApp.Models;
using BudgetApp.Services;

namespace BudgetApp.Tests;

public class CsvParserTests
{
    private readonly CsvParser _parser = new CsvParser();

    private const string ValidCsv =
        "Date,Description,Category,Amount\n" +
        "2024-01-05,Whole Foods,Groceries,85.42\n" +
        "2024-01-10,Shell Gas,Transportation,62.00\n" +
        "2024-01-15,Chipotle,Dining Out,13.75\n";

    [Fact]
    public void ParseContent_ValidCsv_ReturnsCorrectTransactionCount()
    {
        var transactions = _parser.ParseContent(ValidCsv);
        Assert.Equal(3, transactions.Count);
    }

    [Fact]
    public void ParseContent_ValidCsv_ParsesDateCorrectly()
    {
        var transactions = _parser.ParseContent(ValidCsv);
        Assert.Equal(new DateTime(2024, 1, 5), transactions[0].Date);
    }

    [Fact]
    public void ParseContent_ValidCsv_ParsesDescriptionCorrectly()
    {
        var transactions = _parser.ParseContent(ValidCsv);
        Assert.Equal("Whole Foods", transactions[0].Description);
    }

    [Fact]
    public void ParseContent_ValidCsv_ParsesCategoryCorrectly()
    {
        var transactions = _parser.ParseContent(ValidCsv);
        Assert.Equal("Groceries", transactions[0].Category);
    }

    [Fact]
    public void ParseContent_ValidCsv_ParsesAmountCorrectly()
    {
        var transactions = _parser.ParseContent(ValidCsv);
        Assert.Equal(85.42m, transactions[0].Amount);
    }

    [Fact]
    public void ParseContent_EmptyContent_ThrowsInvalidDataException()
    {
        Assert.Throws<InvalidDataException>(() => _parser.ParseContent(""));
    }

    [Fact]
    public void ParseContent_WhitespaceOnly_ThrowsInvalidDataException()
    {
        Assert.Throws<InvalidDataException>(() => _parser.ParseContent("   "));
    }

    [Fact]
    public void ParseContent_WrongHeaders_ThrowsInvalidDataException()
    {
        const string badCsv = "Wrong,Headers,Here,Nope\n2024-01-05,Test,Cat,10.00\n";
        Assert.Throws<InvalidDataException>(() => _parser.ParseContent(badCsv));
    }

    [Fact]
    public void ParseContent_InvalidDate_ThrowsInvalidDataException()
    {
        const string badCsv = "Date,Description,Category,Amount\nnot-a-date,Test,Cat,10.00\n";
        Assert.Throws<InvalidDataException>(() => _parser.ParseContent(badCsv));
    }

    [Fact]
    public void ParseContent_InvalidAmount_ThrowsInvalidDataException()
    {
        const string badCsv = "Date,Description,Category,Amount\n2024-01-05,Test,Cat,not-a-number\n";
        Assert.Throws<InvalidDataException>(() => _parser.ParseContent(badCsv));
    }

    [Fact]
    public void ParseContent_TooFewColumns_ThrowsInvalidDataException()
    {
        const string badCsv = "Date,Description,Category,Amount\n2024-01-05,Test\n";
        Assert.Throws<InvalidDataException>(() => _parser.ParseContent(badCsv));
    }

    [Fact]
    public void ParseContent_SkipsBlankLines()
    {
        const string csv =
            "Date,Description,Category,Amount\n" +
            "2024-01-05,Whole Foods,Groceries,85.42\n" +
            "\n" +
            "2024-01-10,Shell Gas,Transportation,62.00\n";

        var transactions = _parser.ParseContent(csv);
        Assert.Equal(2, transactions.Count);
    }

    [Fact]
    public void ParseContent_HeaderCaseInsensitive_Works()
    {
        const string csv =
            "date,description,category,amount\n" +
            "2024-01-05,Whole Foods,Groceries,85.42\n";

        var transactions = _parser.ParseContent(csv);
        Assert.Single(transactions);
    }

    [Fact]
    public void ParseContent_QuotedDescriptionWithComma_ParsesCorrectly()
    {
        const string csv =
            "Date,Description,Category,Amount\n" +
            "2024-01-05,\"Smith, John Store\",Groceries,85.42\n";

        var transactions = _parser.ParseContent(csv);
        Assert.Equal("Smith, John Store", transactions[0].Description);
    }

    [Fact]
    public void Parse_FileNotFound_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => _parser.Parse("/nonexistent/path/file.csv"));
    }

    [Fact]
    public void Parse_ValidFile_ReturnsTransactions()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile,
                "Date,Description,Category,Amount\n" +
                "2024-01-05,Test Store,Groceries,50.00\n");

            var transactions = _parser.Parse(tempFile);
            Assert.Single(transactions);
            Assert.Equal("Test Store", transactions[0].Description);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseLine_SimpleFields_SplitsCorrectly()
    {
        var fields = CsvParser.ParseLine("a,b,c,d");
        Assert.Equal(new[] { "a", "b", "c", "d" }, fields);
    }

    [Fact]
    public void ParseLine_QuotedFieldWithComma_SplitsCorrectly()
    {
        var fields = CsvParser.ParseLine("\"hello, world\",b,c");
        Assert.Equal(new[] { "hello, world", "b", "c" }, fields);
    }

    [Fact]
    public void ParseLine_EscapedQuoteInField_SplitsCorrectly()
    {
        var fields = CsvParser.ParseLine("\"say \"\"hello\"\"\",b");
        Assert.Equal(new[] { "say \"hello\"", "b" }, fields);
    }
}
