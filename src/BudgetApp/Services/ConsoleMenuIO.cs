namespace BudgetApp.Services;

public class ConsoleMenuIO : IMenuIO
{
    public string? ReadLine() => Console.ReadLine();

    public void Write(string value) => Console.Write(value);

    public void WriteLine(string value = "") => Console.WriteLine(value);
}
