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
            set
            {
                _selectedLayout = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LayoutName));
                OnPropertyChanged(nameof(Category));
                OnPropertyChanged(nameof(HasRoof));
                OnPropertyChanged(nameof(HasTrunk));
                OnPropertyChanged(nameof(HasHood));
                OnPropertyChanged(nameof(NormalEntrySP));
                OnPropertyChanged(nameof(NormalEntryMP));
                OnPropertyChanged(nameof(AnimCombatEntrySP));
                OnPropertyChanged(nameof(AnimCombatEntryMP));
                OnPropertyChanged(nameof(ForcedEntrySP));
                OnPropertyChanged(nameof(ForcedEntryMP));
                OnPropertyChanged(nameof(NormalExitSP));
                OnPropertyChanged(nameof(NormalExitMP));
                UpdateLayoutProperties();
            }
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

        // AnimRate properties (forwarded to the selected LayoutData model)
        public float NormalEntrySP
        {
            get => _selectedLayout?.NormalEntrySP ?? 1.0f;
            set { if (_selectedLayout != null) _selectedLayout.NormalEntrySP = value; OnPropertyChanged(); }
        }
        public float NormalEntryMP
        {
            get => _selectedLayout?.NormalEntryMP ?? 1.5f;
            set { if (_selectedLayout != null) _selectedLayout.NormalEntryMP = value; OnPropertyChanged(); }
        }
        public float AnimCombatEntrySP
        {
            get => _selectedLayout?.AnimCombatEntrySP ?? 1.0f;
            set { if (_selectedLayout != null) _selectedLayout.AnimCombatEntrySP = value; OnPropertyChanged(); }
        }
        public float AnimCombatEntryMP
        {
            get => _selectedLayout?.AnimCombatEntryMP ?? 1.5f;
            set { if (_selectedLayout != null) _selectedLayout.AnimCombatEntryMP = value; OnPropertyChanged(); }
        }
        public float ForcedEntrySP
        {
            get => _selectedLayout?.ForcedEntrySP ?? 1.2f;
            set { if (_selectedLayout != null) _selectedLayout.ForcedEntrySP = value; OnPropertyChanged(); }
        }
        public float ForcedEntryMP
        {
            get => _selectedLayout?.ForcedEntryMP ?? 1.35f;
            set { if (_selectedLayout != null) _selectedLayout.ForcedEntryMP = value; OnPropertyChanged(); }
        }
        public float NormalExitSP
        {
            get => _selectedLayout?.NormalExitSP ?? 1.0f;
            set { if (_selectedLayout != null) _selectedLayout.NormalExitSP = value; OnPropertyChanged(); }
        }
        public float NormalExitMP
        {
            get => _selectedLayout?.NormalExitMP ?? 1.35f;
            set { if (_selectedLayout != null) _selectedLayout.NormalExitMP = value; OnPropertyChanged(); }
        }

        // Commands
        public new ICommand LoadCommand { get; }
        public new ICommand SaveCommand { get; }
        public new ICommand ExportCommand { get; }

        public LayoutsViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            LoadCommand = new RelayCommand(ExecuteLoad);
            SaveCommand = new RelayCommand(ExecuteSave);
            ExportCommand = new RelayCommand(ExecuteExport);
        }

        private void ExecuteLoad()
        {
            try
            {
                var filePath = FileService.OpenFileDialog("layouts");
                if (filePath == null) return;

                ShowInfo("Loading vehiclelayouts.meta...");
                IsLoading = true;

                var (success, layouts, error) = _layoutsService.LoadLayoutsMeta(filePath);

                if (success && layouts != null)
                {
                    Layouts.Clear();
                    foreach (var layout in layouts)
                        Layouts.Add(layout);

                    OnSearchChanged(); // Apply current filter
                    _currentFilePath = filePath;

                    ShowSuccess($"Loaded {layouts.Count} layouts from {Path.GetFileName(filePath)}");
                }
                else
                {
                    ShowError(error ?? "Failed to load vehiclelayouts.meta");
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
                if (Layouts.Count == 0)
                {
                    ShowError("No layouts to save");
                    return;
                }

                var filePath = string.IsNullOrEmpty(_currentFilePath)
                    ? FileService.SaveFileDialog("layouts", "vehiclelayouts.meta")
                    : _currentFilePath;

                if (filePath == null) return;

                ShowInfo("Saving vehiclelayouts.meta...");
                IsLoading = true;

                var (success, error) = _layoutsService.SaveLayoutsMeta(filePath, Layouts.ToList());

                if (success)
                {
                    _currentFilePath = filePath;
                    ShowSuccess($"Saved {Layouts.Count} layouts to {Path.GetFileName(filePath)}");
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
                if (Layouts.Count == 0)
                {
                    ShowError("No layouts to export");
                    return;
                }

                var filePath = FileService.SaveFileDialog("json", "layouts_export.json");
                if (filePath == null) return;

                ShowInfo("Exporting layouts to JSON...");
                IsLoading = true;

                var layoutList = new List<LayoutData>(Layouts);
                var jsonData = JObject.FromObject(new { layouts = layoutList });

                File.WriteAllText(filePath, jsonData.ToString());
                ShowSuccess($"Exported {Layouts.Count} layouts to {Path.GetFileName(filePath)}");
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
