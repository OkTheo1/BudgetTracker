using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Services;

public class TransactionService
{
    private readonly Func<AppDbContext> _contextFactory;
    private readonly SettingsService _settingsService;

    public TransactionService(Func<AppDbContext> contextFactory, SettingsService settingsService)
    {
        _contextFactory = contextFactory;
        _settingsService = settingsService;
    }

    public (decimal amount, string currencyCode) ParseAmount(string rawAmount)
    {
        if (string.IsNullOrWhiteSpace(rawAmount))
            return (0, _settingsService.Settings.BaseCurrencyCode);

        var original = rawAmount.Trim();
        var detectedCurrency = _settingsService.Settings.BaseCurrencyCode;
        decimal amount = 0;

        // Prefix detection (priority: £ $ €) - standard for UK/US
        if (original.StartsWith("£"))
        {
            detectedCurrency = "GBP";
            original = original.Substring(1).Trim();
        }
        else if (original.StartsWith("$"))
        {
            detectedCurrency = "USD";
            original = original.Substring(1).Trim();
        }
        else if (original.StartsWith("€"))
        {
            detectedCurrency = "EUR";
            original = original.Substring(1).Trim();
        }
        // Suffix detection (fallback)
        else if (original.EndsWith("£"))
        {
            detectedCurrency = "GBP";
            original = original.Substring(0, original.Length - 1).Trim();
        }
        else if (original.EndsWith("$"))
        {
            detectedCurrency = "USD";
            original = original.Substring(0, original.Length - 1).Trim();
        }
        else if (original.EndsWith("€"))
        {
            detectedCurrency = "EUR";
            original = original.Substring(0, original.Length - 1).Trim();
        }

        // Clean up the amount string - handle negative amounts in parentheses
        // e.g., (50.00) means -50.00
        int startParenIndex = original.IndexOf('(');
        int endParenIndex = original.LastIndexOf(')');
        
        if (startParenIndex >= 0 && endParenIndex > startParenIndex)
        {
            original = "-" + original.Substring(startParenIndex + 1, endParenIndex - startParenIndex - 1);
        }

        // Remove thousand separators and other currency symbols
        original = original.Replace(",", "").Replace(" ", "");

        // Try to parse the amount
        if (decimal.TryParse(original, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedAmount))
        {
            amount = Math.Abs(parsedAmount);
        }

        return (amount, detectedCurrency);
    }

    public decimal GetExchangeRate(string currencyCode)
    {
        if (currencyCode == _settingsService.Settings.BaseCurrencyCode)
            return 1.0m;

        using var context = _contextFactory();
        var currency = context.Currencies.Find(currencyCode);
        return currency?.ExchangeRateToBase ?? 1.0m;
    }

    public decimal ConvertToBase(decimal amount, string currencyCode)
    {
        var rate = GetExchangeRate(currencyCode);
        return amount / rate;
    }

    public async Task<List<Transaction>> GetAllTransactionsAsync()
    {
        using var context = _contextFactory();
        return await context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime start, DateTime end)
    {
        using var context = _contextFactory();
        return await context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Where(t => t.Date >= start && t.Date <= end)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByCategoryAsync(int categoryId)
    {
        using var context = _contextFactory();
        return await context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .Where(t => t.CategoryId == categoryId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task<Transaction> AddTransactionAsync(Transaction transaction)
    {
        using var context = _contextFactory();
        
        // Parse amount and detect currency
        var (amount, currencyCode) = ParseAmount(transaction.Amount.ToString());
        transaction.Amount = amount;
        transaction.CurrencyCode = currencyCode;
        
        // Convert to base currency
        transaction.ConvertedAmount = ConvertToBase(transaction.Amount, currencyCode);
        
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();
        
        // Update account balance
        await UpdateAccountBalanceAsync(transaction.AccountId);
        
        return transaction;
    }

    public async Task<Transaction> UpdateTransactionAsync(Transaction transaction)
    {
        using var context = _contextFactory();
        
        // Parse amount and detect currency
        var (amount, currencyCode) = ParseAmount(transaction.Amount.ToString());
        transaction.Amount = amount;
        transaction.CurrencyCode = currencyCode;
        
        // Convert to base currency
        transaction.ConvertedAmount = ConvertToBase(transaction.Amount, currencyCode);
        
        context.Transactions.Update(transaction);
        await context.SaveChangesAsync();
        
        // Update account balance
        await UpdateAccountBalanceAsync(transaction.AccountId);
        
        return transaction;
    }

    public async Task DeleteTransactionAsync(int transactionId)
    {
        using var context = _contextFactory();
        var transaction = await context.Transactions.FindAsync(transactionId);
        if (transaction != null)
        {
            var accountId = transaction.AccountId;
            context.Transactions.Remove(transaction);
            await context.SaveChangesAsync();
            
            // Update account balance
            await UpdateAccountBalanceAsync(accountId);
        }
    }

    private async Task UpdateAccountBalanceAsync(int accountId)
    {
        using var context = _contextFactory();
        var account = await context.Accounts.FindAsync(accountId);
        if (account == null) return;

        var transactions = await context.Transactions
            .Where(t => t.AccountId == accountId)
            .ToListAsync();

        decimal balance = 0;
        foreach (var t in transactions)
        {
            if (t.Type == TransactionType.Income)
                balance += t.ConvertedAmount;
            else
                balance -= t.ConvertedAmount;
        }

        account.Balance = balance;
        await context.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalIncomeAsync(DateTime? start = null, DateTime? end = null)
    {
        using var context = _contextFactory();
        var query = context.Transactions.Where(t => t.Type == TransactionType.Income);

        if (start.HasValue)
            query = query.Where(t => t.Date >= start.Value);
        if (end.HasValue)
            query = query.Where(t => t.Date <= end.Value);

        return await query.SumAsync(t => t.ConvertedAmount);
    }

    public async Task<decimal> GetTotalExpenseAsync(DateTime? start = null, DateTime? end = null)
    {
        using var context = _contextFactory();
        var query = context.Transactions.Where(t => t.Type == TransactionType.Expense);

        if (start.HasValue)
            query = query.Where(t => t.Date >= start.Value);
        if (end.HasValue)
            query = query.Where(t => t.Date <= end.Value);

        return await query.SumAsync(t => t.ConvertedAmount);
    }

    public async Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(DateTime? start = null, DateTime? end = null)
    {
        using var context = _contextFactory();
        var query = context.Transactions
            .Include(t => t.Category)
            .Where(t => t.Type == TransactionType.Expense);

        if (start.HasValue)
            query = query.Where(t => t.Date >= start.Value);
        if (end.HasValue)
            query = query.Where(t => t.Date <= end.Value);

        var result = await query
            .GroupBy(t => t.Category != null ? t.Category.Name : "Unknown")
            .Select(g => new { Category = g.Key, Total = g.Sum(t => t.ConvertedAmount) })
            .ToListAsync();

        return result.ToDictionary(x => x.Category, x => x.Total);
    }
}
