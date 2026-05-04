using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Services;
using Newtonsoft.Json;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    /// <summary>
    /// Model for a recent file entry (filename + full path)
    /// </summary>
    public class RecentFileItem
    {
        public string FileName { get; set; } = "";
        public string FullPath { get; set; } = "";

        public override string ToString() => FileName;
    }

    public class BrowserViewModel : BaseTabViewModel
    {
        private const int MaxRecentFiles = 10;
        private static readonly string RecentFilesPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FiveMVehicleMetaEditor",
            "recent_files.json");

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

        public ObservableCollection<RecentFileItem> RecentFiles { get; } = new();
        public ObservableCollection<string> Vehicles { get; } = new();

        public ICommand LoadVehiclesCommand { get; }
        public ICommand LoadHandlingCommand { get; }
        public ICommand LoadLayoutsCommand { get; }
        public ICommand OpenRecentFileCommand { get; }
        public ICommand NavigateToVehiclesCommand { get; }
        public ICommand NavigateToHandlingCommand { get; }
        public ICommand NavigateToLayoutsCommand { get; }

        public BrowserViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            LoadVehiclesCommand = new RelayCommand(_ => OnLoadVehicles());
            LoadHandlingCommand = new RelayCommand(_ => OnLoadHandling());
            LoadLayoutsCommand = new RelayCommand(_ => OnLoadLayouts());
            OpenRecentFileCommand = new RelayCommand(param =>
            {
                if (param is RecentFileItem item)
                    OpenRecentFile(item.FullPath);
                else if (param is string path)
                    OpenRecentFile(path);
            });
            NavigateToVehiclesCommand = new RelayCommand(_ => MainVM?.NavigateTo("vehicles"));
            NavigateToHandlingCommand = new RelayCommand(_ => MainVM?.NavigateTo("handling"));
            NavigateToLayoutsCommand = new RelayCommand(_ => MainVM?.NavigateTo("layouts"));

            LoadRecentFilesFromDisk();
            ShowInfo("Ready to load meta files");
        }

        // ── Recent Files ────────────────────────────────────────────────────────

        private void LoadRecentFilesFromDisk()
        {
            try
            {
                if (!File.Exists(RecentFilesPath)) return;

                var json = File.ReadAllText(RecentFilesPath);
                var paths = JsonConvert.DeserializeObject<string[]>(json);
                if (paths == null) return;

                RecentFiles.Clear();
                foreach (var path in paths)
                {
                    if (File.Exists(path))
                        RecentFiles.Add(new RecentFileItem
                        {
                            FileName = Path.GetFileName(path),
                            FullPath = path
                        });
                }
            }
            catch
            {
                // Silently ignore – recent files are non-critical
            }
        }

        private void AddToRecentFiles(string filePath)
        {
            // Remove duplicate if already present
            for (int i = RecentFiles.Count - 1; i >= 0; i--)
            {
                if (string.Equals(RecentFiles[i].FullPath, filePath, StringComparison.OrdinalIgnoreCase))
                    RecentFiles.RemoveAt(i);
            }

            // Insert at top
            RecentFiles.Insert(0, new RecentFileItem
            {
                FileName = Path.GetFileName(filePath),
                FullPath = filePath
            });

            // Trim to max
            while (RecentFiles.Count > MaxRecentFiles)
                RecentFiles.RemoveAt(RecentFiles.Count - 1);

            SaveRecentFilesToDisk();
        }

        private void SaveRecentFilesToDisk()
        {
            try
            {
                var dir = Path.GetDirectoryName(RecentFilesPath)!;
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var paths = new string[RecentFiles.Count];
                for (int i = 0; i < RecentFiles.Count; i++)
                    paths[i] = RecentFiles[i].FullPath;

                File.WriteAllText(RecentFilesPath, JsonConvert.SerializeObject(paths, Formatting.Indented));
            }
            catch
            {
                // Silently ignore
            }
        }

        // ── Load handlers ───────────────────────────────────────────────────────

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
                    AddToRecentFiles(filePath);
                    ShowSuccess($"Loaded {vehicles.Count} vehicles");
                    MainVM?.NavigateTo("vehicles");
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
                    SelectedFile = filePath;
                    AddToRecentFiles(filePath);
                    ShowSuccess($"Loaded {handlingData.Count} handling entries");
                    MainVM?.NavigateTo("handling");
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
                    AddToRecentFiles(filePath);
                    ShowSuccess($"Loaded {layouts.Count} layouts");
                    MainVM?.NavigateTo("layouts");
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

        // ── Open recent file ────────────────────────────────────────────────────

        private void OpenRecentFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                ShowError($"File not found: {Path.GetFileName(filePath)}");
                return;
            }

            var fileName = Path.GetFileName(filePath).ToLowerInvariant();

            if (fileName.Contains("handling"))
            {
                OnLoadHandlingFromPath(filePath);
            }
            else if (fileName.Contains("layout"))
            {
                OnLoadLayoutsFromPath(filePath);
            }
            else
            {
                // Default: treat as vehicles.meta
                OnLoadVehiclesFromPath(filePath);
            }
        }

        private void OnLoadVehiclesFromPath(string filePath)
        {
            try
            {
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
                    AddToRecentFiles(filePath);
                    ShowSuccess($"Loaded {vehicles.Count} vehicles");
                    MainVM?.NavigateTo("vehicles");
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
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnLoadHandlingFromPath(string filePath)
        {
            try
            {
                ShowInfo("Loading handling.meta...");
                IsLoading = true;

                var service = new MetaHandlingService();
                var (success, handlingData, error) = service.LoadHandlingMeta(filePath);

                if (success && handlingData != null)
                {
                    SelectedFile = filePath;
                    AddToRecentFiles(filePath);
                    ShowSuccess($"Loaded {handlingData.Count} handling entries");
                    MainVM?.NavigateTo("handling");
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
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OnLoadLayoutsFromPath(string filePath)
        {
            try
            {
                ShowInfo("Loading vehiclelayouts.meta...");
                IsLoading = true;

                var service = new MetaLayoutsService();
                var (success, layouts, error) = service.LoadLayoutsMeta(filePath);

                if (success && layouts != null)
                {
                    LayoutCount = layouts.Count;
                    SelectedFile = filePath;
                    AddToRecentFiles(filePath);
                    ShowSuccess($"Loaded {layouts.Count} layouts");
                    MainVM?.NavigateTo("layouts");
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
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
