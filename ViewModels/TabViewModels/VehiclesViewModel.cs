using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Models;
using FiveMVehicleMetaEditorWPF.Core.Services;
using Newtonsoft.Json.Linq;

/// <summary>
/// Observable wrapper for a vehicle flag toggle
/// </summary>
public class FlagItem : INotifyPropertyChanged
{
    private bool _isEnabled;
    private readonly Action<string, bool> _onChange;

    public string Name { get; }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (_isEnabled != value)
            {
                _isEnabled = value;
                OnPropertyChanged();
                _onChange(Name, value);
            }
        }
    }

    public FlagItem(string name, bool enabled, Action<string, bool> onChange)
    {
        Name = name;
        _isEnabled = enabled;
        _onChange = onChange;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? p = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
}

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class VehiclesViewModel : BaseTabViewModel
    {
        private readonly MetaVehiclesService _metaService = new();
        private readonly PresetManager _presetManager = new();
        private readonly BackupManager _backupManager = new(".backups");
        private Vehicle? _selectedVehicle;
        private int _vehicleCount = 0;
        private string? _currentFilePath;

        // Undo/Redo stacks (store snapshots of the vehicle list)
        private readonly Stack<List<Vehicle>> _undoStack = new();
        private readonly Stack<List<Vehicle>> _redoStack = new();

        // Multi-selection for batch edit
        private ObservableCollection<Vehicle> _selectedVehicles = new();
        private string _batchVehicleClass = "";
        private string _batchVehicleType = "";
        private bool _showBatchPanel = false;

        // Flag editor items
        public ObservableCollection<FlagItem> FlagItems { get; } = new();

        public ObservableCollection<Vehicle> Vehicles { get; } = new();
        public ObservableCollection<Vehicle> FilteredVehicles { get; } = new();

        // Preset names for the dropdown
        public List<string> VehiclePresets { get; } = new()
        {
            "— Choose Preset —",
            "Sport Car",
            "SUV",
            "Truck",
            "Van",
            "Police Car",
            "Ambulance",
            "Fire Truck",
            "Motorcycle",
            "Helicopter",
            "Boat"
        };

        public Vehicle? SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (_selectedVehicle != value)
                {
                    _selectedVehicle = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasSelection));
                    RebuildFlagItems();
                }
            }
        }

        private static readonly string[] AllKnownFlags = {
            "FLAG_IS_EMERGENCY_VEHICLE", "FLAG_IS_ARMORED", "FLAG_IS_ELECTRIC",
            "FLAG_IS_ARMORED_POLICE", "FLAG_LAW_ENFORCEMENT", "FLAG_EMERGENCY_SERVICE",
            "FLAG_MILITARY", "FLAG_GANG", "FLAG_IS_FIRE_TRUCK",
            "FLAG_NO_AIRBAGS", "FLAG_ROTATE_REAR_WHEELS", "FLAG_NO_RESPRAY",
            "FLAG_LIGHT_DAMAGE", "FLAG_HAS_ADVANCED_DAMAGE", "FLAG_BULLETPROOF_WINDOWS",
            "FLAG_HAS_LIVERY", "FLAG_HAS_INTERIOR_EXTRAS", "FLAG_CAN_HAVE_NEONS",
            "FLAG_CAN_HAVE_HEADLIGHT_COLOR", "FLAG_HAS_CUSTOM_HORN", "FLAG_HAS_SPECIAL_LIVERY",
            "FLAG_USE_SEARCHLIGHT", "FLAG_HAS_ROOF_LADDER", "FLAG_EXTRAS_REQUIRE_ENGINE_ON",
            "FLAG_IS_ADDON_VEHICLE", "FLAG_IS_DIAMOND_VEHICLE", "FLAG_IS_ARENA_VEHICLE",
            "FLAG_DONT_SPAWN_IN_CARGEN", "FLAG_IGNORE_ON_FOOT_FOR_WANTED_CALCS",
            "FLAG_DONT_ALLOW_PLAYER_TO_RIDE", "FLAG_DISABLE_MOUSE_LOOK_INSIDE",
            "FLAG_CAN_ATTACK_FRIENDLY",
        };

        private void RebuildFlagItems()
        {
            FlagItems.Clear();
            if (_selectedVehicle == null) return;

            var vehicle = _selectedVehicle;
            foreach (var flag in AllKnownFlags)
            {
                FlagItems.Add(new FlagItem(flag, vehicle.Flags.Contains(flag), (name, enabled) =>
                {
                    if (enabled && !vehicle.Flags.Contains(name))
                        vehicle.Flags.Add(name);
                    else if (!enabled)
                        vehicle.Flags.Remove(name);
                }));
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

        public bool HasSelection => _selectedVehicle != null;

        public bool ShowBatchPanel
        {
            get => _showBatchPanel;
            set { _showBatchPanel = value; OnPropertyChanged(); }
        }

        public string BatchVehicleClass
        {
            get => _batchVehicleClass;
            set { _batchVehicleClass = value; OnPropertyChanged(); }
        }

        public string BatchVehicleType
        {
            get => _batchVehicleType;
            set { _batchVehicleType = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand EditVehicleCommand { get; }
        public ICommand AddVehicleCommand { get; }
        public ICommand DeleteVehicleCommand { get; }
        public ICommand DuplicateVehicleCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand ApplyPresetCommand { get; }
        public ICommand ToggleBatchPanelCommand { get; }
        public ICommand ApplyBatchClassCommand { get; }
        public ICommand ApplyBatchTypeCommand { get; }

        public VehiclesViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            EditVehicleCommand = new RelayCommand(param =>
            {
                if (param is Vehicle vehicle)
                    SelectedVehicle = vehicle;
            });

            AddVehicleCommand = new RelayCommand(_ => ExecuteAdd());
            DeleteVehicleCommand = new RelayCommand(_ => ExecuteDelete(), _ => HasSelection);
            DuplicateVehicleCommand = new RelayCommand(_ => ExecuteDuplicate(), _ => HasSelection);
            UndoCommand = new RelayCommand(_ => ExecuteUndo(), _ => _undoStack.Count > 0);
            RedoCommand = new RelayCommand(_ => ExecuteRedo(), _ => _redoStack.Count > 0);
            ToggleBatchPanelCommand = new RelayCommand(_ => ShowBatchPanel = !ShowBatchPanel);
            ApplyBatchClassCommand = new RelayCommand(_ => ExecuteApplyBatchClass());
            ApplyBatchTypeCommand = new RelayCommand(_ => ExecuteApplyBatchType());

            ApplyPresetCommand = new RelayCommand(param =>
            {
                if (param is string presetName && SelectedVehicle != null)
                    ApplyPreset(SelectedVehicle, presetName);
            });

            ShowInfo("Load vehicles.meta to get started");
        }

        private void PushUndoSnapshot()
        {
            _undoStack.Push(Vehicles.Select(v => CloneVehicle(v)).ToList());
            _redoStack.Clear();
        }

        private Vehicle CloneVehicle(Vehicle v) => new Vehicle
        {
            ModelName = v.ModelName,
            TxdName = v.TxdName,
            GameName = v.GameName,
            VehicleMakeName = v.VehicleMakeName,
            HandlingId = v.HandlingId,
            AudioNameHash = v.AudioNameHash,
            VehicleType = v.VehicleType,
            Class = v.Class,
            VehicleClass = v.VehicleClass,
            Layout = v.Layout,
            CameraName = v.CameraName,
            AimCameraName = v.AimCameraName,
            Seats = v.Seats,
            WheelScale = v.WheelScale,
            WheelScaleRear = v.WheelScaleRear,
            DefaultBodyHealth = v.DefaultBodyHealth,
            DirtLevelMax = v.DirtLevelMax,
            DamageMapScale = v.DamageMapScale,
            MaxNum = v.MaxNum,
            Frequency = v.Frequency,
            Swankness = v.Swankness,
            PlateType = v.PlateType,
            DriveableDoors = v.DriveableDoors,
            WeaponForceMult = v.WeaponForceMult,
            Flags = new List<string>(v.Flags),
            PovCameraOffsetX = v.PovCameraOffsetX,
            PovCameraOffsetY = v.PovCameraOffsetY,
            PovCameraOffsetZ = v.PovCameraOffsetZ,
        };

        private void ExecuteAdd()
        {
            PushUndoSnapshot();
            var newVehicle = new Vehicle
            {
                ModelName = $"newvehicle{Vehicles.Count + 1}",
                VehicleMakeName = "ROCKSTAR",
                VehicleType = "VEHICLE_TYPE_CAR",
                VehicleClass = "VC_SPORTS",
                HandlingId = "SPORTSCAR",
                GameName = "New Vehicle",
                Layout = "LAYOUT_STD_CAR",
                Seats = 4,
            };
            Vehicles.Add(newVehicle);
            FilteredVehicles.Add(newVehicle);
            VehicleCount = Vehicles.Count;
            SelectedVehicle = newVehicle;
            ShowSuccess($"Added new vehicle '{newVehicle.ModelName}'");
        }

        private void ExecuteDelete()
        {
            if (SelectedVehicle == null) return;
            PushUndoSnapshot();
            var toDelete = SelectedVehicle;
            var idx = Vehicles.IndexOf(toDelete);
            Vehicles.Remove(toDelete);
            FilteredVehicles.Remove(toDelete);
            VehicleCount = Vehicles.Count;
            SelectedVehicle = Vehicles.Count > 0
                ? Vehicles[Math.Min(idx, Vehicles.Count - 1)]
                : null;
            ShowSuccess($"Deleted '{toDelete.ModelName}'");
        }

        private void ExecuteDuplicate()
        {
            if (SelectedVehicle == null) return;
            PushUndoSnapshot();
            var clone = CloneVehicle(SelectedVehicle);
            clone.ModelName = SelectedVehicle.ModelName + "_copy";
            var idx = Vehicles.IndexOf(SelectedVehicle);
            Vehicles.Insert(idx + 1, clone);
            FilteredVehicles.Insert(Math.Min(idx + 1, FilteredVehicles.Count), clone);
            VehicleCount = Vehicles.Count;
            SelectedVehicle = clone;
            ShowSuccess($"Duplicated as '{clone.ModelName}'");
        }

        private void ExecuteUndo()
        {
            if (_undoStack.Count == 0) return;
            _redoStack.Push(Vehicles.Select(v => CloneVehicle(v)).ToList());
            var snapshot = _undoStack.Pop();
            RestoreSnapshot(snapshot);
            ShowInfo($"Undo — {Vehicles.Count} vehicles");
        }

        private void ExecuteRedo()
        {
            if (_redoStack.Count == 0) return;
            _undoStack.Push(Vehicles.Select(v => CloneVehicle(v)).ToList());
            var snapshot = _redoStack.Pop();
            RestoreSnapshot(snapshot);
            ShowInfo($"Redo — {Vehicles.Count} vehicles");
        }

        private void RestoreSnapshot(List<Vehicle> snapshot)
        {
            Vehicles.Clear();
            FilteredVehicles.Clear();
            foreach (var v in snapshot)
            {
                Vehicles.Add(v);
                FilteredVehicles.Add(v);
            }
            VehicleCount = Vehicles.Count;
            SelectedVehicle = null;
        }

        private void ExecuteApplyBatchClass()
        {
            if (string.IsNullOrWhiteSpace(BatchVehicleClass)) return;
            PushUndoSnapshot();
            int count = 0;
            foreach (var v in Vehicles)
            {
                v.VehicleClass = BatchVehicleClass;
                count++;
            }
            ShowSuccess($"Set VehicleClass='{BatchVehicleClass}' on {count} vehicles");
        }

        private void ExecuteApplyBatchType()
        {
            if (string.IsNullOrWhiteSpace(BatchVehicleType)) return;
            PushUndoSnapshot();
            int count = 0;
            foreach (var v in Vehicles)
            {
                v.VehicleType = BatchVehicleType;
                count++;
            }
            ShowSuccess($"Set VehicleType='{BatchVehicleType}' on {count} vehicles");
        }

        private void ApplyPreset(Vehicle vehicle, string presetName)
        {
            switch (presetName)
            {
                case "Sport Car":
                    vehicle.VehicleType = "VEHICLE_TYPE_CAR";
                    vehicle.VehicleClass = "VC_SPORTS";
                    vehicle.Layout = "LAYOUT_STD_CAR";
                    vehicle.Seats = 2;
                    vehicle.HandlingId = "SPORTSCAR";
                    break;
                case "SUV":
                    vehicle.VehicleType = "VEHICLE_TYPE_CAR";
                    vehicle.VehicleClass = "VC_SUV";
                    vehicle.Layout = "LAYOUT_STD_CAR_BIG";
                    vehicle.Seats = 4;
                    vehicle.HandlingId = "SUV";
                    break;
                case "Truck":
                    vehicle.VehicleType = "VEHICLE_TYPE_TRUCK";
                    vehicle.VehicleClass = "VC_COMMERCIAL";
                    vehicle.Layout = "LAYOUT_STD_TRUCK";
                    vehicle.Seats = 2;
                    vehicle.HandlingId = "TRUCK";
                    break;
                case "Van":
                    vehicle.VehicleType = "VEHICLE_TYPE_CAR";
                    vehicle.VehicleClass = "VC_VANS";
                    vehicle.Layout = "LAYOUT_STD_VAN";
                    vehicle.Seats = 2;
                    vehicle.HandlingId = "MULE";
                    break;
                case "Police Car":
                    vehicle.VehicleType = "VEHICLE_TYPE_CAR";
                    vehicle.VehicleClass = "VC_EMERGENCY";
                    vehicle.Layout = "LAYOUT_STD_CAR";
                    vehicle.Seats = 4;
                    vehicle.HandlingId = "POLICE";
                    if (!vehicle.Flags.Contains("FLAG_IS_EMERGENCY_VEHICLE"))
                        vehicle.Flags.Add("FLAG_IS_EMERGENCY_VEHICLE");
                    if (!vehicle.Flags.Contains("FLAG_LAW_ENFORCEMENT"))
                        vehicle.Flags.Add("FLAG_LAW_ENFORCEMENT");
                    break;
                case "Ambulance":
                    vehicle.VehicleType = "VEHICLE_TYPE_CAR";
                    vehicle.VehicleClass = "VC_EMERGENCY";
                    vehicle.Layout = "LAYOUT_STD_VAN";
                    vehicle.Seats = 4;
                    vehicle.HandlingId = "AMBULANCE";
                    if (!vehicle.Flags.Contains("FLAG_IS_EMERGENCY_VEHICLE"))
                        vehicle.Flags.Add("FLAG_IS_EMERGENCY_VEHICLE");
                    break;
                case "Fire Truck":
                    vehicle.VehicleType = "VEHICLE_TYPE_TRUCK";
                    vehicle.VehicleClass = "VC_EMERGENCY";
                    vehicle.Layout = "LAYOUT_STD_TRUCK";
                    vehicle.Seats = 4;
                    vehicle.HandlingId = "FIRETRUCK";
                    if (!vehicle.Flags.Contains("FLAG_IS_FIRE_TRUCK"))
                        vehicle.Flags.Add("FLAG_IS_FIRE_TRUCK");
                    if (!vehicle.Flags.Contains("FLAG_IS_EMERGENCY_VEHICLE"))
                        vehicle.Flags.Add("FLAG_IS_EMERGENCY_VEHICLE");
                    break;
                case "Motorcycle":
                    vehicle.VehicleType = "VEHICLE_TYPE_BIKE";
                    vehicle.VehicleClass = "VC_MOTORCYCLE";
                    vehicle.Layout = "LAYOUT_STD_BIKE";
                    vehicle.Seats = 2;
                    vehicle.HandlingId = "AKUMA";
                    break;
                case "Helicopter":
                    vehicle.VehicleType = "VEHICLE_TYPE_HELI";
                    vehicle.VehicleClass = "VC_HELICOPTER";
                    vehicle.Layout = "LAYOUT_STD_HELI";
                    vehicle.Seats = 4;
                    vehicle.HandlingId = "POLMAV";
                    break;
                case "Boat":
                    vehicle.VehicleType = "VEHICLE_TYPE_BOAT";
                    vehicle.VehicleClass = "VC_BOATS";
                    vehicle.Layout = "LAYOUT_STD_BOAT";
                    vehicle.Seats = 4;
                    vehicle.HandlingId = "DINGHY";
                    break;
            }
            // Notify UI to refresh
            OnPropertyChanged(nameof(SelectedVehicle));
            ShowSuccess($"Applied preset '{presetName}' to '{vehicle.ModelName}'");
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

                    OnSearchChanged(SearchText);
                    VehicleCount = vehicles.Count;
                    _currentFilePath = filePath;
                    _undoStack.Clear();
                    _redoStack.Clear();

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

                // Auto-backup before saving
                if (File.Exists(filePath))
                    _backupManager.CreateBackup(filePath);

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
                    new List<Vehicle>(Vehicles),
                    searchText
                );
                FilteredVehicles.Clear();
                foreach (var v in filtered)
                    FilteredVehicles.Add(v);
            }
        }

        /// <summary>
        /// Load vehicles from a file path directly (e.g., from drag-drop)
        /// </summary>
        public void LoadFromPath(string filePath)
        {
            try
            {
                ShowInfo("Loading vehicles.meta...");
                IsLoading = true;
                var (success, vehicles, error) = _metaService.LoadVehiclesMeta(filePath);
                if (success && vehicles != null)
                {
                    Vehicles.Clear();
                    foreach (var v in vehicles) Vehicles.Add(v);
                    OnSearchChanged(SearchText);
                    VehicleCount = vehicles.Count;
                    _currentFilePath = filePath;
                    _undoStack.Clear();
                    _redoStack.Clear();
                    ShowSuccess($"Loaded {vehicles.Count} vehicles from {Path.GetFileName(filePath)}");
                }
                else
                {
                    ShowError(error ?? "Failed to load");
                }
            }
            catch (Exception ex) { ShowError(ex.Message); }
            finally { IsLoading = false; }
        }
    }
}
