using System.IO;
using System.Text.Json;

namespace MoneyTracker2026.Services;

public class AppSettings
{
    public string BaseCurrencyCode { get; set; } = "GBP";
    public string Theme { get; set; } = "Dark";
    public string AccentColor { get; set; } = "#00C4B4"; // Mint/Teal
    public string BackupPath { get; set; } = string.Empty;
    public bool AutoBackupOnClose { get; set; } = true;
    public int? CurrentProfileId { get; set; }
}

public class SettingsService
{
    private readonly string _settingsPath;
    private AppSettings _settings;

    public SettingsService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MoneyTracker2026");
        
        Directory.CreateDirectory(appDataPath);
        _settingsPath = Path.Combine(appDataPath, "settings.json");
        _settings = LoadSettings();
    }

    public AppSettings Settings => _settings;

    private AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
        }
        return new AppSettings();
    }

    public void SaveSettings()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsPath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    public void SetBaseCurrency(string currencyCode)
    {
        _settings.BaseCurrencyCode = currencyCode;
        SaveSettings();
    }

    public void SetTheme(string theme)
    {
        _settings.Theme = theme;
        SaveSettings();
    }

    public void SetAccentColor(string color)
    {
        _settings.AccentColor = color;
        SaveSettings();
    }

    public void SetBackupPath(string path)
    {
        _settings.BackupPath = path;
        SaveSettings();
    }

    public void SetCurrentProfile(int profileId)
    {
        _settings.CurrentProfileId = profileId;
        SaveSettings();
    }

    // Async methods for compatibility with views
    public Task<AppSettings> GetSettingsAsync()
    {
        return Task.FromResult(_settings);
    }

    public Task SetBaseCurrencyAsync(string currencyCode)
    {
        SetBaseCurrency(currencyCode);
        return Task.CompletedTask;
    }

    public Task SetAutoBackupAsync(bool enabled)
    {
        _settings.AutoBackupOnClose = enabled;
        SaveSettings();
        return Task.CompletedTask;
    }
}
