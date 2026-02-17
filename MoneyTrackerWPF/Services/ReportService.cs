using System.IO;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Services;

public class ReportService
{
    private readonly Func<AppDbContext> _contextFactory;

    public ReportService(Func<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Dictionary<string, decimal>> GetMonthlySpendingSummaryAsync(int year, int month)
    {
        using var context = _contextFactory();
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var transactions = context.Transactions
            .Where(t => t.Date >= startDate && t.Date < endDate && t.Type == TransactionType.Expense)
            .ToList();

        var summary = transactions
            .GroupBy(t => t.CategoryId)
            .ToDictionary(
                g => context.Categories.Find(g.Key)?.Name ?? "Unknown",
                g => g.Sum(t => t.ConvertedAmount));

        return await Task.FromResult(summary);
    }

    public async Task<Dictionary<string, decimal>> GetMonthlyIncomeSummaryAsync(int year, int month)
    {
        using var context = _contextFactory();
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var transactions = context.Transactions
            .Where(t => t.Date >= startDate && t.Date < endDate && t.Type == TransactionType.Income)
            .ToList();

        var summary = transactions
            .GroupBy(t => t.CategoryId)
            .ToDictionary(
                g => context.Categories.Find(g.Key)?.Name ?? "Unknown",
                g => g.Sum(t => t.ConvertedAmount));

        return await Task.FromResult(summary);
    }

    public async Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime start, DateTime end)
    {
        using var context = _contextFactory();
        return await Task.FromResult(context.Transactions
            .Where(t => t.Date >= start && t.Date <= end)
            .OrderByDescending(t => t.Date)
            .ToList());
    }

    public async Task<List<Transaction>> GetTransactionsByCategoryAsync(int categoryId)
    {
        using var context = _contextFactory();
        return await Task.FromResult(context.Transactions
            .Where(t => t.CategoryId == categoryId)
            .OrderByDescending(t => t.Date)
            .ToList());
    }

    public async Task<Dictionary<string, decimal>> GetYearlySpendingByCategoryAsync(int year)
    {
        using var context = _contextFactory();
        var startDate = new DateTime(year, 1, 1);
        var endDate = startDate.AddYears(1);

        var transactions = context.Transactions
            .Where(t => t.Date >= startDate && t.Date < endDate && t.Type == TransactionType.Expense)
            .ToList();

        var summary = transactions
            .GroupBy(t => t.CategoryId)
            .ToDictionary(
                g => context.Categories.Find(g.Key)?.Name ?? "Unknown",
                g => g.Sum(t => t.ConvertedAmount));

        return await Task.FromResult(summary);
    }

    public async Task<decimal> GetTotalNetWorthAsync()
    {
        using var context = _contextFactory();
        var assets = context.Accounts.Sum(a => a.Balance);
        var liabilities = context.Debts.Sum(d => d.Balance);
        return await Task.FromResult(assets - liabilities);
    }

    public async Task<Dictionary<string, object>> GetFinancialSummaryAsync(int year, int month)
    {
        var monthlySpending = await GetMonthlySpendingSummaryAsync(year, month);
        var monthlyIncome = await GetMonthlyIncomeSummaryAsync(year, month);
        
        var totalSpending = monthlySpending.Values.Sum();
        var totalIncome = monthlyIncome.Values.Sum();
        var netFlow = totalIncome - totalSpending;

        return new Dictionary<string, object>
        {
            { "TotalIncome", totalIncome },
            { "TotalSpending", totalSpending },
            { "NetFlow", netFlow },
            { "SpendingByCategory", monthlySpending },
            { "IncomeByCategory", monthlyIncome }
        };
    }

    public async Task<List<NetWorthEntry>> GetNetWorthHistoryAsync(int months = 12)
    {
        using var context = _contextFactory();
        var startDate = DateTime.Now.AddMonths(-months);
        
        return await Task.FromResult(context.NetWorthEntries
            .Where(n => n.Date >= startDate)
            .OrderBy(n => n.Date)
            .ToList());
    }

    public async Task<Dictionary<string, decimal>> GetDebtPayoffProjectionAsync(string method)
    {
        using var context = _contextFactory();
        var debts = context.Debts.Where(d => d.Balance > 0).ToList();
        
        if (!debts.Any())
            return new Dictionary<string, decimal>();

        // Simple projection - in reality, this would be more complex
        var totalDebt = debts.Sum(d => d.Balance);
        var avgInterest = debts.Average(d => d.InterestRatePercent);
        
        var projection = new Dictionary<string, decimal>
        {
            { "TotalDebt", totalDebt },
            { "AverageInterestRate", avgInterest },
            { "EstimatedMonthsToPayoff", totalDebt / 500m }, // Simplified
            { "Method", method == "snowball" ? 1 : 0 }
        };

        return await Task.FromResult(projection);
    }
}
