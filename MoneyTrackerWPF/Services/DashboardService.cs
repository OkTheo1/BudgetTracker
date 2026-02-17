using System.IO;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Services;

public class DashboardService
{
    private readonly Func<AppDbContext> _contextFactory;
    private readonly CurrencyService _currencyService;

    public DashboardService(Func<AppDbContext> contextFactory, CurrencyService currencyService)
    {
        _contextFactory = contextFactory;
        _currencyService = currencyService;
    }

    public async Task<DashboardSummary> GetDashboardSummaryAsync()
    {
        using var context = _contextFactory();
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        // Total balance
        var totalBalance = context.Accounts.Sum(a => a.Balance);

        // Monthly spending
        var monthlySpending = context.Transactions
            .Where(t => t.Date >= startOfMonth && t.Date < endOfMonth && t.Type == TransactionType.Expense)
            .Sum(t => t.ConvertedAmount);

        // Monthly income
        var monthlyIncome = context.Transactions
            .Where(t => t.Date >= startOfMonth && t.Date < endOfMonth && t.Type == TransactionType.Income)
            .Sum(t => t.ConvertedAmount);

        // Total assets
        var totalAssets = context.Accounts.Sum(a => a.Balance);

        // Total liabilities
        var totalLiabilities = context.Debts.Sum(d => d.Balance);

        // Net worth
        var netWorth = totalAssets - totalLiabilities;

        // Recent transactions
        var recentTransactions = context.Transactions
            .OrderByDescending(t => t.Date)
            .Take(5)
            .ToList();

        // Budget status
        var currentMonthYear = now.ToString("yyyy-MM");
        var budgets = context.Budgets
            .Where(b => b.MonthYear == currentMonthYear)
            .ToList();

        var budgetSummary = budgets.Select(b => new BudgetStatus
        {
            CategoryName = context.Categories.Find(b.CategoryId)?.Name ?? "Unknown",
            Budgeted = b.Amount,
            Spent = b.Spent,
            Remaining = b.Amount - b.Spent,
            PercentUsed = b.Amount > 0 ? (b.Spent / b.Amount) * 100 : 0
        }).ToList();

        // Savings goals progress
        var goals = context.SavingsGoals.ToList();
        var goalSummary = goals.Select(g => new GoalProgress
        {
            Name = g.Name,
            TargetAmount = g.TargetAmount,
            CurrentAmount = g.CurrentAmount,
            Progress = g.TargetAmount > 0 ? (g.CurrentAmount / g.TargetAmount) * 100 : 0,
            TargetDate = g.TargetDate
        }).ToList();

        // Debt summary
        var debts = context.Debts.ToList();
        var debtSummary = new DebtSummary
        {
            TotalDebt = debts.Sum(d => d.Balance),
            TotalMinPayment = debts.Sum(d => d.MinPayment),
            AverageInterestRate = debts.Any() ? debts.Average(d => d.InterestRatePercent) : 0
        };

        return await Task.FromResult(new DashboardSummary
        {
            TotalBalance = totalBalance,
            MonthlySpending = monthlySpending,
            MonthlyIncome = monthlyIncome,
            NetFlow = monthlyIncome - monthlySpending,
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            NetWorth = netWorth,
            RecentTransactions = recentTransactions,
            Budgets = budgetSummary,
            Goals = goalSummary,
            DebtSummary = debtSummary,
            BaseCurrencySymbol = _currencyService.GetBaseCurrencySymbol()
        });
    }

    public async Task<MonthlyTrend> GetMonthlyTrendAsync(int months = 6)
    {
        using var context = _contextFactory();
        var trends = new List<MonthlyTrendData>();
        var now = DateTime.Now;

        for (int i = months - 1; i >= 0; i--)
        {
            var date = now.AddMonths(-i);
            var startOfMonth = new DateTime(date.Year, date.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            var income = context.Transactions
                .Where(t => t.Date >= startOfMonth && t.Date < endOfMonth && t.Type == TransactionType.Income)
                .Sum(t => t.ConvertedAmount);

            var spending = context.Transactions
                .Where(t => t.Date >= startOfMonth && t.Date < endOfMonth && t.Type == TransactionType.Expense)
                .Sum(t => t.ConvertedAmount);

            trends.Add(new MonthlyTrendData
            {
                Month = startOfMonth,
                Income = income,
                Spending = spending,
                Net = income - spending
            });
        }

        return await Task.FromResult(new MonthlyTrend { Data = trends });
    }

    public async Task<CategorySpending> GetCategorySpendingAsync(int year, int month)
    {
        using var context = _contextFactory();
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var transactions = context.Transactions
            .Where(t => t.Date >= startDate && t.Date < endDate && t.Type == TransactionType.Expense)
            .ToList();

        var categorySpending = transactions
            .GroupBy(t => t.CategoryId)
            .Select(g => new CategorySpendingItem
            {
                CategoryName = context.Categories.Find(g.Key)?.Name ?? "Unknown",
                CategoryColor = context.Categories.Find(g.Key)?.ColorHex ?? "#BDC3C7",
                Amount = g.Sum(t => t.ConvertedAmount),
                Percentage = 0 // Will be calculated below
            })
            .ToList();

        var total = categorySpending.Sum(c => c.Amount);
        if (total > 0)
        {
            foreach (var item in categorySpending)
            {
                item.Percentage = (item.Amount / total) * 100;
            }
        }

        return await Task.FromResult(new CategorySpending 
        { 
            Items = categorySpending, 
            Total = total 
        });
    }
}

public class DashboardSummary
{
    public decimal TotalBalance { get; set; }
    public decimal MonthlySpending { get; set; }
    public decimal MonthlyIncome { get; set; }
    public decimal NetFlow { get; set; }
    public decimal TotalAssets { get; set; }
    public decimal TotalLiabilities { get; set; }
    public decimal NetWorth { get; set; }
    public List<Transaction> RecentTransactions { get; set; } = new();
    public List<BudgetStatus> Budgets { get; set; } = new();
    public List<GoalProgress> Goals { get; set; } = new();
    public DebtSummary DebtSummary { get; set; } = new();
    public string BaseCurrencySymbol { get; set; } = "£";
}

public class BudgetStatus
{
    public string CategoryName { get; set; } = "";
    public decimal Budgeted { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining { get; set; }
    public decimal PercentUsed { get; set; }
}

public class GoalProgress
{
    public string Name { get; set; } = "";
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal Progress { get; set; }
    public DateTime? TargetDate { get; set; }
}

public class DebtSummary
{
    public decimal TotalDebt { get; set; }
    public decimal TotalMinPayment { get; set; }
    public decimal AverageInterestRate { get; set; }
}

public class MonthlyTrend
{
    public List<MonthlyTrendData> Data { get; set; } = new();
}

public class MonthlyTrendData
{
    public DateTime Month { get; set; }
    public decimal Income { get; set; }
    public decimal Spending { get; set; }
    public decimal Net { get; set; }
}

public class CategorySpending
{
    public List<CategorySpendingItem> Items { get; set; } = new();
    public decimal Total { get; set; }
}

public class CategorySpendingItem
{
    public string CategoryName { get; set; } = "";
    public string CategoryColor { get; set; } = "#BDC3C7";
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}
