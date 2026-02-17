using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;
using MoneyTracker2026.Services;

namespace MoneyTracker2026.Views;

public partial class TransactionsPage : Page
{
    private TransactionService? _transactionService;
    private CategoryService? _categoryService;
    private AccountService? _accountService;
    private SettingsService? _settingsService;
    private List<Transaction> _allTransactions = new();

    public TransactionsPage()
    {
        InitializeComponent();
        InitializeServices();
        LoadTransactionsAsync();
    }

    private void InitializeServices()
    {
        var contextFactory = () => App.DbContext!;
        _settingsService = new SettingsService();
        _transactionService = new TransactionService(contextFactory, _settingsService);
        _categoryService = new CategoryService(contextFactory);
        _accountService = new AccountService(contextFactory);
    }

    private async void LoadTransactionsAsync()
    {
        try
        {
            if (_transactionService == null) return;

            _allTransactions = await _transactionService.GetAllTransactionsAsync();
            DisplayTransactions(_allTransactions);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading transactions: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void DisplayTransactions(List<Transaction> transactions)
    {
        var displayList = transactions.Select(t => new
        {
            t.Id,
            t.Description,
            t.Date,
            FormattedAmount = FormatAmount(t),
            AmountColor = GetAmountColor(t)
        }).ToList();

        TransactionsList.ItemsSource = displayList;
    }

    private string FormatAmount(Transaction t)
    {
        var symbol = GetCurrencySymbol(t.CurrencyCode);
        var prefix = t.Type == TransactionType.Expense ? "-" : "+";
        return $"{prefix}{symbol}{t.Amount:N2}";
    }

    private string GetCurrencySymbol(string currencyCode)
    {
        return currencyCode switch
        {
            "GBP" => "£",
            "USD" => "$",
            "EUR" => "€",
            _ => "£"
        };
    }

    private Brush GetAmountColor(Transaction t)
    {
        return t.Type == TransactionType.Expense
            ? new SolidColorBrush(Color.FromRgb(255, 107, 107)) // #FF6B6B
            : new SolidColorBrush(Color.FromRgb(91, 215, 135)); // #5BD787
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_transactionService == null) return;

        var searchText = SearchBox.Text?.ToLower() ?? "";
        
        if (string.IsNullOrWhiteSpace(searchText))
        {
            DisplayTransactions(_allTransactions);
            return;
        }

        var filtered = _allTransactions.Where(t => 
            t.Description?.ToLower().Contains(searchText) == true ||
            t.Category?.Name?.ToLower().Contains(searchText) == true
        ).ToList();

        DisplayTransactions(filtered);
    }

    private async void AddTransaction_Click(object sender, RoutedEventArgs e)
    {
        // Create a simple input dialog for adding transactions
        var dialog = new Window
        {
            Title = "Add Transaction",
            Width = 400,
            Height = 450,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Owner = Window.GetWindow(this),
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 30))
        };

        var grid = new Grid { Margin = new Thickness(20) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Description
        var descLabel = new TextBlock { Text = "Description:", Foreground = Brushes.White, Margin = new Thickness(0, 0, 0, 5) };
        Grid.SetRow(descLabel, 0);
        grid.Children.Add(descLabel);

        var descBox = new TextBox { Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)), Foreground = Brushes.White, Padding = new Thickness(10), Margin = new Thickness(0, 0, 0, 15) };
        Grid.SetRow(descBox, 1);
        grid.Children.Add(descBox);

        // Amount
        var amountLabel = new TextBlock { Text = "Amount (e.g., £50.00):", Foreground = Brushes.White, Margin = new Thickness(0, 0, 0, 5) };
        Grid.SetRow(amountLabel, 2);
        grid.Children.Add(amountLabel);

        var amountBox = new TextBox { Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)), Foreground = Brushes.White, Padding = new Thickness(10), Margin = new Thickness(0, 0, 0, 15) };
        Grid.SetRow(amountBox, 3);
        grid.Children.Add(amountBox);

        // Type
        var typeLabel = new TextBlock { Text = "Type:", Foreground = Brushes.White, Margin = new Thickness(0, 0, 0, 5) };
        Grid.SetRow(typeLabel, 4);
        grid.Children.Add(typeLabel);

        var typeCombo = new ComboBox { Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)), Foreground = Brushes.White, Padding = new Thickness(10), Margin = new Thickness(0, 0, 0, 15) };
        typeCombo.Items.Add("Expense");
        typeCombo.Items.Add("Income");
        typeCombo.SelectedIndex = 0;
        Grid.SetRow(typeCombo, 5);
        grid.Children.Add(typeCombo);

        // Buttons
        var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 20, 0, 0) };
        Grid.SetRow(buttonPanel, 6);

        var saveBtn = new Button { Content = "Save", Background = new SolidColorBrush(Color.FromRgb(0, 196, 180)), Foreground = Brushes.White, Padding = new Thickness(20, 10, 20, 10), Margin = new Thickness(0, 0, 10, 0), BorderThickness = new Thickness(0) };
        var cancelBtn = new Button { Content = "Cancel", Background = new SolidColorBrush(Color.FromRgb(68, 68, 68)), Foreground = Brushes.White, Padding = new Thickness(20, 10, 20, 10), BorderThickness = new Thickness(0) };

        saveBtn.Click += async (s, args) =>
        {
            if (string.IsNullOrWhiteSpace(descBox.Text))
            {
                MessageBox.Show("Please enter a description.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(amountBox.Text))
            {
                MessageBox.Show("Please enter an amount.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var (amount, currencyCode) = _transactionService!.ParseAmount(amountBox.Text);
                var type = typeCombo.SelectedIndex == 0 ? TransactionType.Expense : TransactionType.Income;

                // Get default account
                var accounts = await _accountService!.GetAllAccountsAsync();
                var account = accounts.FirstOrDefault();
                if (account == null)
                {
                    MessageBox.Show("Please create an account first in Settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get default category
                var categories = await _categoryService!.GetAllCategoriesAsync();
                var category = categories.FirstOrDefault();
                if (category == null)
                {
                    MessageBox.Show("Please create a category first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var transaction = new Transaction
                {
                    Description = descBox.Text,
                    Amount = amount,
                    CurrencyCode = currencyCode,
                    ConvertedAmount = _transactionService.ConvertToBase(amount, currencyCode),
                    Type = type,
                    Date = DateTime.Now,
                    AccountId = account.Id,
                    CategoryId = category.Id
                };

                await _transactionService.AddTransactionAsync(transaction);
                LoadTransactionsAsync();
                dialog.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        };

        cancelBtn.Click += (s, args) => dialog.Close();

        buttonPanel.Children.Add(saveBtn);
        buttonPanel.Children.Add(cancelBtn);
        grid.Children.Add(buttonPanel);

        dialog.Content = grid;
        dialog.ShowDialog();
    }
}
