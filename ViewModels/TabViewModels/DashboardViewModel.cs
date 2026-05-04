using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class DashboardViewModel : BaseTabViewModel
    {
        private int _backupCount = 0;
        private int _presetCount = 0;
        private string _appVersion = "v1.0.2";

        public int BackupCount
        {
            get => _backupCount;
            set
            {
                if (_backupCount != value)
                {
                    _backupCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public int PresetCount
        {
            get => _presetCount;
            set
            {
                if (_presetCount != value)
                {
                    _presetCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public string AppVersion
        {
            get => _appVersion;
            set
            {
                if (_appVersion != value)
                {
                    _appVersion = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> Features { get; } = new()
        {
            "🚗 Complete vehicles.meta editor with vehicle flags",
            "⚙️ Handling physics editor with 40+ parameters",
            "🗺️ vehiclelayouts editor with seat & door config",
            "🎨 carvariations editor for colors & liveries",
            "💾 Auto-backup system (5 recent backups)",
            "💤 Export/import vehicle profiles as JSON",
            "🎯 Custom presets for quick configuration",
            "⌨️ Keyboard shortcuts (Ctrl+S, Ctrl+E, Ctrl+I)",
            "🔍 Real-time search & filtering in all tabs",
            "✓ Comprehensive validation & error handling"
        };

        public ObservableCollection<string> Shortcuts { get; } = new()
        {
            "Ctrl+S → Save current file",
            "Ctrl+E → Export as profile",
            "Ctrl+I → Import profile",
            "Ctrl+F → Focus search",
            "Escape → Clear search"
        };

        public DashboardViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            UpdateStats();
            ShowInfo("Welcome to FiveM Vehicle Meta Editor");
        }

        private void UpdateStats()
        {
            // Count backups — check both local and AppData locations
            try
            {
                int count = 0;
                var backupPath1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".backups");
                var backupPath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FiveMVehicleMetaEditor", "backups");
                if (Directory.Exists(backupPath1))
                    count += Directory.GetFiles(backupPath1, "*.bak").Length;
                if (Directory.Exists(backupPath2))
                    count += Directory.GetFiles(backupPath2, "*.bak").Length;
                BackupCount = count;
            }
            catch { }

            // Count presets — check both local and AppData locations
            try
            {
                int count = 0;
                var presetPath1 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "custom_presets");
                var presetPath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FiveMVehicleMetaEditor", "presets");
                if (Directory.Exists(presetPath1))
                    count += Directory.GetFiles(presetPath1, "*.json").Length;
                if (Directory.Exists(presetPath2))
                    count += Directory.GetFiles(presetPath2, "*.json").Length;
                PresetCount = count;
            }
            catch { }
        }
    }
}
