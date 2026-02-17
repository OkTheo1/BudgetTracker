using Microsoft.EntityFrameworkCore;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Services;

public class RecurringService
{
    private readonly Func<AppDbContext> _contextFactory;
    private readonly TransactionService _transactionService;

    public RecurringService(Func<AppDbContext> contextFactory, TransactionService transactionService)
    {
        _contextFactory = contextFactory;
        _transactionService = transactionService;
    }

    public async Task<List<RecurringItem>> GetAllRecurringItemsAsync()
    {
        using var context = _contextFactory();
        return await context.RecurringItems
            .Include(r => r.Category)
            .Include(r => r.Account)
            .OrderBy(r => r.NextDate)
            .ToListAsync();
    }

    public async Task<List<RecurringItem>> GetActiveRecurringItemsAsync()
    {
        using var context = _contextFactory();
        var today = DateTime.Today;
        return await context.RecurringItems
            .Where(r => r.NextDate <= today.AddDays(30) && r.IsActive)
            .Include(r => r.Category)
            .Include(r => r.Account)
            .OrderBy(r => r.NextDate)
            .ToListAsync();
    }

    public async Task<RecurringItem> CreateRecurringItemAsync(RecurringItem item)
    {
        using var context = _contextFactory();
        context.RecurringItems.Add(item);
        await context.SaveChangesAsync();
        return item;
    }

    public async Task<RecurringItem> UpdateRecurringItemAsync(RecurringItem item)
    {
        using var context = _contextFactory();
        context.RecurringItems.Update(item);
        await context.SaveChangesAsync();
        return item;
    }

    public async Task DeleteRecurringItemAsync(int itemId)
    {
        using var context = _contextFactory();
        var item = await context.RecurringItems.FindAsync(itemId);
        if (item != null)
        {
            context.RecurringItems.Remove(item);
            await context.SaveChangesAsync();
        }
    }

    public async Task<int> ApplyRecurringTransactionsAsync()
    {
        int appliedCount = 0;
        var today = DateTime.Today;

        using var context = _contextFactory();
        var dueItems = await context.RecurringItems
            .Where(r => r.NextDate <= today && r.IsActive)
            .ToListAsync();

        foreach (var item in dueItems)
        {
            // Create a transaction for this recurring item
            var transaction = new Transaction
            {
                Date = item.NextDate,
                Amount = item.Amount,
                Description = $"Recurring payment",
                CategoryId = item.CategoryId,
                AccountId = item.AccountId,
                Type = item.Type,
                CurrencyCode = item.CurrencyCode,
                ConvertedAmount = _transactionService.ConvertToBase(item.Amount, item.CurrencyCode)
            };

            context.Transactions.Add(transaction);
            appliedCount++;

            // Update next date based on frequency
            item.NextDate = CalculateNextDate(item.NextDate, item.Frequency);
        }

        await context.SaveChangesAsync();
        return appliedCount;
    }

    private DateTime CalculateNextDate(DateTime currentDate, Models.Frequency frequency)
    {
        return frequency switch
        {
            Models.Frequency.Daily => currentDate.AddDays(1),
            Models.Frequency.Weekly => currentDate.AddDays(7),
            Models.Frequency.Monthly => currentDate.AddMonths(1),
            Models.Frequency.Yearly => currentDate.AddYears(1),
            _ => currentDate.AddMonths(1)
        };
    }

    public async Task<decimal> GetTotalRecurringIncomeAsync()
    {
        using var context = _contextFactory();
        return await context.RecurringItems
            .Where(r => r.Type == TransactionType.Income && r.IsActive)
            .SumAsync(r => r.Amount);
    }

    public async Task<decimal> GetTotalRecurringExpensesAsync()
    {
        using var context = _contextFactory();
        return await context.RecurringItems
            .Where(r => r.Type == TransactionType.Expense && r.IsActive)
            .SumAsync(r => r.Amount);
    }

    public async Task<Dictionary<string, decimal>> GetRecurringSummaryAsync()
    {
        var totalIncome = await GetTotalRecurringIncomeAsync();
        var totalExpense = await GetTotalRecurringExpensesAsync();

        return new Dictionary<string, decimal>
        {
            { "TotalIncome", totalIncome },
            { "TotalExpenses", totalExpense },
            { "NetRecurring", totalIncome - totalExpense }
        };
    }
}
