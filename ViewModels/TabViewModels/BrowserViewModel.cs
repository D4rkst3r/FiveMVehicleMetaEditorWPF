using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Services;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class BrowserViewModel : BaseTabViewModel
    {
        private int _vehicleCount = 0;
        private int _layoutCount = 0;
        private string _selectedFile = "";

        public int VehicleCount
        {
            get => _vehicleCount;
            set
            {
                if (_vehicleCount != value)
                {
                    _vehicleCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public int LayoutCount
        {
            get => _layoutCount;
            set
            {
                if (_layoutCount != value)
                {
                    _layoutCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedFile
        {
            get => _selectedFile;
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> RecentFiles { get; } = new();
        public ObservableCollection<string> Vehicles { get; } = new();

        public ICommand LoadVehiclesCommand { get; }
        public ICommand LoadHandlingCommand { get; }
        public ICommand LoadLayoutsCommand { get; }

        public BrowserViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            LoadVehiclesCommand = new RelayCommand(_ => OnLoadVehicles());
            LoadHandlingCommand = new RelayCommand(_ => OnLoadHandling());
            LoadLayoutsCommand = new RelayCommand(_ => OnLoadLayouts());

            ShowInfo("Ready to load meta files");
        }

        private void OnLoadVehicles()
        {
            try
            {
                var filePath = FileService.OpenFileDialog("vehicles");
                if (filePath == null) return;

                ShowInfo("Loading vehicles.meta...");
                IsLoading = true;

                var service = new MetaVehiclesService();
                var (success, vehicles, error) = service.LoadVehiclesMeta(filePath);

                if (success && vehicles != null)
                {
                    Vehicles.Clear();
                    foreach (var v in vehicles)
                        Vehicles.Add(v.ModelName);

                    VehicleCount = vehicles.Count;
                    SelectedFile = filePath;
                    ShowSuccess($"Loaded {vehicles.Count} vehicles");
                }
                else
                {
                    ShowError(error ?? "Failed to load vehicles.meta");
                    FileService.ShowError("Load Error", error ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
                FileService.ShowError("Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnLoadHandling()
        {
            try
            {
                var filePath = FileService.OpenFileDialog("handling");
                if (filePath == null) return;

                ShowInfo("Loading handling.meta...");
                IsLoading = true;

                var service = new MetaHandlingService();
                var (success, handlingData, error) = service.LoadHandlingMeta(filePath);

                if (success && handlingData != null)
                {
                    ShowSuccess($"Loaded {handlingData.Count} handling entries");
                    SelectedFile = filePath;
                }
                else
                {
                    ShowError(error ?? "Failed to load handling.meta");
                    FileService.ShowError("Load Error", error ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
                FileService.ShowError("Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnLoadLayouts()
        {
            try
            {
                var filePath = FileService.OpenFileDialog("layouts");
                if (filePath == null) return;

                ShowInfo("Loading vehiclelayouts.meta...");
                IsLoading = true;

                var service = new MetaLayoutsService();
                var (success, layouts, error) = service.LoadLayoutsMeta(filePath);

                if (success && layouts != null)
                {
                    LayoutCount = layouts.Count;
                    SelectedFile = filePath;
                    ShowSuccess($"Loaded {layouts.Count} layouts");
                }
                else
                {
                    ShowError(error ?? "Failed to load vehiclelayouts.meta");
                    FileService.ShowError("Load Error", error ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
                FileService.ShowError("Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
