using System;
using System.IO;
using Newtonsoft.Json;

namespace FiveMVehicleMetaEditorWPF.Core.Services
{
    /// <summary>
    /// Manages application settings persistence
    /// </summary>
    public class SettingsManager
    {
        private readonly string _settingsPath;
        private AppSettings _settings;

        public SettingsManager()
        {
            _settingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FiveMVehicleMetaEditor",
                "settings.json");

            LoadSettings();
        }

        public AppSettings Settings => _settings;

        public void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = File.ReadAllText(_settingsPath);
                    _settings = JsonConvert.DeserializeObject<AppSettings>(json) ?? GetDefaults();
                }
                else
                {
                    _settings = GetDefaults();
                }
            }
            catch
            {
                _settings = GetDefaults();
            }
        }

        public void SaveSettings()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
                var json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
                File.WriteAllText(_settingsPath, json);
            }
            catch { }
        }

        private static AppSettings GetDefaults()
        {
            return new AppSettings
            {
                // Appearance
                DarkMode = true,
                PrimaryColor = "#FF9500",    // Orange
                SecondaryColor = "#00D4FF",  // Cyan
                FontSize = 12,
                CompactMode = false,

                // File Management
                AutoSaveInterval = 300,     // 5 minutes
                DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                BackupLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FiveMVehicleMetaEditor", "Backups"),
                MaxRecentFiles = 10,

                // Behavior
                AutoLoadRecentFile = false,
                ShowNotifications = true,
                ConfirmOnClose = true,
                EnableKeyboardShortcuts = true,

                // Version
                AppVersion = "1.0.2"
            };
        }
    }

    public class AppSettings
    {
        // Appearance
        public bool DarkMode { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
        public int FontSize { get; set; }
        public bool CompactMode { get; set; }

        // File Management
        public int AutoSaveInterval { get; set; }
        public string DefaultDirectory { get; set; }
        public string BackupLocation { get; set; }
        public int MaxRecentFiles { get; set; }

        // Behavior
        public bool AutoLoadRecentFile { get; set; }
        public bool ShowNotifications { get; set; }
        public bool ConfirmOnClose { get; set; }
        public bool EnableKeyboardShortcuts { get; set; }

        // Version
        public string AppVersion { get; set; }
    }
}
