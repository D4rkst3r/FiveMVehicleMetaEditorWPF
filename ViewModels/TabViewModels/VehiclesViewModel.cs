using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Models;
using FiveMVehicleMetaEditorWPF.Core.Services;
using Newtonsoft.Json.Linq;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class VehiclesViewModel : BaseTabViewModel
    {
        private readonly MetaVehiclesService _metaService = new();
        private readonly PresetManager _presetManager = new();
        private Vehicle? _selectedVehicle;
        private int _vehicleCount = 0;
        private string? _currentFilePath;

        public ObservableCollection<Vehicle> Vehicles { get; } = new();
        public ObservableCollection<Vehicle> FilteredVehicles { get; } = new();

        public Vehicle? SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (_selectedVehicle != value)
                {
                    _selectedVehicle = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public ICommand EditVehicleCommand { get; }

        public VehiclesViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            EditVehicleCommand = new RelayCommand(param =>
            {
                if (param is Vehicle vehicle)
                    SelectedVehicle = vehicle;
            });

            ShowInfo("Load vehicles.meta to get started");
        }

        protected override void OnLoad()
        {
            try
            {
                var filePath = FileService.OpenFileDialog("vehicles");
                if (filePath == null) return;

                ShowInfo("Loading vehicles.meta...");
                IsLoading = true;

                var (success, vehicles, error) = _metaService.LoadVehiclesMeta(filePath);

                if (success && vehicles != null)
                {
                    Vehicles.Clear();
                    foreach (var v in vehicles)
                        Vehicles.Add(v);

                    OnSearchChanged(SearchText); // Apply current filter
                    VehicleCount = vehicles.Count;
                    _currentFilePath = filePath;

                    ShowSuccess($"Loaded {vehicles.Count} vehicles from {Path.GetFileName(filePath)}");
                }
                else
                {
                    ShowError(error ?? "Failed to load vehicles.meta");
                    FileService.ShowError("Load Error", error ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Load failed: {ex.Message}");
                FileService.ShowError("Load Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected override void OnSave()
        {
            try
            {
                if (Vehicles.Count == 0)
                {
                    ShowError("No vehicles to save");
                    return;
                }

                var filePath = _currentFilePath ?? FileService.SaveFileDialog("vehicles", "vehicles.meta");
                if (filePath == null) return;

                ShowInfo("Saving vehicles.meta...");
                IsLoading = true;

                var (success, error) = _metaService.SaveVehiclesMeta(filePath, new List<Vehicle>(Vehicles));

                if (success)
                {
                    _currentFilePath = filePath;
                    ShowSuccess($"Saved {Vehicles.Count} vehicles to {Path.GetFileName(filePath)}");
                }
                else
                {
                    ShowError(error ?? "Failed to save vehicles.meta");
                    FileService.ShowError("Save Error", error ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Save failed: {ex.Message}");
                FileService.ShowError("Save Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected override void OnExport()
        {
            try
            {
                if (Vehicles.Count == 0)
                {
                    ShowError("No vehicles to export");
                    return;
                }

                var filePath = FileService.SaveFileDialog("json", "vehicles_export.json");
                if (filePath == null) return;

                ShowInfo("Exporting vehicles to JSON...");
                IsLoading = true;

                var vehicleList = new List<Vehicle>(Vehicles);
                var jsonData = JObject.FromObject(new { vehicles = vehicleList });

                File.WriteAllText(filePath, jsonData.ToString());
                ShowSuccess($"Exported {Vehicles.Count} vehicles to {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                ShowError($"Export failed: {ex.Message}");
                FileService.ShowError("Export Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        protected override void OnSearchChanged(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                FilteredVehicles.Clear();
                foreach (var v in Vehicles)
                    FilteredVehicles.Add(v);
            }
            else
            {
                var filtered = _metaService.FilterVehicles(
                    new System.Collections.Generic.List<Vehicle>(Vehicles),
                    searchText
                );
                FilteredVehicles.Clear();
                foreach (var v in filtered)
                    FilteredVehicles.Add(v);
            }
        }
    }
}
