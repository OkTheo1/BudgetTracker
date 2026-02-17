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

            // Set up navigation
            ContentFrame.Navigate(typeof(OverviewPage));
            NavigationView.SelectedItem = NavigationView.MenuItems[0];

            // Handle window state changes
            this.SizeChanged += MainWindow_SizeChanged;
            this.WindowStateChanged += MainWindow_WindowStateChanged;
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is NavigationViewItem item && item.TargetPageType != null)
            {
                Type pageType = item.TargetPageType;
                ContentFrame.Navigate(pageType);
                TitleText.Text = $"MoneyTracker 2026 - {item.Content}";
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContentFrame.CanGoBack)
            {
                ContentFrame.GoBack();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                MaximizeButton.Content = "\uE739"; // Maximize icon
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                MaximizeButton.Content = "\uE923"; // Restore icon
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Update title bar visibility or layout if needed
        }

        private void MainWindow_WindowStateChanged(object sender, WindowStateChangedEventArgs args)
        {
            // Update maximize button icon based on window state
            if (this.WindowState == WindowState.Maximized)
            {
                MaximizeButton.Content = "\uE923"; // Restore icon
            }
            else
            {
                MaximizeButton.Content = "\uE739"; // Maximize icon
            }
        }

        public void SetTitleBar(UIElement titleBar)
        {
            // This is handled in OnLaunched in App.xaml.cs
        }
    }
}
