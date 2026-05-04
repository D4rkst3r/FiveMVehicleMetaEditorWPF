using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Models;
using FiveMVehicleMetaEditorWPF.Core.Services;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class MergerViewModel : BaseTabViewModel
    {
        private readonly MetaVehiclesService _vehiclesService = new();
        private int _totalVehicles = 0;
        private string? _selectedFile;

        public ObservableCollection<string> FilesToMerge { get; } = new();

        public int TotalVehicles
        {
            get => _totalVehicles;
            set { _totalVehicles = value; OnPropertyChanged(); }
        }

        public string? SelectedFile
        {
            get => _selectedFile;
            set { _selectedFile = value; OnPropertyChanged(); }
        }

        public new ICommand LoadCommand { get; }
        public ICommand RemoveFileCommand { get; }
        public ICommand ClearCommand { get; }
        public ICommand MergeCommand { get; }

        public MergerViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            LoadCommand = new RelayCommand(ExecuteAddFiles);
            RemoveFileCommand = new RelayCommand(ExecuteRemoveFile);
            ClearCommand = new RelayCommand(ExecuteClear);
            MergeCommand = new RelayCommand(ExecuteMerge);

            ShowInfo("Add vehicles.meta files to merge them into one");
        }

        private void ExecuteAddFiles()
        {
            try
            {
                var files = FileService.OpenMultipleFilesDialog("vehicles");
                if (files == null || files.Count == 0) return;

                foreach (var file in files)
                {
                    if (!FilesToMerge.Contains(file))
                        FilesToMerge.Add(file);
                }

                UpdateTotalCount();
                ShowInfo($"{FilesToMerge.Count} files selected for merging");
            }
            catch (Exception ex)
            {
                ShowError($"Error adding files: {ex.Message}");
            }
        }

        private void ExecuteRemoveFile()
        {
            if (SelectedFile != null && FilesToMerge.Contains(SelectedFile))
            {
                FilesToMerge.Remove(SelectedFile);
                SelectedFile = null;
                UpdateTotalCount();
                ShowInfo($"{FilesToMerge.Count} files remaining");
            }
        }

        private void ExecuteClear()
        {
            FilesToMerge.Clear();
            TotalVehicles = 0;
            ShowInfo("File list cleared");
        }

        private void ExecuteMerge()
        {
            try
            {
                if (FilesToMerge.Count == 0)
                {
                    ShowError("No files to merge. Add files first.");
                    return;
                }

                IsLoading = true;
                ShowInfo("Merging files...");

                var allVehicles = new List<Vehicle>();
                var seenModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                int skipped = 0;

                foreach (var filePath in FilesToMerge)
                {
                    var (success, vehicles, error) = _vehiclesService.LoadVehiclesMeta(filePath);
                    if (!success || vehicles == null)
                    {
                        ShowError($"Error loading {Path.GetFileName(filePath)}: {error}");
                        return;
                    }

                    foreach (var v in vehicles)
                    {
                        if (seenModels.Add(v.ModelName))
                            allVehicles.Add(v);
                        else
                            skipped++;
                    }
                }

                var savePath = FileService.SaveFileDialog("vehicles", "vehicles_merged.meta");
                if (savePath == null) return;

                var (saveSuccess, saveError) = _vehiclesService.SaveVehiclesMeta(savePath, allVehicles);
                if (saveSuccess)
                    ShowSuccess($"Merged {allVehicles.Count} vehicles into {Path.GetFileName(savePath)} ({skipped} duplicates skipped)");
                else
                    ShowError($"Error saving: {saveError}");
            }
            catch (Exception ex)
            {
                ShowError($"Merge error: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateTotalCount()
        {
            TotalVehicles = 0;
            foreach (var file in FilesToMerge)
            {
                try
                {
                    var (success, vehicles, _) = _vehiclesService.LoadVehiclesMeta(file);
                    if (success && vehicles != null)
                        TotalVehicles += vehicles.Count;
                }
                catch { }
            }
        }
    }
}
