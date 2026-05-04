using System;
using System.Collections.Generic;
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

        public ObservableCollection<string> CarcolEntries { get; } = new();

        public string? SelectedEntry
        {
            get => _selectedEntry;
            set { _selectedEntry = value; OnPropertyChanged(); }
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

                if (root == null)
                {
                    ShowError("Invalid XML: No root element");
                    return;
                }

                CarcolEntries.Clear();

                // Try different carcols.meta structures
                var kits = root.Descendants("Kit").ToList();
                if (kits.Count > 0)
                {
                    foreach (var kit in kits)
                    {
                        var kitName = kit.Element("kitName")?.Value
                                   ?? kit.Attribute("value")?.Value
                                   ?? "(unnamed kit)";
                        CarcolEntries.Add(kitName);
                    }
                }
                else
                {
                    // Try alternate structure
                    var items = root.Descendants("Item").ToList();
                    foreach (var item in items.Take(200))
                    {
                        var name = item.Element("kitName")?.Value
                                ?? item.Attribute("id")?.Value
                                ?? item.Element("modelName")?.Value;
                        if (!string.IsNullOrEmpty(name))
                            CarcolEntries.Add(name);
                    }
                }

                _currentFilePath = filePath;

                if (CarcolEntries.Count == 0)
                    ShowInfo($"Loaded carcols.meta - no named entries found (file has {root.Descendants().Count()} elements)");
                else
                    ShowSuccess($"Loaded {CarcolEntries.Count} entries from {Path.GetFileName(filePath)}");
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
                    ? FileService.SaveFileDialog("carcols", "carcols.meta")
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
                if (CarcolEntries.Count == 0)
                {
                    ShowError("No entries to export");
                    return;
                }

                var filePath = FileService.SaveFileDialog("json", "carcols_export.json");
                if (filePath == null) return;

                IsLoading = true;
                var json = JObject.FromObject(new { entries = CarcolEntries, file = _currentFilePath });
                File.WriteAllText(filePath, json.ToString());
                ShowSuccess($"Exported {CarcolEntries.Count} entries to {Path.GetFileName(filePath)}");
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
