using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;
using MoneyTracker2026.Services;

namespace MoneyTracker2026.Views
{
    public sealed partial class ReportsPage : Page
    {
        public ObservableCollection<CategoryBreakdownItem> CategoryBreakdown { get; } = new();

        public ReportsPage()
        {
            this.InitializeComponent();
            
            // Set default date range (current month)
            StartDatePicker.Date = new System.DateTimeOffset(new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1));
            EndDatePicker.Date = new System.DateTimeOffset(System.DateTime.Now);
            
            LoadReports();
        }

        private void LoadReports()
        {
            var profileName = ProfileManager.CurrentProfile?.Name ?? "Default";
            using var context = new AppDbContext(profileName);

            var startDate = StartDatePicker.Date?.DateTime ?? new System.DateTime(System.DateTime.Now.Year, System.DateTime.Now.Month, 1);
            var endDate = EndDatePicker.Date?.DateTime ?? System.DateTime.Now;

            var transactions = context.Transactions
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .ToList();

            // Income vs Expenses
            var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.ConvertedAmount);
            var expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.ConvertedAmount);
            IncomeExpensesSummary.Text = $"Income: £{income:N2} | Expenses: £{expenses:N2}";

            // Category breakdown
            var categoryGroups = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.CategoryId)
                .Select(g => new
                {
                    CategoryId = g.Key,
                    Total = g.Sum(t => t.ConvertedAmount)
                })
                .OrderByDescending(c => c.Total)
                .ToList();

            CategoryBreakdown.Clear();
            foreach (var cat in categoryGroups)
            {
                var category = context.Categories.Find(cat.CategoryId);
                CategoryBreakdown.Add(new CategoryBreakdownItem
                {
                    Icon = category?.IconGlyph ?? "📝",
                    CategoryName = category?.Name ?? "Unknown",
                    Amount = $"£{cat.Total:N2}"
                });
            }
            CategoryBreakdownList.ItemsSource = CategoryBreakdown;

            // Monthly trend
            var monthlyGroups = transactions
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .ToList();

            var trendText = string.Join(" → ", monthlyGroups.Select(g =>
            {
                var incomeSum = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.ConvertedAmount);
                var expenseSum = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.ConvertedAmount);
                return $"{(MonthName)g.Key.Month}: +£{incomeSum:N2}/-£{expenseSum:N2}";
            }));
            MonthlyTrendText.Text = trendText;
        }

        private void ApplyDateRange_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            LoadReports();
        }

        private void Import_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Import CSV",
                Content = "Import wizard coming soon!",
                CloseButtonText = "OK"
            };
            dialog.ShowAsync();
        }

        private void Export_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Export Data",
                Content = "Export options coming soon!",
                CloseButtonText = "OK"
            };
            dialog.ShowAsync();
        }

        private enum MonthName
        {
            Jan = 1, Feb = 2, Mar = 3, Apr = 4, May = 5, Jun = 6,
            Jul = 7, Aug = 8, Sep = 9, Oct = 10, Nov = 11, Dec = 12
        }
    }

    public class CategoryBreakdownItem
    {
        public string Icon { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
    }
}
