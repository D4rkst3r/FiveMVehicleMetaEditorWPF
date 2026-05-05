using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Models;
using FiveMVehicleMetaEditorWPF.Core.Services;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class DiffViewModel : BaseTabViewModel
    {
        private readonly MetaVehiclesService _metaService = new();
        private string _fileAPath = "";
        private string _fileBPath = "";

        public string FileAPath
        {
            get => _fileAPath;
            set { _fileAPath = value; OnPropertyChanged(); OnPropertyChanged(nameof(FileAName)); }
        }

        public string FileBPath
        {
            get => _fileBPath;
            set { _fileBPath = value; OnPropertyChanged(); OnPropertyChanged(nameof(FileBName)); }
        }

        public string FileAName => string.IsNullOrEmpty(_fileAPath) ? "No file selected" : System.IO.Path.GetFileName(_fileAPath);
        public string FileBName => string.IsNullOrEmpty(_fileBPath) ? "No file selected" : System.IO.Path.GetFileName(_fileBPath);

        // Results
        public ObservableCollection<string> OnlyInA { get; } = new();
        public ObservableCollection<string> OnlyInB { get; } = new();
        public ObservableCollection<string> ChangedInBoth { get; } = new();
        public ObservableCollection<string> SameInBoth { get; } = new();

        private int _totalA = 0;
        private int _totalB = 0;

        public int TotalA
        {
            get => _totalA;
            set { _totalA = value; OnPropertyChanged(); }
        }
        public int TotalB
        {
            get => _totalB;
            set { _totalB = value; OnPropertyChanged(); }
        }

        public ICommand SelectFileACommand { get; }
        public ICommand SelectFileBCommand { get; }
        public ICommand RunDiffCommand { get; }

        public DiffViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            SelectFileACommand = new RelayCommand(_ => SelectFile("A"));
            SelectFileBCommand = new RelayCommand(_ => SelectFile("B"));
            RunDiffCommand = new RelayCommand(_ => ExecuteDiff(),
                _ => !string.IsNullOrEmpty(_fileAPath) && !string.IsNullOrEmpty(_fileBPath));

            ShowInfo("Select two vehicles.meta files to compare");
        }

        private void SelectFile(string slot)
        {
            var path = FileService.OpenFileDialog("vehicles");
            if (path == null) return;
            if (slot == "A") FileAPath = path;
            else FileBPath = path;
            ShowInfo($"File {slot}: {System.IO.Path.GetFileName(path)}");
        }

        private void ExecuteDiff()
        {
            try
            {
                IsLoading = true;
                ShowInfo("Comparing files...");

                var (okA, vehiclesA, errA) = _metaService.LoadVehiclesMeta(_fileAPath);
                var (okB, vehiclesB, errB) = _metaService.LoadVehiclesMeta(_fileBPath);

                if (!okA || vehiclesA == null) { ShowError($"Error loading File A: {errA}"); return; }
                if (!okB || vehiclesB == null) { ShowError($"Error loading File B: {errB}"); return; }

                TotalA = vehiclesA.Count;
                TotalB = vehiclesB.Count;

                var dictA = vehiclesA.ToDictionary(v => v.ModelName.ToLowerInvariant());
                var dictB = vehiclesB.ToDictionary(v => v.ModelName.ToLowerInvariant());

                OnlyInA.Clear();
                OnlyInB.Clear();
                ChangedInBoth.Clear();
                SameInBoth.Clear();

                // Only in A
                foreach (var key in dictA.Keys.Except(dictB.Keys))
                    OnlyInA.Add(dictA[key].ModelName);

                // Only in B
                foreach (var key in dictB.Keys.Except(dictA.Keys))
                    OnlyInB.Add(dictB[key].ModelName);

                // In both — compare
                foreach (var key in dictA.Keys.Intersect(dictB.Keys))
                {
                    var a = dictA[key];
                    var b = dictB[key];
                    var diffs = GetDiffs(a, b);
                    if (diffs.Count > 0)
                        ChangedInBoth.Add($"{a.ModelName}  [{string.Join(", ", diffs)}]");
                    else
                        SameInBoth.Add(a.ModelName);
                }

                ShowSuccess($"Done — {OnlyInA.Count} only in A, {OnlyInB.Count} only in B, {ChangedInBoth.Count} changed, {SameInBoth.Count} identical");
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

        private static List<string> GetDiffs(Vehicle a, Vehicle b)
        {
            var diffs = new List<string>();
            if (a.VehicleClass != b.VehicleClass) diffs.Add($"Class: {a.VehicleClass}→{b.VehicleClass}");
            if (a.VehicleType != b.VehicleType) diffs.Add($"Type: {a.VehicleType}→{b.VehicleType}");
            if (a.HandlingId != b.HandlingId) diffs.Add($"HandlingID: {a.HandlingId}→{b.HandlingId}");
            if (a.Seats != b.Seats) diffs.Add($"Seats: {a.Seats}→{b.Seats}");
            if (a.DefaultBodyHealth != b.DefaultBodyHealth) diffs.Add($"Health: {a.DefaultBodyHealth}→{b.DefaultBodyHealth}");
            if (a.Layout != b.Layout) diffs.Add($"Layout: {a.Layout}→{b.Layout}");
            if (a.VehicleMakeName != b.VehicleMakeName) diffs.Add($"Make: {a.VehicleMakeName}→{b.VehicleMakeName}");
            if (a.GameName != b.GameName) diffs.Add($"Name: {a.GameName}→{b.GameName}");
            if (a.Frequency != b.Frequency) diffs.Add($"Freq: {a.Frequency}→{b.Frequency}");
            // Flags diff
            var flagsAdded = b.Flags.Except(a.Flags).ToList();
            var flagsRemoved = a.Flags.Except(b.Flags).ToList();
            if (flagsAdded.Count > 0) diffs.Add($"+flags: {string.Join(",", flagsAdded)}");
            if (flagsRemoved.Count > 0) diffs.Add($"-flags: {string.Join(",", flagsRemoved)}");
            return diffs;
        }
    }
}
