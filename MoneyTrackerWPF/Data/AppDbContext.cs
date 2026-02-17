using Microsoft.EntityFrameworkCore;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Data;

public class AppDbContext : DbContext
{
    public DbSet<Category> Categories { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<RecurringItem> RecurringItems { get; set; }
    public DbSet<SavingsGoal> SavingsGoals { get; set; }
    public DbSet<Debt> Debts { get; set; }
    public DbSet<NetWorthEntry> NetWorthEntries { get; set; }
    public DbSet<Currency> Currencies { get; set; }

    private readonly string _dbPath;

    public AppDbContext(string dbPath)
    {
        _dbPath = dbPath;
    }

    public AppDbContext() : this(GetDefaultDbPath())
    {
    }

    private static string GetDefaultDbPath()
    {
        var appDataPath = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MoneyTracker2026");
        
        System.IO.Directory.CreateDirectory(appDataPath);
        return System.IO.Path.Combine(appDataPath, "MoneyTracker.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure indexes
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.Date);
        
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.CategoryId);
        
        modelBuilder.Entity<Transaction>()
            .HasIndex(t => t.CurrencyCode);
        
        modelBuilder.Entity<Budget>()
            .HasIndex(b => b.MonthYear);
        
        modelBuilder.Entity<NetWorthEntry>()
            .HasIndex(n => n.Date);

        // Seed default currencies
        modelBuilder.Entity<Currency>().HasData(
            new Currency { Code = "GBP", Name = "British Pound", Symbol = "£", ExchangeRateToBase = 1.0m, IsBase = true, LastUpdated = DateTime.Now },
            new Currency { Code = "USD", Name = "US Dollar", Symbol = "$", ExchangeRateToBase = 1.3m, IsBase = false, LastUpdated = DateTime.Now },
            new Currency { Code = "EUR", Name = "Euro", Symbol = "€", ExchangeRateToBase = 1.18m, IsBase = false, LastUpdated = DateTime.Now }
        );

        // Seed default categories
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Salary", IconGlyph = "💰", ColorHex = "#5BD787", IsSystem = true },
            new Category { Id = 2, Name = "Food & Dining", IconGlyph = "🍔", ColorHex = "#FF6B6B", IsSystem = true },
            new Category { Id = 3, Name = "Transportation", IconGlyph = "🚗", ColorHex = "#4ECDC4", IsSystem = true },
            new Category { Id = 4, Name = "Shopping", IconGlyph = "🛍️", ColorHex = "#FFE66D", IsSystem = true },
            new Category { Id = 5, Name = "Entertainment", IconGlyph = "🎬", ColorHex = "#95E1D3", IsSystem = true },
            new Category { Id = 6, Name = "Bills & Utilities", IconGlyph = "📄", ColorHex = "#F38181", IsSystem = true },
            new Category { Id = 7, Name = "Health", IconGlyph = "🏥", ColorHex = "#AA96DA", IsSystem = true },
            new Category { Id = 8, Name = "Travel", IconGlyph = "✈️", ColorHex = "#FCBAD3", IsSystem = true },
            new Category { Id = 9, Name = "Education", IconGlyph = "📚", ColorHex = "#A8D8EA", IsSystem = true },
            new Category { Id = 10, Name = "Other Income", IconGlyph = "💵", ColorHex = "#00C4B4", IsSystem = true },
            new Category { Id = 11, Name = "Other Expense", IconGlyph = "📝", ColorHex = "#9B59B6", IsSystem = true }
        );
    }
}
