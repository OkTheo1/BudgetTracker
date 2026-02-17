using System.IO;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Services;

public class CurrencyService
{
    private readonly Func<AppDbContext> _contextFactory;
    private readonly SettingsService _settingsService;

    public CurrencyService(Func<AppDbContext> contextFactory, SettingsService settingsService)
    {
        _contextFactory = contextFactory;
        _settingsService = settingsService;
    }

    public string GetBaseCurrencyCode()
    {
        return _settingsService.Settings.BaseCurrencyCode ?? "GBP";
    }

    public string GetBaseCurrencySymbol()
    {
        using var context = _contextFactory();
        var baseCurrency = GetBaseCurrencyCode();
        var currency = context.Currencies.FirstOrDefault(c => c.Code == baseCurrency);
        return currency?.Symbol ?? "£";
    }

    public async Task<List<Currency>> GetAllCurrenciesAsync()
    {
        using var context = _contextFactory();
        return await Task.FromResult(context.Currencies.OrderBy(c => c.Code).ToList());
    }

    public async Task<Currency?> GetCurrencyAsync(string code)
    {
        using var context = _contextFactory();
        return await Task.FromResult(context.Currencies.FirstOrDefault(c => c.Code == code));
    }

    public async Task<decimal> ConvertToBaseAsync(decimal amount, string fromCurrency)
    {
        if (string.IsNullOrEmpty(fromCurrency))
            return amount;

        var baseCurrency = GetBaseCurrencyCode();
        if (fromCurrency == baseCurrency)
            return amount;

        using var context = _contextFactory();
        var from = await GetCurrencyAsync(fromCurrency);
        var to = await GetCurrencyAsync(baseCurrency);

        if (from == null || to == null || from.ExchangeRateToBase == 0)
            return amount;

        // Convert: amount in fromCurrency -> GBP
        return amount / from.ExchangeRateToBase;
    }

    public async Task<decimal> ConvertFromBaseAsync(decimal amount, string toCurrency)
    {
        if (string.IsNullOrEmpty(toCurrency))
            return amount;

        var baseCurrency = GetBaseCurrencyCode();
        if (toCurrency == baseCurrency)
            return amount;

        using var context = _contextFactory();
        var from = await GetCurrencyAsync(baseCurrency);
        var to = await GetCurrencyAsync(toCurrency);

        if (from == null || to == null || to.ExchangeRateToBase == 0)
            return amount;

        // Convert: amount in GBP -> toCurrency
        return amount * to.ExchangeRateToBase;
    }

    public string DetectCurrencyFromAmount(string amount)
    {
        if (string.IsNullOrWhiteSpace(amount))
            return GetBaseCurrencyCode();

        // Prefix detection (priority): £ → GBP, $ → USD, € → EUR
        if (amount.TrimStart().StartsWith("£"))
            return "GBP";
        if (amount.TrimStart().StartsWith("$"))
            return "USD";
        if (amount.TrimStart().StartsWith("€"))
            return "EUR";
        if (amount.TrimStart().StartsWith("¥"))
            return "JPY";
        if (amount.TrimStart().StartsWith("₹"))
            return "INR";

        // Suffix detection: 50£, 100$, etc.
        if (amount.TrimEnd().EndsWith("£"))
            return "GBP";
        if (amount.TrimEnd().EndsWith("$"))
            return "USD";
        if (amount.TrimEnd().EndsWith("€"))
            return "EUR";
        if (amount.TrimEnd().EndsWith("¥"))
            return "JPY";
        if (amount.TrimEnd().EndsWith("₹"))
            return "INR";

        // Default to base currency
        return GetBaseCurrencyCode();
    }

    public decimal ParseAmount(string amount)
    {
        if (string.IsNullOrWhiteSpace(amount))
            return 0;

        // Remove currency symbols and whitespace
        var cleanAmount = amount.Trim()
            .Replace("£", "")
            .Replace("$", "")
            .Replace("€", "")
            .Replace("¥", "")
            .Replace("₹", "")
            .Replace(",", "")
            .Trim();

        // Handle negative amounts in parentheses: (50) -> -50
        if (cleanAmount.StartsWith("(") && cleanAmount.EndsWith(")"))
        {
            cleanAmount = "-" + cleanAmount.Trim('(', ')');
        }

        if (decimal.TryParse(cleanAmount, out var result))
            return result;

        return 0;
    }

    public async Task SeedDefaultCurrenciesAsync()
    {
        using var context = _contextFactory();
        
        if (context.Currencies.Any())
            return;

        var currencies = new List<Currency>
        {
            new() { Code = "GBP", Name = "British Pound", Symbol = "£", ExchangeRateToBase = 1.0m, IsBase = true },
            new() { Code = "USD", Name = "US Dollar", Symbol = "$", ExchangeRateToBase = 1.27m, IsBase = false },
            new() { Code = "EUR", Name = "Euro", Symbol = "€", ExchangeRateToBase = 1.17m, IsBase = false },
            new() { Code = "JPY", Name = "Japanese Yen", Symbol = "¥", ExchangeRateToBase = 189.5m, IsBase = false },
            new() { Code = "INR", Name = "Indian Rupee", Symbol = "₹", ExchangeRateToBase = 106.2m, IsBase = false },
            new() { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", ExchangeRateToBase = 1.95m, IsBase = false },
            new() { Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", ExchangeRateToBase = 1.73m, IsBase = false }
        };

        context.Currencies.AddRange(currencies);
        await context.SaveChangesAsync();
    }

    public string FormatAmount(decimal amount, string currencyCode = "")
    {
        var symbol = string.IsNullOrEmpty(currencyCode) ? GetBaseCurrencySymbol() : GetCurrencySymbol(currencyCode);
        return $"{symbol}{amount:N2}";
    }

    public string GetCurrencySymbol(string currencyCode)
    {
        using var context = _contextFactory();
        var currency = context.Currencies.FirstOrDefault(c => c.Code == currencyCode);
        return currency?.Symbol ?? "£";
    }
}
