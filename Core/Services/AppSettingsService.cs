using System;
using FiveMVehicleMetaEditorWPF.Core.Services;

namespace FiveMVehicleMetaEditorWPF.Core
{
    /// <summary>
    /// Global singleton service for application settings with live change notifications
    /// </summary>
    public class AppSettingsService
    {
        private static AppSettingsService? _instance;
        private static readonly object LockObject = new object();

        private SettingsManager _settingsManager;
        private AppSettings _currentSettings;

        // Events for live setting changes
        public event EventHandler<SettingChangedEventArgs>? SettingChanged;
        public event EventHandler? SettingsSaved;

        private AppSettingsService()
        {
            _settingsManager = new SettingsManager();
            _currentSettings = _settingsManager.Settings;
        }

        /// <summary>
        /// Get the singleton instance
        /// </summary>
        public static AppSettingsService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (LockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new AppSettingsService();
                        }
                    }
                }
                return _instance;
            }
        }

        public AppSettings CurrentSettings => _currentSettings;

        /// <summary>
        /// Update a setting and notify subscribers
        /// </summary>
        public void UpdateSetting(string settingName, object value)
        {
            var property = typeof(AppSettings).GetProperty(settingName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(_currentSettings, value);
                SettingChanged?.Invoke(this, new SettingChangedEventArgs(settingName, value));
            }
        }

        /// <summary>
        /// Save all settings to disk
        /// </summary>
        public void SaveSettings()
        {
            _settingsManager.SaveSettings();
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Reload settings from disk
        /// </summary>
        public void ReloadSettings()
        {
            _settingsManager.LoadSettings();
            _currentSettings = _settingsManager.Settings;
        }
    }

    /// <summary>
    /// Event args for setting changes
    /// </summary>
    public class SettingChangedEventArgs : EventArgs
    {
        public string SettingName { get; }
        public object Value { get; }

        public SettingChangedEventArgs(string settingName, object value)
        {
            SettingName = settingName;
            Value = value;
        }
    }
}
