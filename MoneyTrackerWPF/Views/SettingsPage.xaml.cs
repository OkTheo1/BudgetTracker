using System.Windows;
using System.Windows.Controls;

namespace MoneyTracker2026.Views;

public partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private void NewProfile_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Create new profile
    }

    private void DeleteProfile_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Delete current profile
    }

    private void BackupNow_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Create backup now
    }

    private void RestoreBackup_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Restore from backup
    }

    private void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Save all settings
        MessageBox.Show("Settings saved successfully!", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
