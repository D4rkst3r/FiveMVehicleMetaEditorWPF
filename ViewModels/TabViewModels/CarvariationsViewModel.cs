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
        private string _currentFilePath = "";
        private string? _selectedVariation;
        private XDocument? _loadedDoc;

        public ObservableCollection<string> Variations { get; } = new();

        // Detail properties for selected vehicle
        private int _colorCount = 0;
        public ObservableCollection<string> KitNames { get; } = new();
        public ObservableCollection<string> ColorSummaries { get; } = new();

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

        public new ICommand LoadCommand { get; }
        public new ICommand SaveCommand { get; }
        public new ICommand ExportCommand { get; }

        public CarvariationsViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            LoadCommand = new RelayCommand(ExecuteLoad);
            SaveCommand = new RelayCommand(ExecuteSave);
            ExportCommand = new RelayCommand(ExecuteExport);
            ShowInfo("Load a carvariations.meta file to edit vehicle color variations");
        }

        private void LoadVariationDetails(string? modelName)
        {
            KitNames.Clear();
            ColorSummaries.Clear();
            ColorCount = 0;

            if (modelName == null || _loadedDoc == null) return;

            var item = _loadedDoc.Root?
                .Descendants("variationData")
                .Elements("Item")
                .FirstOrDefault(i => i.Element("modelName")?.Value == modelName);

            if (item == null) return;

            // Kits
            var kits = item.Element("kits")?.Elements("Item").ToList() ?? new();
            foreach (var k in kits)
                KitNames.Add(k.Value);

            // Colors
            var colors = item.Element("colors")?.Elements("Item").ToList() ?? new();
            ColorCount = colors.Count;
            int idx = 1;
            foreach (var color in colors)
            {
                var indices = color.Element("indices")?.Value?.Trim()
                    .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (indices != null && indices.Length > 0)
                    ColorSummaries.Add($"Variant {idx++}: [{string.Join(", ", indices)}]");
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
