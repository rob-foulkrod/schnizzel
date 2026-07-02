namespace BudgetApp.Services;

public interface IMenuIO
{
    string? ReadLine();

    void Write(string value);

    void WriteLine(string value = "");
}
