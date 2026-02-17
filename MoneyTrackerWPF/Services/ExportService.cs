using System.Globalization;
using System.IO;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Services;

public class ExportService
{
    private readonly Func<AppDbContext> _contextFactory;
    private readonly TransactionService _transactionService;

    public ExportService(Func<AppDbContext> contextFactory, TransactionService transactionService)
    {
        _contextFactory = contextFactory;
        _transactionService = transactionService;
    }

    public async Task ExportTransactionsToCsvAsync(string filePath, DateTime? startDate = null, DateTime? endDate = null)
    {
        using var context = _contextFactory();
        
        var query = context.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        var transactions = await query
            .OrderByDescending(t => t.Date)
            .ToListAsync();

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        // Write header
        csv.WriteField("Date");
        csv.WriteField("Amount");
        csv.WriteField("Currency");
        csv.WriteField("Amount (GBP)");
        csv.WriteField("Description");
        csv.WriteField("Category");
        csv.WriteField("Account");
        csv.WriteField("Type");
        await csv.NextRecordAsync();

        // Write data
        foreach (var t in transactions)
        {
            csv.WriteField(t.Date.ToString("yyyy-MM-dd"));
            csv.WriteField(t.Amount.ToString("F2"));
            csv.WriteField(t.CurrencyCode);
            csv.WriteField(t.ConvertedAmount.ToString("F2"));
            csv.WriteField(t.Description);
            csv.WriteField(t.Category?.Name ?? "");
            csv.WriteField(t.Account?.Name ?? "");
            csv.WriteField(t.Type.ToString());
            await csv.NextRecordAsync();
        }
    }

    public async Task ExportBudgetsToCsvAsync(string filePath, string monthYear)
    {
        using var context = _contextFactory();
        
        var budgets = await context.Budgets
            .Include(b => b.Category)
            .Where(b => b.MonthYear == monthYear)
            .ToListAsync();

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteField("Category");
        csv.WriteField("Budget Amount");
        csv.WriteField("Spent");
        csv.WriteField("Remaining");
        csv.WriteField("Percent Used");
        await csv.NextRecordAsync();

        foreach (var b in budgets)
        {
            var remaining = b.Amount - b.Spent;
            var percentUsed = b.Amount > 0 ? (b.Spent / b.Amount * 100) : 0;

            csv.WriteField(b.Category?.Name ?? "Unknown");
            csv.WriteField(b.Amount.ToString("F2"));
            csv.WriteField(b.Spent.ToString("F2"));
            csv.WriteField(remaining.ToString("F2"));
            csv.WriteField(percentUsed.ToString("F1") + "%");
            await csv.NextRecordAsync();
        }
    }

    public async Task ExportDebtsToCsvAsync(string filePath)
    {
        using var context = _contextFactory();
        
        var debts = await context.Debts.ToListAsync();

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteField("Name");
        csv.WriteField("Balance");
        csv.WriteField("Interest Rate (%)");
        csv.WriteField("Min Payment");
        csv.WriteField("Payoff Method");
        await csv.NextRecordAsync();

        foreach (var d in debts)
        {
            csv.WriteField(d.Name);
            csv.WriteField(d.Balance.ToString("F2"));
            csv.WriteField(d.InterestRatePercent.ToString("F2"));
            csv.WriteField(d.MinPayment.ToString("F2"));
            csv.WriteField(d.PayoffMethod.ToString());
            await csv.NextRecordAsync();
        }
    }

    public async Task ExportToJsonAsync(string filePath)
    {
        using var context = _contextFactory();
        
        var data = new
        {
            ExportDate = DateTime.Now,
            Transactions = await context.Transactions
                .Include(t => t.Category)
                .Include(t => t.Account)
                .OrderByDescending(t => t.Date)
                .Take(1000)
                .ToListAsync(),
            Accounts = await context.Accounts.ToListAsync(),
            Categories = await context.Categories.ToListAsync(),
            Budgets = await context.Budgets.ToListAsync(),
            SavingsGoals = await context.SavingsGoals.ToListAsync(),
            Debts = await context.Debts.ToListAsync()
        };

        var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await File.WriteAllTextAsync(filePath, json);
    }
}
