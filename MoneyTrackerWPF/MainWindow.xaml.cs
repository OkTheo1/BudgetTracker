using System.Windows;
using MoneyTracker2026.Views;

namespace MoneyTracker2026;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Title = "MoneyTracker 2026 - £ GBP";
        
        // Navigate to Overview page by default
        MainFrame.Navigate(new OverviewPage());
    }

    private void Overview_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new OverviewPage());
    }

    private void Transactions_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new TransactionsPage());
    }

    private void Budgets_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new BudgetsPage());
    }

    private void Goals_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new GoalsPage());
    }

    private void Debts_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new DebtsPage());
    }

    private void Reports_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new ReportsPage());
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        MainFrame.Navigate(new SettingsPage());
    }
}
