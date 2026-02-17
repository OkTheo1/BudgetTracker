using System;
using System.IO;
using System.Threading.Tasks;
using MoneyTracker2026.Data;

namespace MoneyTracker2026.Services
{
    public static class BackupService
    {
        public static async Task CreateBackupAsync()
        {
            if (!SettingsService.Settings.AutoBackupOnClose)
                return;

            try
            {
                var backupPath = SettingsService.Settings.BackupPath;
                Directory.CreateDirectory(backupPath);

                var profileName = ProfileManager.CurrentProfile?.Name ?? "Default";
                var sourceDbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MoneyTracker2026", "Profiles", $"{profileName}.db");

                if (!File.Exists(sourceDbPath))
                    return;

                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var backupFileName = $"{profileName}_{timestamp}.db";
                var backupFullPath = Path.Combine(backupPath, backupFileName);

                await Task.Run(() => File.Copy(sourceDbPath, backupFullPath, true));

                // Clean up old backups
                await CleanOldBackupsAsync(backupPath, profileName);

                System.Diagnostics.Debug.WriteLine($"Backup created: {backupFullPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating backup: {ex.Message}");
            }
        }

        private static async Task CleanOldBackupsAsync(string backupPath, string profileName)
        {
            await Task.Run(() =>
            {
                try
                {
                    var maxBackups = SettingsService.Settings.MaxBackups;
                    var pattern = $"{profileName}_*.db";
                    var backupFiles = new DirectoryInfo(backupPath)
                        .GetFiles(pattern)
                        .OrderByDescending(f => f.CreationTime)
                        .ToList();

                    if (backupFiles.Count > maxBackups)
                    {
                        foreach (var file in backupFiles.Skip(maxBackups))
                        {
                            file.Delete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error cleaning backups: {ex.Message}");
                }
            });
        }

        public static string GetLatestBackupPath(string profileName)
        {
            var backupPath = SettingsService.Settings.BackupPath;
            var pattern = $"{profileName}_*.db";
            
            var latestBackup = new DirectoryInfo(backupPath)
                .GetFiles(pattern)
                .OrderByDescending(f => f.CreationTime)
                .FirstOrDefault();

            return latestBackup?.FullName ?? string.Empty;
        }

        public static async Task RestoreBackupAsync(string backupFilePath)
        {
            try
            {
                var profileName = ProfileManager.CurrentProfile?.Name ?? "Default";
                var targetDbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MoneyTracker2026", "Profiles", $"{profileName}.db");

                await Task.Run(() => File.Copy(backupFilePath, targetDbPath, true));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restoring backup: {ex.Message}");
                throw;
            }
        }
    }
}
