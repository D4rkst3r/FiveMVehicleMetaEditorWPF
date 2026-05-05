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
        private readonly BackupManager _backupManager = new(".backups");
        private string _currentFilePath = "";
        private string? _selectedEntry;
        private XDocument? _loadedDoc;

        public ObservableCollection<string> CarcolEntries { get; } = new();

        // Read-only detail properties
        private string _kitId = "";
        private string _kitType = "";
        private int _visibleModCount = 0;
        private int _statModCount = 0;
        private int _linkModCount = 0;
        public ObservableCollection<string> VisibleMods { get; } = new();

        // Editable kit properties
        private string _editKitName = "";
        private string _editKitId = "";
        private string _editKitType = "";

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

        public string EditKitName
        {
            get => _editKitName;
            set { _editKitName = value; OnPropertyChanged(); }
        }
        public string EditKitId
        {
            get => _editKitId;
            set { _editKitId = value; OnPropertyChanged(); }
        }
        public string EditKitType
        {
            get => _editKitType;
            set { _editKitType = value; OnPropertyChanged(); }
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
        public ICommand AddKitCommand { get; }
        public ICommand DeleteKitCommand { get; }
        public ICommand RenameKitCommand { get; }
        public ICommand ApplyKitChangesCommand { get; }

        public CarcolsViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            LoadCommand = new RelayCommand(ExecuteLoad);
            SaveCommand = new RelayCommand(ExecuteSave);
            ExportCommand = new RelayCommand(ExecuteExport);
            AddKitCommand = new RelayCommand(ExecuteAddKit);
            DeleteKitCommand = new RelayCommand(ExecuteDeleteKit, _ => _selectedEntry != null);
            RenameKitCommand = new RelayCommand(ExecuteRenameKit, _ => _selectedEntry != null);
            ApplyKitChangesCommand = new RelayCommand(ExecuteApplyKitChanges, _ => _selectedEntry != null);
            ShowInfo("Load a carcols.meta file to edit vehicle color palettes");
        }

        private void LoadKitDetails(string? kitName)
        {
            VisibleMods.Clear();
            KitId = "";
            KitType = "";
            EditKitName = "";
            EditKitId = "";
            EditKitType = "";
            VisibleModCount = 0;
            StatModCount = 0;
            LinkModCount = 0;

            if (kitName == null || _loadedDoc == null) return;

            var kit = FindKit(kitName);
            if (kit == null) return;

            KitId = kit.Element("id")?.Attribute("value")?.Value ?? "";
            KitType = kit.Element("kitType")?.Value ?? "";
            EditKitName = kitName;
            EditKitId = KitId;
            EditKitType = KitType;

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

        private XElement? FindKit(string kitName)
        {
            return _loadedDoc?.Root?
                .Descendants("Kits")
                .Elements("Item")
                .FirstOrDefault(i => i.Element("kitName")?.Value == kitName);
        }

        private int GetNextKitId()
        {
            if (_loadedDoc == null) return 1;
            var ids = _loadedDoc.Root?
                .Descendants("Kits").Elements("Item")
                .Select(i => int.TryParse(i.Element("id")?.Attribute("value")?.Value, out var id) ? id : 0)
                .ToList() ?? new();
            return (ids.Count > 0 ? ids.Max() : 0) + 1;
        }

        private void ExecuteAddKit()
        {
            if (_loadedDoc == null) { ShowError("No file loaded"); return; }

            var newId = GetNextKitId();
            var newName = $"kit_{newId:D3}";

            var kitsElem = _loadedDoc.Root?.Descendants("Kits").FirstOrDefault();
            if (kitsElem == null)
            {
                // Create Kits element
                _loadedDoc.Root?.Add(new XElement("Kits"));
                kitsElem = _loadedDoc.Root?.Element("Kits");
            }

            var newKit = new XElement("Item",
                new XElement("kitName", newName),
                new XElement("id", new XAttribute("value", newId)),
                new XElement("kitType", "KIT_TYPE_STANDARD"),
                new XElement("visibleMods"),
                new XElement("statMods"),
                new XElement("linkMods")
            );

            kitsElem?.Add(newKit);
            CarcolEntries.Add(newName);
            SelectedEntry = newName;
            ShowSuccess($"Added kit '{newName}' (ID {newId})");
        }

        private void ExecuteDeleteKit()
        {
            if (_selectedEntry == null || _loadedDoc == null) return;

            var kit = FindKit(_selectedEntry);
            if (kit == null) return;

            var name = _selectedEntry;
            kit.Remove();
            CarcolEntries.Remove(name);
            SelectedEntry = null;
            ShowSuccess($"Deleted kit '{name}'");
        }

        private void ExecuteRenameKit()
        {
            if (_selectedEntry == null || _loadedDoc == null) return;
            if (string.IsNullOrWhiteSpace(EditKitName)) { ShowError("Kit name cannot be empty"); return; }

            var kit = FindKit(_selectedEntry);
            if (kit == null) return;

            var oldName = _selectedEntry;
            var newName = EditKitName.Trim();

            kit.Element("kitName")!.Value = newName;

            var idx = CarcolEntries.IndexOf(oldName);
            if (idx >= 0) CarcolEntries[idx] = newName;

            SelectedEntry = newName;
            ShowSuccess($"Renamed '{oldName}' → '{newName}'");
        }

        private void ExecuteApplyKitChanges()
        {
            if (_selectedEntry == null || _loadedDoc == null) return;

            var kit = FindKit(_selectedEntry);
            if (kit == null) return;

            // Apply ID change
            if (!string.IsNullOrWhiteSpace(EditKitId))
            {
                var idElem = kit.Element("id");
                if (idElem == null) kit.Add(new XElement("id", new XAttribute("value", EditKitId)));
                else idElem.SetAttributeValue("value", EditKitId);
                KitId = EditKitId;
            }

            // Apply type change
            if (!string.IsNullOrWhiteSpace(EditKitType))
            {
                var typeElem = kit.Element("kitType");
                if (typeElem == null) kit.Add(new XElement("kitType", EditKitType));
                else typeElem.Value = EditKitType;
                KitType = EditKitType;
            }

            // Apply name change
            if (!string.IsNullOrWhiteSpace(EditKitName) && EditKitName != _selectedEntry)
            {
                ExecuteRenameKit();
                return;
            }

            ShowSuccess("Kit changes applied");
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

                if (File.Exists(filePath))
                    _backupManager.CreateBackup(filePath);

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
