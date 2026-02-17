using Microsoft.EntityFrameworkCore;
using MoneyTracker2026.Models;
using System;
using System.IO;

namespace MoneyTracker2026.Data
{
    public class AppDbContext : DbContext
    {
        private readonly string _dbPath;

        public AppDbContext(string? profileName = null)
        {
            if (string.IsNullOrEmpty(profileName))
            {
                profileName = "Default";
            }
            
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MoneyTracker2026", "Profiles");
            
            Directory.CreateDirectory(appDataPath);
            _dbPath = Path.Combine(appDataPath, $"{profileName}.db");
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<RecurringItem> RecurringItems { get; set; }
        public DbSet<SavingsGoal> SavingsGoals { get; set; }
        public DbSet<Debt> Debts { get; set; }
        public DbSet<NetWorthEntry> NetWorthEntries { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<Profile> Profiles { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Indexes
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.Date);
            
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.CategoryId);
            
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.CurrencyCode);
            
            modelBuilder.Entity<Budget>()
                .HasIndex(b => new { b.Month, b.Year, b.CategoryId })
                .IsUnique();

            // Seed default currencies (GBP as base)
            modelBuilder.Entity<Currency>().HasData(
                new Currency { Code = "GBP", Name = "British Pound", Symbol = "£", ExchangeRateToBase = 1.0m, IsBase = true },
                new Currency { Code = "USD", Name = "US Dollar", Symbol = "$", ExchangeRateToBase = 1.30m, IsBase = false },
                new Currency { Code = "EUR", Name = "Euro", Symbol = "€", ExchangeRateToBase = 1.18m, IsBase = false },
                new Currency { Code = "JPY", Name = "Japanese Yen", Symbol = "¥", ExchangeRateToBase = 150.0m, IsBase = false },
                new Currency { Code = "AUD", Name = "Australian Dollar", Symbol = "A$", ExchangeRateToBase = 1.95m, IsBase = false },
                new Currency { Code = "CAD", Name = "Canadian Dollar", Symbol = "C$", ExchangeRateToBase = 1.75m, IsBase = false }
            );

            // Seed default categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Salary", IconGlyph = "💰", ColorHex = "#5BD787", IsSystem = true },
                new Category { Id = 2, Name = "Food & Dining", IconGlyph = "🍔", ColorHex = "#FFB74D", IsSystem = true },
                new Category { Id = 3, Name = "Transportation", IconGlyph = "🚗", ColorHex = "#42A5F5", IsSystem = true },
                new Category { Id = 4, Name = "Shopping", IconGlyph = "🛒", ColorHex = "#AB47BC", IsSystem = true },
                new Category { Id = 5, Name = "Entertainment", IconGlyph = "🎬", ColorHex = "#EF5350", IsSystem = true },
                new Category { Id = 6, Name = "Bills & Utilities", IconGlyph = "💡", ColorHex = "#FFA726", IsSystem = true },
                new Category { Id = 7, Name = "Healthcare", IconGlyph = "🏥", ColorHex = "#26A69A", IsSystem = true },
                new Category { Id = 8, Name = "Travel", IconGlyph = "✈️", ColorHex = "#7E57C2", IsSystem = true },
                new Category { Id = 9, Name = "Investments", IconGlyph = "📈", ColorHex = "#66BB6A", IsSystem = true },
                new Category { Id = 10, Name = "Other Income", IconGlyph = "💵", ColorHex = "#29B6F6", IsSystem = true },
                new Category { Id = 11, Name = "Other Expense", IconGlyph = "📝", ColorHex = "#78909C", IsSystem = true }
            );
        }
    }
}
