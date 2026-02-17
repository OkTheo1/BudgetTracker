using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;
using MoneyTracker2026.Services;

namespace MoneyTracker2026.Views
{
    public sealed partial class TransactionsPage : Page
    {
        public ObservableCollection<TransactionDisplayItem> Transactions { get; } = new();

        public TransactionsPage()
        {
            this.InitializeComponent();
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            var profileName = ProfileManager.CurrentProfile?.Name ?? "Default";
            using var context = new AppDbContext(profileName);

            var transactions = context.Transactions
                .OrderByDescending(t => t.Date)
                .ToList();

            Transactions.Clear();
            foreach (var txn in transactions)
            {
                var category = context.Categories.Find(txn.CategoryId);
                Transactions.Add(new TransactionDisplayItem
                {
                    Icon = category?.IconGlyph ?? "📝",
                    Description = txn.Description,
                    Category = category?.Name ?? "Uncategorized",
                    Date = txn.Date.ToString("MMM dd, yyyy"),
                    Amount = txn.Type == TransactionType.Income
                        ? $"+£{txn.Amount:N2}"
                        : $"-£{txn.Amount:N2}",
                    IsIncome = txn.Type == TransactionType.Income
                });
            }

            TransactionsList.ItemsSource = Transactions;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Implement search functionality
            var searchText = SearchBox.Text.ToLower();
            if (string.IsNullOrEmpty(searchText))
            {
                LoadTransactions();
                return;
            }

            var filtered = Transactions.Where(t => 
                t.Description.ToLower().Contains(searchText) ||
                t.Category.ToLower().Contains(searchText));
            
            TransactionsList.ItemsSource = filtered;
        }

        private void AddTransaction_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // TODO: Show add transaction dialog
            // For now, show a message
            var dialog = new ContentDialog
            {
                Title = "Add Transaction",
                Content = "Transaction dialog coming soon!",
                CloseButtonText = "OK"
            };
            dialog.ShowAsync();
        }
    }
}
