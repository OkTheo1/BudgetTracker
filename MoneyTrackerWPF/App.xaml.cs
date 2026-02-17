using System;
using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using MoneyTracker2026.Data;
using MoneyTracker2026.Services;

namespace MoneyTracker2026;

public partial class App : Application
{
    public static AppDbContext? DbContext { get; private set; }
    public static string AppDataPath { get; private set; } = string.Empty;
    public static string DatabasePath { get; private set; } = string.Empty;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Set up app data directory
        AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MoneyTracker2026");

        if (!Directory.Exists(AppDataPath))
        {
            Directory.CreateDirectory(AppDataPath);
        }

        // Set up database path
        DatabasePath = Path.Combine(AppDataPath, "moneytracker.db");

        // Initialize database
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        try
        {
            // Use the constructor that takes a string path
            DbContext = new AppDbContext(DatabasePath);
            DbContext.Database.EnsureCreated();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing database: {ex.Message}",
                "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Auto-backup on close
        try
        {
            BackupService.CreateAutomaticBackup(DatabasePath);
        }
        catch
        {
            // Ignore backup errors on exit
        }

        DbContext?.Dispose();
        base.OnExit(e);
    }
}
