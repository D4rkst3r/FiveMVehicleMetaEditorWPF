using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Services;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class SplitterViewModel : BaseTabViewModel
    {
        private readonly MetaVehiclesService _vehiclesService = new();
        private string _sourceFile = "";
        private string _outputFolder = "";
        private int _vehicleCount = 0;

        public string SourceFile
        {
            get => _sourceFile;
            set { _sourceFile = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanSplit)); }
        }

        public string OutputFolder
        {
            get => _outputFolder;
            set { _outputFolder = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanSplit)); }
        }

        public int VehicleCount
        {
            get => _vehicleCount;
            set { _vehicleCount = value; OnPropertyChanged(); }
        }

        public bool CanSplit => !string.IsNullOrEmpty(SourceFile) && !string.IsNullOrEmpty(OutputFolder);

        public ObservableCollection<string> SplitResults { get; } = new();

        public new ICommand LoadCommand { get; }
        public ICommand BrowseFolderCommand { get; }
        public ICommand SplitCommand { get; }

        public SplitterViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            LoadCommand = new RelayCommand(ExecuteLoad);
            BrowseFolderCommand = new RelayCommand(ExecuteBrowseFolder);
            SplitCommand = new RelayCommand(ExecuteSplit);

            ShowInfo("Load a vehicles.meta to split into individual files");
        }

        private void ExecuteLoad()
        {
            try
            {
                var filePath = FileService.OpenFileDialog("vehicles");
                if (filePath == null) return;

                IsLoading = true;
                ShowInfo("Loading file...");

                var (success, vehicles, error) = _vehiclesService.LoadVehiclesMeta(filePath);
                if (success && vehicles != null)
                {
                    SourceFile = filePath;
                    VehicleCount = vehicles.Count;
                    ShowSuccess($"Loaded {vehicles.Count} vehicles from {Path.GetFileName(filePath)}");
                }
                else
                {
                    ShowError(error ?? "Failed to load file");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteBrowseFolder()
        {
            try
            {
                var dialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    Description = "Select output folder for split files"
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    OutputFolder = dialog.SelectedPath;
                    ShowInfo($"Output folder: {OutputFolder}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error selecting folder: {ex.Message}");
            }
        }

        private void ExecuteSplit()
        {
            try
            {
                if (string.IsNullOrEmpty(SourceFile))
                {
                    ShowError("No source file loaded");
                    return;
                }
                if (string.IsNullOrEmpty(OutputFolder))
                {
                    ShowError("No output folder selected");
                    return;
                }

                IsLoading = true;
                ShowInfo("Splitting file...");
                SplitResults.Clear();

                var (success, vehicles, error) = _vehiclesService.LoadVehiclesMeta(SourceFile);
                if (!success || vehicles == null)
                {
                    ShowError(error ?? "Failed to load source file");
                    return;
                }

                int saved = 0;
                foreach (var vehicle in vehicles)
                {
                    try
                    {
                        var doc = new XDocument(
                            new XDeclaration("1.0", "UTF-8", null),
                            new XElement("CVehicleModelInfo__InitDataList",
                                new XElement("InitDatas",
                                    vehicle.OriginalElement ?? new XElement("Item",
                                        new XElement("modelName", vehicle.ModelName),
                                        new XElement("txdName", vehicle.TxdName),
                                        new XElement("handlingId", vehicle.HandlingId),
                                        new XElement("gameName", vehicle.GameName),
                                        new XElement("vehicleMakeName", vehicle.VehicleMakeName),
                                        new XElement("vehicleClass", vehicle.VehicleClass),
                                        new XElement("vehicleType", vehicle.VehicleType)
                                    )
                                )
                            )
                        );

                        var fileName = Path.Combine(OutputFolder, $"{vehicle.ModelName}.meta");
                        doc.Save(fileName);
                        SplitResults.Add($"✓ {vehicle.ModelName}.meta");
                        saved++;
                    }
                    catch
                    {
                        SplitResults.Add($"✗ {vehicle.ModelName} (failed)");
                    }
                }

                ShowSuccess($"Split {saved} vehicles into {OutputFolder}");
            }
            catch (Exception ex)
            {
                ShowError($"Split error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
