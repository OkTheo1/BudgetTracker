using Microsoft.UI.Xaml.Controls;
using System.Linq;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;
using MoneyTracker2026.Services;

namespace MoneyTracker2026.Views
{
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var profileName = ProfileManager.CurrentProfile?.Name ?? "Default";
            using var context = new AppDbContext(profileName);

            // Load profiles
            ProfileComboBox.Items.Clear();
            var profiles = context.Profiles.ToList();
            foreach (var profile in profiles)
            {
                ProfileComboBox.Items.Add(new ComboBoxItem { Content = profile.Name });
            }
            
            // Select current profile
            if (ProfileManager.CurrentProfile != null)
            {
                for (int i = 0; i < ProfileComboBox.Items.Count; i++)
                {
                    if (ProfileComboBox.Items[i] is ComboBoxItem item && 
                        item.Content?.ToString() == ProfileManager.CurrentProfile.Name)
                    {
                        ProfileComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void ProfileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfileComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var profileName = selectedItem.Content?.ToString();
                if (!string.IsNullOrEmpty(profileName))
                {
                    ProfileManager.SwitchProfile(profileName);
                }
            }
        }

        private void CreateProfile_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var dialog = new ContentDialog
            {
                Title = "Create Profile",
                Content = "Profile creation dialog coming soon!",
                CloseButtonText = "OK"
            };
            dialog.ShowAsync();
        }

        private void BaseCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Save base currency preference
            if (BaseCurrencyComboBox.SelectedIndex >= 0)
            {
                var currencies = new[] { "GBP", "USD", "EUR" };
                SettingsService.SetSetting("BaseCurrency", currencies[BaseCurrencyComboBox.SelectedIndex]);
            }
        }

        private void BackupNow_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            BackupService.CreateBackup();
            
            var dialog = new ContentDialog
            {
                Title = "Backup Complete",
                Content = "Your data has been backed up successfully!",
                CloseButtonText = "OK"
            };
            dialog.ShowAsync();
        }
    }
}
