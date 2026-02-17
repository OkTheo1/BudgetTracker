using System.IO;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Services;

public class AccountService
{
    private readonly Func<AppDbContext> _contextFactory;

    public AccountService(Func<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Account>> GetAllAccountsAsync()
    {
        using var context = _contextFactory();
        return await Task.FromResult(context.Accounts.OrderBy(a => a.Name).ToList());
    }

    public async Task<Account?> GetAccountByIdAsync(int id)
    {
        using var context = _contextFactory();
        return await Task.FromResult(context.Accounts.Find(id));
    }

    public async Task<decimal> GetTotalBalanceAsync()
    {
        using var context = _contextFactory();
        return await Task.FromResult(context.Accounts.Sum(a => a.Balance));
    }

    public async Task<Account> CreateAccountAsync(Account account)
    {
        using var context = _contextFactory();
        context.Accounts.Add(account);
        await context.SaveChangesAsync();
        return account;
    }

    public async Task<Account> UpdateAccountAsync(Account account)
    {
        using var context = _contextFactory();
        context.Accounts.Update(account);
        await context.SaveChangesAsync();
        return account;
    }

    public async Task DeleteAccountAsync(int accountId)
    {
        using var context = _contextFactory();
        var account = await context.Accounts.FindAsync(accountId);
        if (account != null)
        {
            context.Accounts.Remove(account);
            await context.SaveChangesAsync();
        }
    }

    public async Task UpdateBalanceAsync(int accountId, decimal newBalance)
    {
        using var context = _contextFactory();
        var account = await context.Accounts.FindAsync(accountId);
        if (account != null)
        {
            account.Balance = newBalance;
            await context.SaveChangesAsync();
        }
    }

    public async Task AddToBalanceAsync(int accountId, decimal amount)
    {
        using var context = _contextFactory();
        var account = await context.Accounts.FindAsync(accountId);
        if (account != null)
        {
            account.Balance += amount;
            await context.SaveChangesAsync();
        }
    }

    public async Task SeedDefaultAccountsAsync()
    {
        using var context = _contextFactory();
        
        if (context.Accounts.Any())
            return;

        var accounts = new List<Account>
        {
            new() { Name = "Cash", Balance = 0, Type = "Cash", CurrencyCode = "GBP" },
            new() { Name = "Bank Account", Balance = 0, Type = "Bank", CurrencyCode = "GBP" },
            new() { Name = "Credit Card", Balance = 0, Type = "CreditCard", CurrencyCode = "GBP" },
            new() { Name = "Savings", Balance = 0, Type = "Savings", CurrencyCode = "GBP" }
        };

        context.Accounts.AddRange(accounts);
        await context.SaveChangesAsync();
    }

    public async Task<Dictionary<string, decimal>> GetAccountBalancesByTypeAsync()
    {
        using var context = _contextFactory();
        var accounts = context.Accounts.ToList();
        
        return accounts
            .GroupBy(a => a.Type)
            .ToDictionary(g => g.Key, g => g.Sum(a => a.Balance));
    }
}
