using System.IO;

namespace MoneyTracker2026.Services;

public class BackupService
{
    private readonly SettingsService _settingsService;
    private readonly string? _dbPath;

    public BackupService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public BackupService(SettingsService settingsService, string dbPath)
    {
        _settingsService = settingsService;
        _dbPath = dbPath;
    }

    public string GetBackupPath()
    {
        var backupPath = _settingsService?.Settings?.BackupPath;
        if (string.IsNullOrEmpty(backupPath))
        {
            backupPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "MoneyTrackerBackups");
        }
        
        Directory.CreateDirectory(backupPath);
        return backupPath;
    }

    public void BackupDatabase(string dbPath)
    {
        try
        {
            if (!File.Exists(dbPath))
            {
                System.Diagnostics.Debug.WriteLine($"Database file not found: {dbPath}");
                return;
            }

            var backupDir = GetBackupPath();
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var backupFileName = $"MoneyTracker_{timestamp}.db";
            var backupPath = Path.Combine(backupDir, backupFileName);

            File.Copy(dbPath, backupPath, true);
            
            System.Diagnostics.Debug.WriteLine($"Database backed up to: {backupPath}");
            
            // Clean up old backups (keep last 10)
            CleanupOldBackups(backupDir, 10);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error backing up database: {ex.Message}");
        }
    }

    private void CleanupOldBackups(string backupDir, int keepCount)
    {
        try
        {
            var backupFiles = Directory.GetFiles(backupDir, "MoneyTracker_*.db")
                .OrderByDescending(f => File.GetCreationTime(f))
                .ToList();

            if (backupFiles.Count > keepCount)
            {
                foreach (var file in backupFiles.Skip(keepCount))
                {
                    File.Delete(file);
                    System.Diagnostics.Debug.WriteLine($"Deleted old backup: {file}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cleaning up backups: {ex.Message}");
        }
    }

    public void RestoreBackup(string backupPath, string targetDbPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("Backup file not found", backupPath);
            }

            // Close any existing connections first
            File.Copy(backupPath, targetDbPath, true);
            
            System.Diagnostics.Debug.WriteLine($"Database restored from: {backupPath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error restoring database: {ex.Message}");
            throw;
        }
    }

    public List<string> GetAvailableBackups()
    {
        var backupDir = GetBackupPath();
        if (!Directory.Exists(backupDir))
        {
            return new List<string>();
        }

        return Directory.GetFiles(backupDir, "MoneyTracker_*.db")
            .OrderByDescending(f => File.GetCreationTime(f))
            .ToList();
    }

    // Async methods for compatibility with views
    public Task<bool> BackupNowAsync()
    {
        if (string.IsNullOrEmpty(_dbPath))
        {
            return Task.FromResult(false);
        }
        
        try
        {
            BackupDatabase(_dbPath);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public Task BackupDatabaseAsync(string dbPath)
    {
        BackupDatabase(dbPath);
        return Task.CompletedTask;
    }

    // Static method for automatic backup on app close
    public static void CreateAutomaticBackup(string dbPath)
    {
        try
        {
            if (!File.Exists(dbPath))
            {
                return;
            }

            var backupPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "MoneyTrackerBackups");
            
            Directory.CreateDirectory(backupPath);
            
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var backupFileName = $"MoneyTracker_{timestamp}.db";
            var fullBackupPath = Path.Combine(backupPath, backupFileName);

            File.Copy(dbPath, fullBackupPath, true);
            
            System.Diagnostics.Debug.WriteLine($"Automatic backup created: {fullBackupPath}");
            
            // Keep only last 10 backups
            var backupFiles = Directory.GetFiles(backupPath, "MoneyTracker_*.db")
                .OrderByDescending(f => File.GetCreationTime(f))
                .Skip(10)
                .ToList();

            foreach (var file in backupFiles)
            {
                try { File.Delete(file); } catch { }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating automatic backup: {ex.Message}");
        }
    }
}
