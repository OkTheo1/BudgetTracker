using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;
using MoneyTracker2026.Services;

namespace MoneyTracker2026.Views
{
    public sealed partial class DebtsPage : Page
    {
        public ObservableCollection<DebtDisplayItem> Debts { get; } = new();
        private PayoffMethod _currentMethod = PayoffMethod.Snowball;

        public DebtsPage()
        {
            this.InitializeComponent();
            LoadDebts();
        }

        private void LoadDebts()
        {
            var profileName = ProfileManager.CurrentProfile?.Name ?? "Default";
            using var context = new AppDbContext(profileName);

            var debts = context.Debts.Where(d => !d.IsPaidOff).ToList();

            // Sort by method
            if (_currentMethod == PayoffMethod.Snowball)
            {
                debts = debts.OrderBy(d => d.Balance).ToList();
            }
            else
            {
                debts = debts.OrderByDescending(d => d.InterestRatePercent).ToList();
            }

            // Calculate totals
            var totalDebt = debts.Sum(d => d.Balance);
            var totalMinPayment = debts.Sum(d => d.MinPayment);

            TotalDebtText.Text = $"£{totalDebt:N2}";
            MonthlyPaymentText.Text = $"£{totalMinPayment:N2}";

            // Estimate payoff date
            if (totalDebt > 0 && totalMinPayment > 0)
            {
                var monthsToPayoff = (int)(totalDebt / totalMinPayment);
                var payoffDate = System.DateTime.Now.AddMonths(monthsToPayoff);
                DebtFreeText.Text = payoffDate.ToString("MMM yyyy");
            }
            else
            {
                DebtFreeText.Text = "-";
            }

            // Load debts list
            Debts.Clear();
            foreach (var debt in debts)
            {
                var monthsToPayoff = debt.MinPayment > 0 
                    ? (int)(debt.Balance / debt.MinPayment) 
                    : 0;
                var payoffDate = System.DateTime.Now.AddMonths(monthsToPayoff);

                Debts.Add(new DebtDisplayItem
                {
                    Name = debt.Name,
                    Balance = debt.Balance,
                    BalanceText = $"£{debt.Balance:N2}",
                    InterestRatePercent = debt.InterestRatePercent,
                    InterestRateText = $"{debt.InterestRatePercent}% APR",
                    MinPayment = debt.MinPayment,
                    MinPaymentText = $"£{debt.MinPayment:N2}/mo",
                    PayoffDate = payoffDate,
                    PayoffDateText = payoffDate.ToString("MMM yyyy")
                });
            }

            DebtsList.ItemsSource = Debts;
        }

        private void Method_Changed(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _currentMethod = AvalancheMethod.IsChecked == true 
                ? PayoffMethod.Avalanche 
                : PayoffMethod.Snowball;
            LoadDebts();
        }

        private void AddDebt_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Add Debt",
                Content = "Debt dialog coming soon!",
                CloseButtonText = "OK"
            };
            dialog.ShowAsync();
        }
    }

    public class DebtDisplayItem
    {
        public string Name { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public string BalanceText { get; set; } = string.Empty;
        public decimal InterestRatePercent { get; set; }
        public string InterestRateText { get; set; } = string.Empty;
        public decimal MinPayment { get; set; }
        public string MinPaymentText { get; set; } = string.Empty;
        public System.DateTime PayoffDate { get; set; }
        public string PayoffDateText { get; set; } = string.Empty;
    }
}
