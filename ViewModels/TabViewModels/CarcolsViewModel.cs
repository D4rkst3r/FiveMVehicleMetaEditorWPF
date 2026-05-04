using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Services;
using Newtonsoft.Json.Linq;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class CarcolsViewModel : BaseTabViewModel
    {
        private string _currentFilePath = "";
        private string? _selectedEntry;
        private XDocument? _loadedDoc;

        // List of kit names
        public ObservableCollection<string> CarcolEntries { get; } = new();

        // Detail properties for selected kit
        private string _kitId = "";
        private string _kitType = "";
        private int _visibleModCount = 0;
        private int _statModCount = 0;
        private int _linkModCount = 0;
        public ObservableCollection<string> VisibleMods { get; } = new();

        public string KitId
        {
            get => _kitId;
            set { _kitId = value; OnPropertyChanged(); }
        }
        public string KitType
        {
            get => _kitType;
            set { _kitType = value; OnPropertyChanged(); }
        }
        public int VisibleModCount
        {
            get => _visibleModCount;
            set { _visibleModCount = value; OnPropertyChanged(); }
        }
        public int StatModCount
        {
            get => _statModCount;
            set { _statModCount = value; OnPropertyChanged(); }
        }
        public int LinkModCount
        {
            get => _linkModCount;
            set { _linkModCount = value; OnPropertyChanged(); }
        }

        public string? SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                OnPropertyChanged();
                LoadKitDetails(value);
            }
        }

        public new ICommand LoadCommand { get; }
        public new ICommand SaveCommand { get; }
        public new ICommand ExportCommand { get; }

        public CarcolsViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            LoadCommand = new RelayCommand(ExecuteLoad);
            SaveCommand = new RelayCommand(ExecuteSave);
            ExportCommand = new RelayCommand(ExecuteExport);
            ShowInfo("Load a carcols.meta file to edit vehicle color palettes");
        }

        private void LoadKitDetails(string? kitName)
        {
            VisibleMods.Clear();
            KitId = "";
            KitType = "";
            VisibleModCount = 0;
            StatModCount = 0;
            LinkModCount = 0;

            if (kitName == null || _loadedDoc == null) return;

            var kit = _loadedDoc.Root?
                .Descendants("Kits")
                .Elements("Item")
                .FirstOrDefault(i => i.Element("kitName")?.Value == kitName);

            if (kit == null) return;

            KitId = kit.Element("id")?.Attribute("value")?.Value ?? "";
            KitType = kit.Element("kitType")?.Value ?? "";

            var visible = kit.Element("visibleMods")?.Elements("Item").ToList() ?? new();
            VisibleModCount = visible.Count;
            foreach (var mod in visible)
            {
                var modName = mod.Element("modelName")?.Value;
                var modType = mod.Element("type")?.Value;
                if (!string.IsNullOrEmpty(modName))
                    VisibleMods.Add($"{modName}  [{modType}]");
            }

            StatModCount = kit.Element("statMods")?.Elements("Item").Count() ?? 0;
            LinkModCount = kit.Element("linkMods")?.Elements("Item").Count() ?? 0;
        }

        private void ExecuteLoad()
        {
            try
            {
                var filePath = FileService.OpenFileDialog("carcols");
                if (filePath == null) return;

                IsLoading = true;
                ShowInfo("Loading carcols.meta...");

                var content = File.ReadAllText(filePath);
                _loadedDoc = XDocument.Parse(content);
                var root = _loadedDoc.Root;

                if (root == null) { ShowError("Invalid XML"); return; }

                CarcolEntries.Clear();
                var kits = root.Descendants("Kits").Elements("Item").ToList();
                foreach (var kit in kits)
                {
                    var name = kit.Element("kitName")?.Value;
                    if (!string.IsNullOrEmpty(name))
                        CarcolEntries.Add(name);
                }

                _currentFilePath = filePath;

                if (CarcolEntries.Count == 0)
                    ShowInfo($"Loaded carcols.meta — no named kits found");
                else
                    ShowSuccess($"Loaded {CarcolEntries.Count} kits from {Path.GetFileName(filePath)}");
            }
            catch (Exception ex) { ShowError($"Error: {ex.Message}"); FileService.ShowError("Load Error", ex.Message); }
            finally { IsLoading = false; }
        }

        private void ExecuteSave()
        {
            try
            {
                if (_loadedDoc == null) { ShowError("No file loaded"); return; }
                var filePath = string.IsNullOrEmpty(_currentFilePath)
                    ? FileService.SaveFileDialog("carcols", "carcols.meta") : _currentFilePath;
                if (filePath == null) return;
                IsLoading = true;
                _loadedDoc.Save(filePath);
                _currentFilePath = filePath;
                ShowSuccess($"Saved to {Path.GetFileName(filePath)}");
            }
            catch (Exception ex) { ShowError($"Error saving: {ex.Message}"); }
            finally { IsLoading = false; }
        }

        private void ExecuteExport()
        {
            try
            {
                if (CarcolEntries.Count == 0) { ShowError("No entries to export"); return; }
                var filePath = FileService.SaveFileDialog("json", "carcols_export.json");
                if (filePath == null) return;
                IsLoading = true;
                var json = JObject.FromObject(new { entries = CarcolEntries, file = _currentFilePath });
                File.WriteAllText(filePath, json.ToString());
                ShowSuccess($"Exported {CarcolEntries.Count} entries to {Path.GetFileName(filePath)}");
            }
            catch (Exception ex) { ShowError($"Error: {ex.Message}"); }
            finally { IsLoading = false; }
        }
    }
}
