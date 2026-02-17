namespace MoneyTracker2026.Models;

public class MonthlyReport
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal Net => TotalIncome - TotalExpenses;
    public List<CategorySpending> TopCategories { get; set; } = new();
}

public class CategorySpending
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
