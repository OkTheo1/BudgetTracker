using Microsoft.EntityFrameworkCore;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Services;

public class BudgetService
{
    private readonly Func<AppDbContext> _contextFactory;
    private readonly SettingsService _settingsService;

    public BudgetService(Func<AppDbContext> contextFactory, SettingsService settingsService)
    {
        _contextFactory = contextFactory;
        _settingsService = settingsService;
    }

    public string GetCurrentMonthYear()
    {
        return DateTime.Now.ToString("yyyy-MM");
    }

    public async Task<List<Budget>> GetBudgetsForMonthAsync(string monthYear)
    {
        using var context = _contextFactory();
        return await context.Budgets
            .Include(b => b.Category)
            .Where(b => b.MonthYear == monthYear)
            .ToListAsync();
    }

    public async Task<List<Budget>> GetCurrentMonthBudgetsAsync()
    {
        return await GetBudgetsForMonthAsync(GetCurrentMonthYear());
    }

    public async Task<Budget> CreateOrUpdateBudgetAsync(int categoryId, string monthYear, decimal amount)
    {
        using var context = _contextFactory();
        
        var existingBudget = await context.Budgets
            .FirstOrDefaultAsync(b => b.CategoryId == categoryId && b.MonthYear == monthYear);

        if (existingBudget != null)
        {
            existingBudget.Amount = amount;
            existingBudget.Spent = await CalculateSpentForCategoryAsync(categoryId, monthYear);
            await context.SaveChangesAsync();
            return existingBudget;
        }
        else
        {
            var budget = new Budget
            {
                CategoryId = categoryId,
                MonthYear = monthYear,
                Amount = amount,
                Spent = await CalculateSpentForCategoryAsync(categoryId, monthYear)
            };
            context.Budgets.Add(budget);
            await context.SaveChangesAsync();
            return budget;
        }
    }

    public async Task<decimal> CalculateSpentForCategoryAsync(int categoryId, string monthYear)
    {
        using var context = _contextFactory();
        
        var startDate = DateTime.ParseExact(monthYear + "-01", "yyyy-MM-dd", null);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var transactions = await context.Transactions
            .Where(t => t.CategoryId == categoryId 
                && t.Type == TransactionType.Expense 
                && t.Date >= startDate 
                && t.Date <= endDate)
            .ToListAsync();

        return transactions.Sum(t => t.ConvertedAmount);
    }

    public async Task UpdateBudgetSpendingAsync(string monthYear)
    {
        using var context = _contextFactory();
        
        var budgets = await context.Budgets
            .Where(b => b.MonthYear == monthYear)
            .ToListAsync();

        foreach (var budget in budgets)
        {
            budget.Spent = await CalculateSpentForCategoryAsync(budget.CategoryId, monthYear);
        }

        await context.SaveChangesAsync();
    }

    public async Task<Dictionary<int, (decimal budget, decimal spent, decimal remaining)>> GetBudgetSummaryAsync(string monthYear)
    {
        using var context = _contextFactory();
        
        var budgets = await context.Budgets
            .Where(b => b.MonthYear == monthYear)
            .ToListAsync();

        var result = new Dictionary<int, (decimal budget, decimal spent, decimal remaining)>();
        
        foreach (var budget in budgets)
        {
            result[budget.CategoryId] = (budget.Amount, budget.Spent, budget.Amount - budget.Spent);
        }

        return result;
    }

    public async Task<List<Budget>> GetOverBudgetItemsAsync(string monthYear)
    {
        using var context = _contextFactory();
        
        return await context.Budgets
            .Include(b => b.Category)
            .Where(b => b.MonthYear == monthYear && b.Spent > b.Amount)
            .ToListAsync();
    }

    public async Task AutoCreateBudgetsForMonthAsync(string monthYear)
    {
        using var context = _contextFactory();
        
        // Get existing budgets for the month
        var existingBudgets = await context.Budgets
            .Where(b => b.MonthYear == monthYear)
            .Select(b => b.CategoryId)
            .ToListAsync();

        // Get all expense categories
        var categories = await context.Categories.ToListAsync();

        // Create budgets for categories that don't have one
        foreach (var category in categories)
        {
            if (!existingBudgets.Contains(category.Id))
            {
                // Default budget of 0 - user can set their own
                var budget = new Budget
                {
                    CategoryId = category.Id,
                    MonthYear = monthYear,
                    Amount = 0,
                    Spent = 0
                };
                context.Budgets.Add(budget);
            }
        }

        await context.SaveChangesAsync();
    }
}
