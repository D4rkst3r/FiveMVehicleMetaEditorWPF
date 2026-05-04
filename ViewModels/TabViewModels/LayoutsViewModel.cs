using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Models;
using FiveMVehicleMetaEditorWPF.Core.Services;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    /// <summary>
    /// ViewModel for Layouts Tab (vehiclelayouts.meta editor)
    /// </summary>
    public class LayoutsViewModel : BaseTabViewModel
    {
        private readonly MetaLayoutsService _layoutsService = new();
        private string _currentFilePath = "";

        // Observable collection of layout entries
        private ObservableCollection<LayoutData> _layouts = new();
        public ObservableCollection<LayoutData> Layouts
        {
            get => _layouts;
            set { _layouts = value; OnPropertyChanged(); }
        }

        // Selected layout entry
        private LayoutData? _selectedLayout;
        public LayoutData? SelectedLayout
        {
            get => _selectedLayout;
            set { _selectedLayout = value; OnPropertyChanged(); UpdateLayoutProperties(); }
        }

        // Search text for filtering
        private string _searchText = "";
        public new string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                OnSearchChanged();
            }
        }

        // Layout properties
        private string _layoutName = "";
        public string LayoutName
        {
            get => _selectedLayout?.LayoutName ?? _layoutName;
            set { if (_selectedLayout != null) _selectedLayout.LayoutName = value; OnPropertyChanged(); }
        }

        private string _category = "Standard Vehicles";
        public string Category
        {
            get => _selectedLayout?.Category ?? _category;
            set { if (_selectedLayout != null) _selectedLayout.Category = value; OnPropertyChanged(); }
        }

        private string _seatsList = "";
        public string SeatsList
        {
            get => _seatsList;
            set { _seatsList = value; OnPropertyChanged(); }
        }

        private string _doorsList = "";
        public string DoorsList
        {
            get => _doorsList;
            set { _doorsList = value; OnPropertyChanged(); }
        }

        private bool _hasRoof = true;
        public bool HasRoof
        {
            get => _selectedLayout?.HasRoof ?? _hasRoof;
            set { if (_selectedLayout != null) _selectedLayout.HasRoof = value; OnPropertyChanged(); }
        }

        private bool _hasTrunk = true;
        public bool HasTrunk
        {
            get => _selectedLayout?.HasTrunk ?? _hasTrunk;
            set { if (_selectedLayout != null) _selectedLayout.HasTrunk = value; OnPropertyChanged(); }
        }

        private bool _hasHood = true;
        public bool HasHood
        {
            get => _selectedLayout?.HasHood ?? _hasHood;
            set { if (_selectedLayout != null) _selectedLayout.HasHood = value; OnPropertyChanged(); }
        }

        // Commands
        public new ICommand LoadCommand { get; }
        public new ICommand SaveCommand { get; }
        public new ICommand ExportCommand { get; }

        public LayoutsViewModel()
        {
            LoadCommand = new RelayCommand(ExecuteLoad);
            SaveCommand = new RelayCommand(ExecuteSave);
            ExportCommand = new RelayCommand(ExecuteExport);
        }

        private void ExecuteLoad()
        {
            try
            {
                ShowInfo("Select a vehiclelayouts.meta file to load...");
                // TODO: Implement file dialog
                ShowSuccess("Load vehiclelayouts.meta (placeholder)");
            }
            catch (Exception ex)
            {
                ShowError($"Error loading file: {ex.Message}");
            }
        }

        private void ExecuteSave()
        {
            try
            {
                if (string.IsNullOrEmpty(_currentFilePath))
                {
                    ShowError("No file loaded");
                    return;
                }

                var (success, error) = _layoutsService.SaveLayoutsMeta(_currentFilePath, Layouts.ToList());
                if (success)
                    ShowSuccess("Layout data saved successfully");
                else
                    ShowError($"Error saving: {error}");
            }
            catch (Exception ex)
            {
                ShowError($"Error saving file: {ex.Message}");
            }
        }

        private void ExecuteExport()
        {
            try
            {
                ShowInfo("Select location to export layout data...");
                // TODO: Implement export functionality
                ShowSuccess("Export layout data (placeholder)");
            }
            catch (Exception ex)
            {
                ShowError($"Error exporting: {ex.Message}");
            }
        }

        private void OnSearchChanged()
        {
            var allLayouts = Layouts.ToList();
            var filtered = _layoutsService.FilterLayouts(allLayouts, SearchText);

            Layouts.Clear();
            foreach (var item in filtered)
                Layouts.Add(item);
        }

        private void UpdateLayoutProperties()
        {
            if (_selectedLayout == null)
            {
                SeatsList = "";
                DoorsList = "";
                return;
            }

            // Format seats list
            SeatsList = string.Join(", ", _selectedLayout.Seats.Select(s => $"{s.Value}({s.Key})"));
            // Format doors list
            DoorsList = string.Join(", ", _selectedLayout.Doors.Select(d => $"{d.Value}({d.Key})"));
        }

        public void LoadLayoutsFile(string filePath)
        {
            try
            {
                var (success, layoutList, error) = _layoutsService.LoadLayoutsMeta(filePath);
                if (!success || layoutList == null)
                {
                    ShowError($"Error loading vehiclelayouts.meta: {error}");
                    return;
                }

                _currentFilePath = filePath;
                Layouts.Clear();
                foreach (var item in layoutList)
                    Layouts.Add(item);

                ShowSuccess($"Loaded {layoutList.Count} layout entries");
            }
            catch (Exception ex)
            {
                ShowError($"Exception loading file: {ex.Message}");
            }
        }
    }
}
