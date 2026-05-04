using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Models;
using FiveMVehicleMetaEditorWPF.Core.Services;
using Newtonsoft.Json.Linq;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    /// <summary>
    /// ViewModel for Handling Tab (handling.meta editor)
    /// </summary>
    public class HandlingViewModel : BaseTabViewModel
    {
        private readonly MetaHandlingService _handlingService = new();
        private string _currentFilePath = "";
        private List<HandlingData> _allHandlingEntries = new(); // master list for search

        // Observable collection of handling entries
        private ObservableCollection<HandlingData> _handlingEntries = new();
        public ObservableCollection<HandlingData> HandlingEntries
        {
            get => _handlingEntries;
            set { _handlingEntries = value; OnPropertyChanged(); }
        }

        // Selected handling entry
        private HandlingData? _selectedHandling;
        public HandlingData? SelectedHandling
        {
            get => _selectedHandling;
            set
            {
                _selectedHandling = value;
                // Notify ALL properties so UI refreshes all values for newly selected entry
                OnPropertyChanged(string.Empty);
            }
        }

        // Search text for filtering
        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                OnSearchChanged();
            }
        }

        // Mass & Dimensions section
        public float Mass
        {
            get => _selectedHandling?.Mass ?? 1500f;
            set { if (_selectedHandling != null) _selectedHandling.Mass = value; OnPropertyChanged(); }
        }

        public float DimensionsX
        {
            get => _selectedHandling?.Dimensions_X ?? 2f;
            set { if (_selectedHandling != null) _selectedHandling.Dimensions_X = value; OnPropertyChanged(); }
        }

        public float DimensionsY
        {
            get => _selectedHandling?.Dimensions_Y ?? 4f;
            set { if (_selectedHandling != null) _selectedHandling.Dimensions_Y = value; OnPropertyChanged(); }
        }

        public float DimensionsZ
        {
            get => _selectedHandling?.Dimensions_Z ?? 1.5f;
            set { if (_selectedHandling != null) _selectedHandling.Dimensions_Z = value; OnPropertyChanged(); }
        }

        // Transmission section
        public float AccelerationX
        {
            get => _selectedHandling?.AccelerationX ?? 0.3f;
            set { if (_selectedHandling != null) _selectedHandling.AccelerationX = value; OnPropertyChanged(); }
        }

        public float AccelerationY
        {
            get => _selectedHandling?.AccelerationY ?? 0.0f;
            set { if (_selectedHandling != null) _selectedHandling.AccelerationY = value; OnPropertyChanged(); }
        }

        public float AccelerationZ
        {
            get => _selectedHandling?.AccelerationZ ?? 0.0f;
            set { if (_selectedHandling != null) _selectedHandling.AccelerationZ = value; OnPropertyChanged(); }
        }

        public int NumberOfGears
        {
            get => _selectedHandling?.NumberOfGears ?? 6;
            set { if (_selectedHandling != null) _selectedHandling.NumberOfGears = value; OnPropertyChanged(); }
        }

        public float TopSpeed
        {
            get => _selectedHandling?.TopSpeed ?? 200f;
            set { if (_selectedHandling != null) _selectedHandling.TopSpeed = value; OnPropertyChanged(); }
        }

        // Steering & Braking section
        public float SteeringLock
        {
            get => _selectedHandling?.SteeringLock ?? 45f;
            set { if (_selectedHandling != null) _selectedHandling.SteeringLock = value; OnPropertyChanged(); }
        }

        public float SteeringBias
        {
            get => _selectedHandling?.SteeringBias ?? 0f;
            set { if (_selectedHandling != null) _selectedHandling.SteeringBias = value; OnPropertyChanged(); }
        }

        public float BrakeForce
        {
            get => _selectedHandling?.BrakeForce ?? 1f;
            set { if (_selectedHandling != null) _selectedHandling.BrakeForce = value; OnPropertyChanged(); }
        }

        public float BrakeBias
        {
            get => _selectedHandling?.BrakeBias ?? 0.5f;
            set { if (_selectedHandling != null) _selectedHandling.BrakeBias = value; OnPropertyChanged(); }
        }

        // Suspension section
        public float SuspensionHeight
        {
            get => _selectedHandling?.SuspensionHeight ?? 0.1f;
            set { if (_selectedHandling != null) _selectedHandling.SuspensionHeight = value; OnPropertyChanged(); }
        }

        public float SuspensionLowerLimit
        {
            get => _selectedHandling?.SuspensionLowerLimit ?? 0.0f;
            set { if (_selectedHandling != null) _selectedHandling.SuspensionLowerLimit = value; OnPropertyChanged(); }
        }

        public float SuspensionUpperLimit
        {
            get => _selectedHandling?.SuspensionUpperLimit ?? 0.3f;
            set { if (_selectedHandling != null) _selectedHandling.SuspensionUpperLimit = value; OnPropertyChanged(); }
        }

        public float SuspensionStiffness
        {
            get => _selectedHandling?.SuspensionStiffness ?? 1f;
            set { if (_selectedHandling != null) _selectedHandling.SuspensionStiffness = value; OnPropertyChanged(); }
        }

        public float SuspensionDamping
        {
            get => _selectedHandling?.SuspensionDamping ?? 0.05f;
            set { if (_selectedHandling != null) _selectedHandling.SuspensionDamping = value; OnPropertyChanged(); }
        }

        // Traction section
        public float TractionCurveMax
        {
            get => _selectedHandling?.TractionCurveMax ?? 1.3f;
            set { if (_selectedHandling != null) _selectedHandling.TractionCurveMax = value; OnPropertyChanged(); }
        }

        public float TractionCurveMin
        {
            get => _selectedHandling?.TractionCurveMin ?? 0.8f;
            set { if (_selectedHandling != null) _selectedHandling.TractionCurveMin = value; OnPropertyChanged(); }
        }

        // Advanced settings section
        public float Downforce
        {
            get => _selectedHandling?.Downforce ?? 0.3f;
            set { if (_selectedHandling != null) _selectedHandling.Downforce = value; OnPropertyChanged(); }
        }

        public float RollCentreHeightFront
        {
            get => _selectedHandling?.RollCentreHeightFront ?? 0.5f;
            set { if (_selectedHandling != null) _selectedHandling.RollCentreHeightFront = value; OnPropertyChanged(); }
        }

        public float RollCentreHeightRear
        {
            get => _selectedHandling?.RollCentreHeightRear ?? 0.5f;
            set { if (_selectedHandling != null) _selectedHandling.RollCentreHeightRear = value; OnPropertyChanged(); }
        }

        // Commands
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExportCommand { get; }

        public HandlingViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            LoadCommand = new RelayCommand(ExecuteLoad);
            SaveCommand = new RelayCommand(ExecuteSave);
            ExportCommand = new RelayCommand(ExecuteExport);
        }

        private void ExecuteLoad()
        {
            try
            {
                var filePath = FileService.OpenFileDialog("handling");
                if (filePath == null) return;

                ShowInfo("Loading handling.meta...");
                IsLoading = true;

                var (success, handlingEntries, error) = _handlingService.LoadHandlingMeta(filePath);

                if (success && handlingEntries != null)
                {
                    _allHandlingEntries = handlingEntries; // store master list
                    HandlingEntries.Clear();
                    foreach (var entry in handlingEntries)
                        HandlingEntries.Add(entry);

                    OnSearchChanged(); // Apply current filter
                    _currentFilePath = filePath;

                    ShowSuccess($"Loaded {handlingEntries.Count} handling entries from {Path.GetFileName(filePath)}");
                }
                else
                {
                    ShowError(error ?? "Failed to load handling.meta");
                    FileService.ShowError("Load Error", error ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error loading file: {ex.Message}");
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
                if (HandlingEntries.Count == 0)
                {
                    ShowError("No handling entries to save");
                    return;
                }

                var filePath = string.IsNullOrEmpty(_currentFilePath)
                    ? FileService.SaveFileDialog("handling", "handling.meta")
                    : _currentFilePath;

                if (filePath == null) return;

                ShowInfo("Saving handling.meta...");
                IsLoading = true;

                var (success, error) = _handlingService.SaveHandlingMeta(filePath, HandlingEntries.ToList());

                if (success)
                {
                    _currentFilePath = filePath;
                    ShowSuccess($"Saved {HandlingEntries.Count} handling entries to {Path.GetFileName(filePath)}");
                }
                else
                {
                    ShowError($"Error saving: {error}");
                    FileService.ShowError("Save Error", error ?? "Unknown error");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error saving file: {ex.Message}");
                FileService.ShowError("Save Error", ex.Message);
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
                if (HandlingEntries.Count == 0)
                {
                    ShowError("No handling entries to export");
                    return;
                }

                var filePath = FileService.SaveFileDialog("json", "handling_export.json");
                if (filePath == null) return;

                ShowInfo("Exporting handling data to JSON...");
                IsLoading = true;

                var handlingList = new List<HandlingData>(HandlingEntries);
                var jsonData = JObject.FromObject(new { handlingEntries = handlingList });

                File.WriteAllText(filePath, jsonData.ToString());
                ShowSuccess($"Exported {HandlingEntries.Count} handling entries to {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                ShowError($"Error exporting: {ex.Message}");
                FileService.ShowError("Export Error", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnSearchChanged()
        {
            var filtered = _handlingService.FilterHandling(_allHandlingEntries, SearchText);
            HandlingEntries.Clear();
            foreach (var item in filtered)
                HandlingEntries.Add(item);
        }

        public void LoadHandlingFile(string filePath)
        {
            try
            {
                var (success, handlingList, error) = _handlingService.LoadHandlingMeta(filePath);
                if (!success || handlingList == null)
                {
                    ShowError($"Error loading handling.meta: {error}");
                    return;
                }

                _currentFilePath = filePath;
                HandlingEntries.Clear();
                foreach (var item in handlingList)
                    HandlingEntries.Add(item);

                ShowSuccess($"Loaded {handlingList.Count} handling entries");
            }
            catch (Exception ex)
            {
                ShowError($"Exception loading file: {ex.Message}");
            }
        }
    }
}
