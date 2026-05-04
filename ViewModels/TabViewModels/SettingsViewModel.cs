using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Services;
using static FiveMVehicleMetaEditorWPF.Core.AppSettingsService;
using WinForms = System.Windows.Forms;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly SettingsManager _settingsManager;
        private bool _darkMode;
        private string _primaryColor;
        private string _secondaryColor;
        private int _fontSize;
        private bool _compactMode;
        private int _autoSaveInterval;
        private string _defaultDirectory;
        private string _backupLocation;
        private int _maxRecentFiles;
        private bool _autoLoadRecentFile;
        private bool _showNotifications;
        private bool _confirmOnClose;
        private bool _enableKeyboardShortcuts;
        private string _appVersion;
        private bool _isLoading = false;

        public ICommand SaveCommand { get; }
        public ICommand BrowseDefaultDirectoryCommand { get; }
        public ICommand BrowseBackupLocationCommand { get; }

        public SettingsViewModel()
        {
            _settingsManager = new SettingsManager();
            LoadSettingsFromManager();

            SaveCommand = new RelayCommand(_ =>
            {
                SaveSettings();
            });

            BrowseDefaultDirectoryCommand = new RelayCommand(_ =>
            {
                BrowseDirectory("Select Default Directory", ref _defaultDirectory, nameof(DefaultDirectory));
            });

            BrowseBackupLocationCommand = new RelayCommand(_ =>
            {
                BrowseDirectory("Select Backup Location", ref _backupLocation, nameof(BackupLocation));
            });
        }

        private void LoadSettingsFromManager()
        {
            _isLoading = true;
            try
            {
                var settings = _settingsManager.Settings;
                DarkMode = settings.DarkMode;
                PrimaryColor = settings.PrimaryColor;
                SecondaryColor = settings.SecondaryColor;
                FontSize = settings.FontSize;
                CompactMode = settings.CompactMode;
                AutoSaveInterval = settings.AutoSaveInterval;
                DefaultDirectory = settings.DefaultDirectory;
                BackupLocation = settings.BackupLocation;
                MaxRecentFiles = settings.MaxRecentFiles;
                AutoLoadRecentFile = settings.AutoLoadRecentFile;
                ShowNotifications = settings.ShowNotifications;
                ConfirmOnClose = settings.ConfirmOnClose;
                EnableKeyboardShortcuts = settings.EnableKeyboardShortcuts;
                AppVersion = settings.AppVersion;
            }
            finally
            {
                _isLoading = false;
            }
        }

        public void SaveSettings()
        {
            var settings = _settingsManager.Settings;
            settings.DarkMode = DarkMode;
            settings.PrimaryColor = PrimaryColor;
            settings.SecondaryColor = SecondaryColor;
            settings.FontSize = FontSize;
            settings.CompactMode = CompactMode;
            settings.AutoSaveInterval = AutoSaveInterval;
            settings.DefaultDirectory = DefaultDirectory;
            settings.BackupLocation = BackupLocation;
            settings.MaxRecentFiles = MaxRecentFiles;
            settings.AutoLoadRecentFile = AutoLoadRecentFile;
            settings.ShowNotifications = ShowNotifications;
            settings.ConfirmOnClose = ConfirmOnClose;
            settings.EnableKeyboardShortcuts = EnableKeyboardShortcuts;

            _settingsManager.SaveSettings();

            // Notify global settings service
            AppSettingsService.Instance.SaveSettings();
        }

        // Properties
        public bool DarkMode { get => _darkMode; set { if (_darkMode != value) { _darkMode = value; OnPropertyChanged(); } } }
        public string PrimaryColor { get => _primaryColor; set { if (_primaryColor != value) { _primaryColor = value; OnPropertyChanged(); } } }
        public string SecondaryColor { get => _secondaryColor; set { if (_secondaryColor != value) { _secondaryColor = value; OnPropertyChanged(); } } }
        public int FontSize { get => _fontSize; set { if (_fontSize != value) { _fontSize = value; OnPropertyChanged(); } } }
        public bool CompactMode { get => _compactMode; set { if (_compactMode != value) { _compactMode = value; OnPropertyChanged(); } } }
        public int AutoSaveInterval { get => _autoSaveInterval; set { if (_autoSaveInterval != value) { _autoSaveInterval = value; OnPropertyChanged(); } } }
        public string DefaultDirectory { get => _defaultDirectory; set { if (_defaultDirectory != value) { _defaultDirectory = value; OnPropertyChanged(); } } }
        public string BackupLocation { get => _backupLocation; set { if (_backupLocation != value) { _backupLocation = value; OnPropertyChanged(); } } }
        public int MaxRecentFiles { get => _maxRecentFiles; set { if (_maxRecentFiles != value) { _maxRecentFiles = value; OnPropertyChanged(); } } }
        public bool AutoLoadRecentFile { get => _autoLoadRecentFile; set { if (_autoLoadRecentFile != value) { _autoLoadRecentFile = value; OnPropertyChanged(); } } }
        public bool ShowNotifications { get => _showNotifications; set { if (_showNotifications != value) { _showNotifications = value; OnPropertyChanged(); } } }
        public bool ConfirmOnClose { get => _confirmOnClose; set { if (_confirmOnClose != value) { _confirmOnClose = value; OnPropertyChanged(); } } }
        public bool EnableKeyboardShortcuts { get => _enableKeyboardShortcuts; set { if (_enableKeyboardShortcuts != value) { _enableKeyboardShortcuts = value; OnPropertyChanged(); } } }
        public string AppVersion { get => _appVersion; set { if (_appVersion != value) { _appVersion = value; OnPropertyChanged(); } } }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Notify global settings service of live changes (but not during initial load)
            if (!_isLoading && propertyName != null)
            {
                var property = GetType().GetProperty(propertyName);
                if (property != null)
                {
                    var value = property.GetValue(this);
                    AppSettingsService.Instance.UpdateSetting(propertyName, value!);
                }
            }
        }

        /// <summary>
        /// Opens a folder browser dialog to select a directory
        /// </summary>
        private void BrowseDirectory(string title, ref string fieldToUpdate, string propertyName)
        {
            try
            {
                using (var dialog = new WinForms.FolderBrowserDialog())
                {
                    dialog.Description = title;
                    dialog.SelectedPath = fieldToUpdate;

                    if (dialog.ShowDialog() == WinForms.DialogResult.OK && !string.IsNullOrEmpty(dialog.SelectedPath))
                    {
                        // Update the field
                        fieldToUpdate = dialog.SelectedPath;

                        // Notify property changed
                        OnPropertyChanged(propertyName);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error selecting directory: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
