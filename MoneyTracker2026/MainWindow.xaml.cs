using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using MoneyTracker2026.Views;

namespace MoneyTracker2026
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // Set up acrylic/mica backdrop for premium 2026 feel
            this.SystemBackdrop = new Microsoft.UI.Xaml.Media.MicaBackdrop { Kind = Microsoft.UI.Xaml.Media.MicaKind.BaseAlt };

            // Navigate to Overview page on startup
            ContentFrame.Navigate(typeof(OverviewPage));
            
            // Select the first item
            if (NavigationView.MenuItems.Count > 0)
            {
                NavigationView.SelectedItem = NavigationView.MenuItems[0];
            }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item)
            {
                string tag = item.Tag?.ToString();
                
                Type pageType = tag switch
                {
                    "Overview" => typeof(OverviewPage),
                    "Transactions" => typeof(TransactionsPage),
                    "Budgets" => typeof(BudgetsPage),
                    "Goals" => typeof(GoalsPage),
                    "Debts" => typeof(DebtsPage),
                    "Reports" => typeof(ReportsPage),
                    "Settings" => typeof(SettingsPage),
                    _ => typeof(OverviewPage)
                };
                
                ContentFrame.Navigate(pageType);
            }
        }

        private void ContentFrame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            // Update navigation selection based on the current page
            if (e.SourcePageType == typeof(OverviewPage))
                NavigationView.SelectedItem = NavigationView.MenuItems[0];
            else if (e.SourcePageType == typeof(TransactionsPage))
                NavigationView.SelectedItem = NavigationView.MenuItems[1];
            else if (e.SourcePageType == typeof(BudgetsPage))
                NavigationView.SelectedItem = NavigationView.MenuItems[2];
            else if (e.SourcePageType == typeof(GoalsPage))
                NavigationView.SelectedItem = NavigationView.MenuItems[3];
            else if (e.SourcePageType == typeof(DebtsPage))
                NavigationView.SelectedItem = NavigationView.MenuItems[4];
            else if (e.SourcePageType == typeof(ReportsPage))
                NavigationView.SelectedItem = NavigationView.MenuItems[5];
            else if (e.SourcePageType == typeof(SettingsPage))
                NavigationView.SelectedItem = NavigationView.FooterMenuItems[0];
        }
    }
}
