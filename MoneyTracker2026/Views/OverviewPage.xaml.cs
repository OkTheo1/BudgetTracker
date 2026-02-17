using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;
using MoneyTracker2026.Services;

namespace MoneyTracker2026.Views
{
    public sealed partial class OverviewPage : Page
    {
        public ObservableCollection<TransactionDisplayItem> RecentTransactions { get; } = new();

        public OverviewPage()
        {
            this.InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            var profileName = ProfileManager.CurrentProfile?.Name ?? "Default";
            using var context = new AppDbContext(profileName);

            // Load total balance
            var accounts = context.Accounts.ToList();
            var totalBalance = accounts.Sum(a => a.Balance);
            TotalBalanceText.Text = $"£{totalBalance:N2}";

            // Load monthly income/expenses
            var startOfMonth = new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            var monthlyTransactions = context.Transactions
                .Where(t => t.Date >= startOfMonth && t.Date <= endOfMonth)
                .ToList();

            var monthlyIncome = monthlyTransactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.ConvertedAmount);
            
            var monthlyExpenses = monthlyTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.ConvertedAmount);

            MonthlyIncomeText.Text = $"£{monthlyIncome:N2}";
            MonthlyExpensesText.Text = $"£{monthlyExpenses:N2}";

            // Load net worth
            var latestNetWorth = context.NetWorthEntries
                .OrderByDescending(n => n.Date)
                .FirstOrDefault();

            if (latestNetWorth != null)
            {
                NetWorthText.Text = $"£{latestNetWorth.NetWorth:N2}";
            }
            else
            {
                var totalAssets = accounts.Sum(a => a.Balance);
                var totalLiabilities = context.Debts.Sum(d => d.Balance);
                var netWorth = totalAssets - totalLiabilities;
                NetWorthText.Text = $"£{netWorth:N2}";
            }

            // Load recent transactions
            RecentTransactions.Clear();
            var recentTxns = context.Transactions
                .OrderByDescending(t => t.Date)
                .Take(10)
                .ToList();

            foreach (var txn in recentTxns)
            {
                var category = context.Categories.Find(txn.CategoryId);
                RecentTransactions.Add(new TransactionDisplayItem
                {
                    Icon = category?.IconGlyph ?? "📝",
                    Description = txn.Description,
                    Date = txn.Date.ToString("MMM dd, yyyy"),
                    Amount = txn.Type == TransactionType.Income 
                        ? $"+£{txn.Amount:N2}" 
                        : $"-£{txn.Amount:N2}",
                    IsIncome = txn.Type == TransactionType.Income
                });
            }
        }
    }

    public class TransactionDisplayItem
    {
        public string Icon { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public bool IsIncome { get; set; }
    }
}
