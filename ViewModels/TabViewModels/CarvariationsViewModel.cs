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
    public class CarvariationsViewModel : BaseTabViewModel
    {
        private readonly BackupManager _backupManager = new(".backups");
        private string _currentFilePath = "";
        private string? _selectedVariation;
        private XDocument? _loadedDoc;

        public ObservableCollection<string> Variations { get; } = new();

        private int _colorCount = 0;
        public ObservableCollection<string> KitNames { get; } = new();
        public ObservableCollection<string> ColorSummaries { get; } = new();

        // Color editing
        private string? _selectedColorSummary;
        private string _editColorIndices = "";

        public int ColorCount
        {
            get => _colorCount;
            set { _colorCount = value; OnPropertyChanged(); }
        }

        public string? SelectedVariation
        {
            get => _selectedVariation;
            set
            {
                _selectedVariation = value;
                OnPropertyChanged();
                LoadVariationDetails(value);
            }
        }

        public string? SelectedColorSummary
        {
            get => _selectedColorSummary;
            set
            {
                _selectedColorSummary = value;
                OnPropertyChanged();
                OnColorSelected(value);
            }
        }

        public string EditColorIndices
        {
            get => _editColorIndices;
            set { _editColorIndices = value; OnPropertyChanged(); }
        }

        public new ICommand LoadCommand { get; }
        public new ICommand SaveCommand { get; }
        public new ICommand ExportCommand { get; }
        public ICommand SaveColorIndicesCommand { get; }
        public ICommand AddColorVariantCommand { get; }
        public ICommand DeleteColorVariantCommand { get; }

        public CarvariationsViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            LoadCommand = new RelayCommand(ExecuteLoad);
            SaveCommand = new RelayCommand(ExecuteSave);
            ExportCommand = new RelayCommand(ExecuteExport);
            SaveColorIndicesCommand = new RelayCommand(ExecuteSaveColorIndices, _ => _selectedColorSummary != null);
            AddColorVariantCommand = new RelayCommand(ExecuteAddColorVariant, _ => _selectedVariation != null);
            DeleteColorVariantCommand = new RelayCommand(ExecuteDeleteColorVariant, _ => _selectedColorSummary != null);
            ShowInfo("Load a carvariations.meta file to edit vehicle color variations");
        }

        private void OnColorSelected(string? summary)
        {
            if (summary == null) { EditColorIndices = ""; return; }
            // Extract indices from "Variant N: [27, 0, 28, ...]"
            var start = summary.IndexOf('[');
            var end = summary.IndexOf(']');
            if (start >= 0 && end > start)
                EditColorIndices = summary.Substring(start + 1, end - start - 1).Replace(", ", " ");
            else
                EditColorIndices = "";
        }

        private int GetColorVariantIndex()
        {
            if (_selectedColorSummary == null) return -1;
            var idx = ColorSummaries.IndexOf(_selectedColorSummary);
            return idx;
        }

        private XElement? GetCurrentVariationElement()
        {
            if (_selectedVariation == null || _loadedDoc == null) return null;

            var item = _loadedDoc.Root?
                .Descendants("variationData")
                .Elements("Item")
                .FirstOrDefault(i => i.Element("modelName")?.Value == _selectedVariation);

            if (item == null)
                item = _loadedDoc.Root?
                    .Descendants("Item")
                    .FirstOrDefault(i => i.Element("modelName")?.Value == _selectedVariation);

            return item;
        }

        private void ExecuteSaveColorIndices()
        {
            if (_selectedColorSummary == null) return;
            var idx = GetColorVariantIndex();
            if (idx < 0) return;

            var item = GetCurrentVariationElement();
            if (item == null) return;

            var colors = item.Element("colors")?.Elements("Item").ToList();
            if (colors == null || idx >= colors.Count) return;

            var colorItem = colors[idx];
            var indicesElem = colorItem.Element("indices");
            if (indicesElem == null) { ShowError("Could not find indices element"); return; }

            indicesElem.Value = " " + EditColorIndices.Trim() + " ";

            // Refresh display
            var oldSummary = _selectedColorSummary;
            LoadVariationDetails(_selectedVariation);
            // Try to re-select something close
            if (idx < ColorSummaries.Count)
                SelectedColorSummary = ColorSummaries[idx];

            ShowSuccess($"Saved indices for Variant {idx + 1}");
        }

        private void ExecuteAddColorVariant()
        {
            var item = GetCurrentVariationElement();
            if (item == null) return;

            var colorsElem = item.Element("colors");
            if (colorsElem == null)
            {
                colorsElem = new XElement("colors");
                item.Add(colorsElem);
            }

            var newColor = new XElement("Item",
                new XElement("indices", " 0 0 0 0 0 0 ")
            );
            colorsElem.Add(newColor);

            LoadVariationDetails(_selectedVariation);
            ShowSuccess("Added color variant");
        }

        private void ExecuteDeleteColorVariant()
        {
            if (_selectedColorSummary == null) return;
            var idx = GetColorVariantIndex();
            if (idx < 0) return;

            var item = GetCurrentVariationElement();
            if (item == null) return;

            var colors = item.Element("colors")?.Elements("Item").ToList();
            if (colors == null || idx >= colors.Count) return;

            colors[idx].Remove();
            LoadVariationDetails(_selectedVariation);
            ShowSuccess($"Deleted Variant {idx + 1}");
        }

        private void LoadVariationDetails(string? modelName)
        {
            KitNames.Clear();
            ColorSummaries.Clear();
            ColorCount = 0;
            SelectedColorSummary = null;
            EditColorIndices = "";

            if (modelName == null || _loadedDoc == null) return;

            var item = GetCurrentVariationElement();
            if (item == null) return;

            var kits = item.Element("kits")?.Elements("Item").ToList() ?? new();
            foreach (var k in kits)
                KitNames.Add(k.Value);

            var colors = item.Element("colors")?.Elements("Item").ToList() ?? new();
            ColorCount = colors.Count;
            int i = 1;
            foreach (var color in colors)
            {
                var indices = color.Element("indices")?.Value?.Trim()
                    .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (indices != null && indices.Length > 0)
                    ColorSummaries.Add($"Variant {i++}: [{string.Join(", ", indices)}]");
            }
        }

        private void ExecuteLoad()
        {
            try
            {
                var filePath = FileService.OpenFileDialog("carvariations");
                if (filePath == null) return;

                IsLoading = true;
                ShowInfo("Loading carvariations.meta...");

                var content = File.ReadAllText(filePath);
                _loadedDoc = XDocument.Parse(content);
                var root = _loadedDoc.Root;

                if (root == null) { ShowError("Invalid XML"); return; }

                Variations.Clear();
                var items = root.Descendants("variationData").Elements("Item").ToList();
                if (items.Count == 0)
                    items = root.Descendants("Item").Where(i => i.Element("modelName") != null).ToList();

                foreach (var item in items)
                {
                    var name = item.Element("modelName")?.Value;
                    if (!string.IsNullOrWhiteSpace(name))
                        Variations.Add(name);
                }

                _currentFilePath = filePath;

                if (Variations.Count == 0)
                    ShowInfo("Loaded carvariations.meta — no vehicle models found");
                else
                    ShowSuccess($"Loaded {Variations.Count} vehicle variations from {Path.GetFileName(filePath)}");
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
                    ? FileService.SaveFileDialog("carvariations", "carvariations.meta") : _currentFilePath;
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
                if (Variations.Count == 0) { ShowError("No variations to export"); return; }
                var filePath = FileService.SaveFileDialog("json", "carvariations_export.json");
                if (filePath == null) return;
                IsLoading = true;
                var json = JObject.FromObject(new { vehicles = Variations, file = _currentFilePath });
                File.WriteAllText(filePath, json.ToString());
                ShowSuccess($"Exported {Variations.Count} vehicles to {Path.GetFileName(filePath)}");
            }
            catch (Exception ex) { ShowError($"Error: {ex.Message}"); }
            finally { IsLoading = false; }
        }
    }
}
