using BudgetApp;
using BudgetApp.Services;

var parser = new CsvParser();
var calculator = new BudgetCalculator();
var menuIo = new ConsoleMenuIO();
var categoryProvider = new TransactionCategoryProvider();
var transactionCsvStore = new TransactionCsvStore();
var menu = new BudgetMenu(parser, calculator, menuIo, categoryProvider, transactionCsvStore);

menu.Run();
