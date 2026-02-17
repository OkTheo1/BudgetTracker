using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MoneyTracker2026.Data;
using MoneyTracker2026.Models;

namespace MoneyTracker2026.Services
{
    public static class ProfileManager
    {
        private static readonly string ProfilesFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "MoneyTracker2026", "profiles.json");

        private static List<Profile> _profiles = new();
        private static Profile? _currentProfile;

        public static Profile? CurrentProfile => _currentProfile;
        public static IReadOnlyList<Profile> Profiles => _profiles.AsReadOnly();

        public static void Initialize()
        {
            LoadProfiles();
            
            if (_profiles.Count == 0)
            {
                CreateDefaultProfile();
            }
        }

        private static void LoadProfiles()
        {
            try
            {
                if (File.Exists(ProfilesFilePath))
                {
                    var json = File.ReadAllText(ProfilesFilePath);
                    _profiles = JsonSerializer.Deserialize<List<Profile>>(json) ?? new List<Profile>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profiles: {ex.Message}");
                _profiles = new List<Profile>();
            }
        }

        private static void SaveProfiles()
        {
            try
            {
                var directory = Path.GetDirectoryName(ProfilesFilePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                var json = JsonSerializer.Serialize(_profiles, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ProfilesFilePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving profiles: {ex.Message}");
            }
        }

        private static void CreateDefaultProfile()
        {
            var profile = new Profile
            {
                Id = 1,
                Name = "Main",
                DataFilePath = "Main.db",
                IsActive = true,
                CreatedAt = DateTime.Now,
                LastAccessedAt = DateTime.Now
            };

            _profiles.Add(profile);
            SaveProfiles();
            
            // Initialize database
            using var context = new AppDbContext(profile.Name);
            context.Database.EnsureCreated();
            
            _currentProfile = profile;
        }

        public static void SwitchProfile(string profileName)
        {
            var profile = _profiles.Find(p => p.Name == profileName);
            if (profile != null)
            {
                profile.LastAccessedAt = DateTime.Now;
                _currentProfile = profile;
                SaveProfiles();
            }
        }

        public static void CreateProfile(string name)
        {
            var profile = new Profile
            {
                Id = _profiles.Count + 1,
                Name = name,
                DataFilePath = $"{name}.db",
                IsActive = true,
                CreatedAt = DateTime.Now,
                LastAccessedAt = DateTime.Now
            };

            _profiles.Add(profile);
            SaveProfiles();
            
            // Create database
            using var context = new AppDbContext(profile.Name);
            context.Database.EnsureCreated();
        }

        public static void DeleteProfile(string profileName)
        {
            var profile = _profiles.Find(p => p.Name == profileName);
            if (profile != null && _profiles.Count > 1)
            {
                _profiles.Remove(profile);
                SaveProfiles();
                
                // Delete database file
                var dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MoneyTracker2026", "Profiles", profile.DataFilePath);
                
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                }
            }
        }
    }
}
