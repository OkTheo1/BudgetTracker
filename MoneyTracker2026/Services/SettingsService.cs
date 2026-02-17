using System;
using System.IO;
using System.Text.Json;

namespace MoneyTracker2026.Services
{
    public class AppSettings
    {
        public string BaseCurrencyCode { get; set; } = "GBP";
        public string Theme { get; set; } = "Dark";
        public string AccentColor { get; set; } = "#00C4B4";
        public bool AutoBackupOnClose { get; set; } = true;
        public string BackupPath { get; set; } = string.Empty;
        public int MaxBackups { get; set; } = 10;
        public bool ShowNotifications { get; set; } = true;
        public bool AutoApplyRecurrings { get; set; } = true;
    }

    public static class SettingsService
    {
        private static readonly string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MoneyTracker2026", "settings.json");

        private static AppSettings _settings = new();

        public static AppSettings Settings => _settings;

        public static void Initialize()
        {
            LoadSettings();
        }

        private static void LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                
                // Set default backup path if not set
                if (string.IsNullOrEmpty(_settings.BackupPath))
                {
                    _settings.BackupPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                        "MoneyTrackerBackups");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
                _settings = new AppSettings();
            }
        }

        public static void SaveSettings()
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public static void UpdateBaseCurrency(string currencyCode)
        {
            _settings.BaseCurrencyCode = currencyCode;
            SaveSettings();
        }

        public static void UpdateTheme(string theme)
        {
            _settings.Theme = theme;
            SaveSettings();
        }

        public static void UpdateAccentColor(string colorHex)
        {
            _settings.AccentColor = colorHex;
            SaveSettings();
        }

        public static void SetSetting(string key, string value)
        {
            switch (key)
            {
                case "BaseCurrency":
                    _settings.BaseCurrencyCode = value;
                    break;
                case "Theme":
                    _settings.Theme = value;
                    break;
                case "AccentColor":
                    _settings.AccentColor = value;
                    break;
                case "AutoBackupOnClose":
                    _settings.AutoBackupOnClose = bool.Parse(value);
                    break;
                case "BackupPath":
                    _settings.BackupPath = value;
                    break;
                case "MaxBackups":
                    _settings.MaxBackups = int.Parse(value);
                    break;
                case "ShowNotifications":
                    _settings.ShowNotifications = bool.Parse(value);
                    break;
                case "AutoApplyRecurrings":
                    _settings.AutoApplyRecurrings = bool.Parse(value);
                    break;
            }
            SaveSettings();
        }
    }
}
