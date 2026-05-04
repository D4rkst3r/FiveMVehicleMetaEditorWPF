using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Services;

namespace FiveMVehicleMetaEditorWPF.ViewModels
{
    /// <summary>
    /// ViewModel for MainWindow - handles tab switching and app-level state
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _statusMessage = "Ready";
        private string _currentTab = "dashboard";
        private bool _isLoading = false;

        public ICommand ShowTabCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand ExportCommand { get; }

        public MainWindowViewModel()
        {
            ShowTabCommand = new RelayCommand(param =>
            {
                if (param is string tabId)
                    ShowTab(tabId);
            });

            SaveCommand = new RelayCommand(_ =>
            {
                StatusMessage = "Save command triggered";
            });

            LoadCommand = new RelayCommand(_ =>
            {
                StatusMessage = "Load command triggered";
            });

            ExportCommand = new RelayCommand(_ =>
            {
                StatusMessage = "Export command triggered";
            });

            // Subscribe to global settings changes for live updates
            AppSettingsService.Instance.SettingChanged += OnSettingChanged;
            AppSettingsService.Instance.SettingsSaved += OnSettingsSaved;
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public string CurrentTab
        {
            get => _currentTab;
            set
            {
                if (_currentTab != value)
                {
                    _currentTab = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        // Tab list for navigation
        public ObservableCollection<TabItem> Tabs { get; } = new()
        {
            new TabItem { Id = "dashboard", Icon = "📊", Name = "Dashboard" },
            new TabItem { Id = "browser", Icon = "📋", Name = "Browser" },
            new TabItem { Id = "vehicles", Icon = "🚗", Name = "Vehicles" },
            new TabItem { Id = "generator", Icon = "🔧", Name = "Generator" },
            new TabItem { Id = "handling", Icon = "⚙️", Name = "Handling" },
            new TabItem { Id = "merger", Icon = "🔀", Name = "Merger" },
            new TabItem { Id = "splitter", Icon = "✂️", Name = "Splitter" },
            new TabItem { Id = "validator", Icon = "✓", Name = "Validator" },
            new TabItem { Id = "carcols", Icon = "🚨", Name = "Carcols" },
            new TabItem { Id = "layouts", Icon = "🗺️", Name = "Layouts" },
            new TabItem { Id = "carvariations", Icon = "🎨", Name = "Variations" },
            new TabItem { Id = "settings", Icon = "⚙️", Name = "Settings" }
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ShowTab(string tabId)
        {
            CurrentTab = tabId;
        }

        public void SetStatus(string message)
        {
            StatusMessage = message;
        }

        /// <summary>
        /// Handle live settings changes from the global AppSettingsService
        /// </summary>
        private void OnSettingChanged(object? sender, SettingChangedEventArgs e)
        {
            try
            {
                switch (e.SettingName)
                {
                    case nameof(AppSettings.FontSize):
                        ApplyFontSizeChange((int)e.Value);
                        break;

                    case nameof(AppSettings.DarkMode):
                    case nameof(AppSettings.PrimaryColor):
                    case nameof(AppSettings.SecondaryColor):
                        ApplyThemeChange();
                        break;

                    case nameof(AppSettings.CompactMode):
                        ApplyCompactModeChange((bool)e.Value);
                        break;

                    case nameof(AppSettings.ShowNotifications):
                        StatusMessage = (bool)e.Value ? "✓ Notifications enabled" : "✗ Notifications disabled";
                        break;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error applying setting: {ex.Message}";
            }
        }

        /// <summary>
        /// Apply font size changes to all text elements
        /// </summary>
        private void ApplyFontSizeChange(int fontSize)
        {
            try
            {
                Application.Current.Resources["DefaultFontSize"] = (double)fontSize;
                StatusMessage = $"✓ Font size changed to {fontSize}pt";
            }
            catch { }
        }

        /// <summary>
        /// Apply theme/color changes
        /// </summary>
        private void ApplyThemeChange()
        {
            try
            {
                var settings = AppSettingsService.Instance.CurrentSettings;

                // Update primary and secondary colors
                if (Application.Current.Resources["PrimaryBrush"] is System.Windows.Media.SolidColorBrush primaryBrush)
                {
                    string primaryColorStr = settings.PrimaryColor;
                    // Ensure color string has # prefix for ColorConverter
                    if (!primaryColorStr.StartsWith("#"))
                        primaryColorStr = "#" + primaryColorStr;

                    var primaryColor = System.Windows.Media.ColorConverter.ConvertFromString(primaryColorStr);
                    if (primaryColor is System.Windows.Media.Color color)
                        primaryBrush.Color = color;
                }

                if (Application.Current.Resources["SecondaryBrush"] is System.Windows.Media.SolidColorBrush secondaryBrush)
                {
                    string secondaryColorStr = settings.SecondaryColor;
                    // Ensure color string has # prefix for ColorConverter
                    if (!secondaryColorStr.StartsWith("#"))
                        secondaryColorStr = "#" + secondaryColorStr;

                    var secondaryColor = System.Windows.Media.ColorConverter.ConvertFromString(secondaryColorStr);
                    if (secondaryColor is System.Windows.Media.Color color)
                        secondaryBrush.Color = color;
                }

                StatusMessage = "✓ Theme updated";
            }
            catch { }
        }

        /// <summary>
        /// Apply compact mode changes
        /// </summary>
        private void ApplyCompactModeChange(bool isCompact)
        {
            try
            {
                Application.Current.Resources["CompactMode"] = isCompact;
                StatusMessage = isCompact ? "✓ Compact mode enabled" : "✓ Compact mode disabled";
            }
            catch { }
        }

        /// <summary>
        /// Handle when settings are saved
        /// </summary>
        private void OnSettingsSaved(object? sender, EventArgs e)
        {
            StatusMessage = "✓ Settings saved successfully";
        }
    }

    /// <summary>
    /// Represents a tab in the navigation sidebar
    /// </summary>
    public class TabItem
    {
        public string Id { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Name { get; set; } = "";

        public override string ToString() => Name;
    }
}
