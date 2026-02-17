using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;
using MoneyTracker2026.Services;

namespace MoneyTracker2026.Views
{
    public sealed partial class BudgetsPage : Page
    {
        public ObservableCollection<BudgetDisplayItem> Budgets { get; } = new();
        private int _currentMonth;
        private int _currentYear;

        public BudgetsPage()
        {
            this.InitializeComponent();
            _currentMonth = System.DateTime.Now.Month;
            _currentYear = System.DateTime.Now.Year;
            LoadBudgets();
        }

        private void LoadBudgets()
        {
            MonthYearText.Text = new System.DateTime(_currentYear, _currentMonth, 1).ToString("MMMM yyyy");

            var profileName = ProfileManager.CurrentProfile?.Name ?? "Default";
            using var context = new AppDbContext(profileName);

            var budgets = context.Budgets
                .Where(b => b.Month == _currentMonth && b.Year == _currentYear)
                .ToList();

            Budgets.Clear();
            foreach (var budget in budgets)
            {
                var category = context.Categories.Find(budget.CategoryId);
                Budgets.Add(new BudgetDisplayItem
                {
                    CategoryName = category?.Name ?? "Unknown",
                    Spent = budget.Spent,
                    Amount = budget.Amount,
                    PercentUsed = budget.PercentUsed,
                    SpentText = $"£{budget.Spent:N2} spent",
                    AmountText = $"of £{budget.Amount:N2}"
                });
            }

            BudgetsGrid.ItemsSource = Budgets;
        }

        private void PreviousMonth_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _currentMonth--;
            if (_currentMonth < 1)
            {
                _currentMonth = 12;
                _currentYear--;
            }
            LoadBudgets();
        }

        private void NextMonth_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _currentMonth++;
            if (_currentMonth > 12)
            {
                _currentMonth = 1;
                _currentYear++;
            }
            LoadBudgets();
        }

        private void AddBudget_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Add Budget",
                Content = "Budget dialog coming soon!",
                CloseButtonText = "OK"
            };
            dialog.ShowAsync();
        }
    }

    public class BudgetDisplayItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Spent { get; set; }
        public decimal Amount { get; set; }
        public double PercentUsed { get; set; }
        public string SpentText { get; set; } = string.Empty;
        public string AmountText { get; set; } = string.Empty;
    }
}
