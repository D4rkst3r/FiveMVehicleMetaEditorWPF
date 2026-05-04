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

        public string? SelectedVariation
        {
            get => _selectedVariation;
            set { _selectedVariation = value; OnPropertyChanged(); }
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

                if (root == null)
                {
                    ShowError("Invalid XML: No root element");
                    return;
                }

                Variations.Clear();

                // carvariations.meta structure: variationData > Item > modelName
                var items = root.Descendants("variationData")
                    .Elements("Item")
                    .ToList();

                if (items.Count == 0)
                    items = root.Descendants("Item").ToList();

                foreach (var item in items)
                {
                    var modelName = item.Element("modelName")?.Value;
                    if (!string.IsNullOrWhiteSpace(modelName))
                        Variations.Add(modelName);
                }

                _currentFilePath = filePath;

                if (Variations.Count == 0)
                    ShowInfo($"Loaded carvariations.meta - no vehicle models found");
                else
                    ShowSuccess($"Loaded {Variations.Count} vehicle variations from {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading: {ex.Message}");
                FileService.ShowError("Load Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteSave()
        {
            try
            {
                if (_loadedDoc == null)
                {
                    ShowError("No file loaded to save");
                    return;
                }

                var filePath = string.IsNullOrEmpty(_currentFilePath)
                    ? FileService.SaveFileDialog("carvariations", "carvariations.meta")
                    : _currentFilePath;
                if (filePath == null) return;

                IsLoading = true;
                _loadedDoc.Save(filePath);
                _currentFilePath = filePath;
                ShowSuccess($"Saved to {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                ShowError($"Error saving: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteExport()
        {
            try
            {
                if (Variations.Count == 0)
                {
                    ShowError("No variations to export");
                    return;
                }

                var filePath = FileService.SaveFileDialog("json", "carvariations_export.json");
                if (filePath == null) return;

                IsLoading = true;
                var json = JObject.FromObject(new { vehicles = Variations, file = _currentFilePath });
                File.WriteAllText(filePath, json.ToString());
                ShowSuccess($"Exported {Variations.Count} vehicles to {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                ShowError($"Error exporting: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
