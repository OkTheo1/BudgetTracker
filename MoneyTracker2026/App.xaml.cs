using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.IO;
using MoneyTracker2026.Services;

namespace MoneyTracker2026
{
    public partial class App : Application
    {
        private static readonly string AppDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MoneyTracker2026");

        public static string AppDataFolder => AppDataPath;
        public static string ProfilesPath => Path.Combine(AppDataPath, "Profiles");
        public static string BackupsPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "MoneyTrackerBackups");

        public App()
        {
            this.InitializeComponent();
            
            // Ensure app data directories exist
            Directory.CreateDirectory(AppDataPath);
            Directory.CreateDirectory(ProfilesPath);
            Directory.CreateDirectory(BackupsPath);
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchedEventArgs args)
        {
            // Initialize services
            ProfileManager.Initialize();
            SettingsService.Initialize();

            var window = new MainWindow();
            window.Title = "MoneyTracker 2026";
            window.ExtendsContentIntoTitleBar = true;
            window.SetTitleBar(AppTitleBar());
            window.Activate();

            // Setup auto-backup on close
            window.Closed += async (sender, e) =>
            {
                await BackupService.CreateBackupAsync();
            };
        }

        private UIElement AppTitleBar()
        {
            // Title bar will be defined in MainWindow
            return null;
        }
    }
}
