using Microsoft.EntityFrameworkCore;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Services;

public class NetWorthService
{
    private readonly Func<AppDbContext> _contextFactory;
    private readonly TransactionService _transactionService;

    public NetWorthService(Func<AppDbContext> contextFactory, TransactionService transactionService)
    {
        _contextFactory = contextFactory;
        _transactionService = transactionService;
    }

    public async Task<List<NetWorthEntry>> GetAllEntriesAsync()
    {
        using var context = _contextFactory();
        return await context.NetWorthEntries
            .OrderByDescending(n => n.Date)
            .ToListAsync();
    }

    public async Task<List<NetWorthEntry>> GetEntriesForDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        using var context = _contextFactory();
        return await context.NetWorthEntries
            .Where(n => n.Date >= startDate && n.Date <= endDate)
            .OrderBy(n => n.Date)
            .ToListAsync();
    }

    public async Task<NetWorthEntry> GetLatestEntryAsync()
    {
        using var context = _contextFactory();
        return await context.NetWorthEntries
            .OrderByDescending(n => n.Date)
            .FirstOrDefaultAsync() ?? new NetWorthEntry();
    }

    public async Task<NetWorthEntry> CalculateCurrentNetWorthAsync()
    {
        using var context = _contextFactory();
        
        // Get total assets (positive account balances)
        var totalAssets = await context.Accounts
            .Where(a => a.Balance > 0)
            .SumAsync(a => a.Balance);

        // Get total liabilities (debts)
        var totalLiabilities = await context.Debts
            .SumAsync(d => d.Balance);

        // Add savings goals as assets
        var savingsGoals = await context.SavingsGoals.SumAsync(g => g.CurrentAmount);

        // Calculate total assets including savings goals
        totalAssets += savingsGoals;

        var entry = new NetWorthEntry
        {
            Date = DateTime.Today,
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities
        };

        return entry;
    }

    public async Task<NetWorthEntry> SaveCurrentNetWorthAsync()
    {
        var current = await CalculateCurrentNetWorthAsync();
        
        using var context = _contextFactory();
        
        // Check if we already have an entry for today
        var existingEntry = await context.NetWorthEntries
            .FirstOrDefaultAsync(n => n.Date.Date == DateTime.Today);

        if (existingEntry != null)
        {
            existingEntry.TotalAssets = current.TotalAssets;
            existingEntry.TotalLiabilities = current.TotalLiabilities;
            context.NetWorthEntries.Update(existingEntry);
        }
        else
        {
            context.NetWorthEntries.Add(current);
        }

        await context.SaveChangesAsync();
        return current;
    }

    public async Task<decimal> GetNetWorthChangeAsync(int days = 30)
    {
        var startDate = DateTime.Today.AddDays(-days);
        
        using var context = _contextFactory();
        
        var current = await context.NetWorthEntries
            .OrderByDescending(n => n.Date)
            .FirstOrDefaultAsync();

        var previous = await context.NetWorthEntries
            .Where(n => n.Date < startDate)
            .OrderByDescending(n => n.Date)
            .FirstOrDefaultAsync();

        if (current == null || previous == null)
            return 0;

        return (current.TotalAssets - current.TotalLiabilities) - 
               (previous.TotalAssets - previous.TotalLiabilities);
    }

    public async Task<Dictionary<string, decimal>> GetNetWorthSummaryAsync()
    {
        var current = await CalculateCurrentNetWorthAsync();
        var change30Days = await GetNetWorthChangeAsync(30);
        var change90Days = await GetNetWorthChangeAsync(90);
        var change1Year = await GetNetWorthChangeAsync(365);

        var netWorth = current.TotalAssets - current.TotalLiabilities;

        return new Dictionary<string, decimal>
        {
            { "TotalAssets", current.TotalAssets },
            { "TotalLiabilities", current.TotalLiabilities },
            { "NetWorth", netWorth },
            { "Change30Days", change30Days },
            { "Change90Days", change90Days },
            { "Change1Year", change1Year }
        };
    }

    public async Task<List<(DateTime Date, decimal NetWorth)>> GetNetWorthTrendAsync(int months = 12)
    {
        var startDate = DateTime.Today.AddMonths(-months);
        
        using var context = _contextFactory();
        
        var entries = await context.NetWorthEntries
            .Where(n => n.Date >= startDate)
            .OrderBy(n => n.Date)
            .ToListAsync();

        return entries.Select(e => (e.Date, e.TotalAssets - e.TotalLiabilities)).ToList();
    }
}
