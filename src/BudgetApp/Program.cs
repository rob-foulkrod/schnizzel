using BudgetApp;
using BudgetApp.Services;

var parser = new CsvParser();
var calculator = new BudgetCalculator();
var menu = new BudgetMenu(parser, calculator);

menu.Run();
