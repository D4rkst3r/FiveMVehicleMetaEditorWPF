using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Xml.Linq;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Services;
using Newtonsoft.Json.Linq;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    /// <summary>
    /// ViewModel for Validator Tab - validates meta file integrity
    /// </summary>
    public class ValidatorViewModel : BaseTabViewModel
    {
        private ObservableCollection<string> _validationResults = new();
        public ObservableCollection<string> ValidationResults
        {
            get => _validationResults;
            set { _validationResults = value; OnPropertyChanged(); }
        }

        private string _selectedFilePath = "";
        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set { _selectedFilePath = value; OnPropertyChanged(); }
        }

        public ICommand LoadCommand { get; }
        public ICommand ValidateCommand { get; }
        public ICommand ExportCommand { get; }

        public ValidatorViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            LoadCommand = new RelayCommand(ExecuteLoad);
            ValidateCommand = new RelayCommand(ExecuteValidate);
            ExportCommand = new RelayCommand(ExecuteExport);
        }

        private void ExecuteLoad()
        {
            try
            {
                var filePath = FileService.OpenFileDialog("meta");
                if (filePath == null) return;

                ShowInfo("Loading meta file for validation...");
                IsLoading = true;

                // Validate the file exists and is readable
                if (!File.Exists(filePath))
                {
                    ShowError("File not found");
                    return;
                }

                // Try to parse as XML to validate structure
                try
                {
                    var doc = JObject.Parse(File.ReadAllText(filePath));
                    _selectedFilePath = filePath;
                    ValidationResults.Clear();
                    ShowSuccess($"Loaded {Path.GetFileName(filePath)} - ready to validate");
                }
                catch
                {
                    // File might be XML, try parsing as XML
                    try
                    {
                        var xmlContent = File.ReadAllText(filePath);
                        var doc = System.Xml.Linq.XDocument.Parse(xmlContent);
                        _selectedFilePath = filePath;
                        ValidationResults.Clear();
                        ShowSuccess($"Loaded {Path.GetFileName(filePath)} - ready to validate");
                    }
                    catch (Exception ex)
                    {
                        ShowError($"File is not valid JSON or XML: {ex.Message}");
                    }
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

        private void ExecuteValidate()
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedFilePath))
                {
                    ShowError("No file selected. Click Load first.");
                    return;
                }

                ValidationResults.Clear();
                IsLoading = true;
                ShowInfo("Validating...");

                var content = File.ReadAllText(_selectedFilePath);

                // 1. XML structure check
                System.Xml.Linq.XDocument doc;
                try
                {
                    doc = System.Xml.Linq.XDocument.Parse(content);
                    ValidationResults.Add("✓ XML structure is valid");
                }
                catch (Exception ex)
                {
                    ValidationResults.Add($"✗ XML parse error: {ex.Message}");
                    ShowError("File has XML errors");
                    return;
                }

                var root = doc.Root;
                var rootName = root?.Name.LocalName ?? "(unknown)";
                ValidationResults.Add($"✓ Root element: <{rootName}>");

                // 2. Detect file type and validate accordingly
                var fileName = Path.GetFileName(_selectedFilePath).ToLower();

                if (rootName == "CVehicleModelInfo__InitDataList" || fileName.Contains("vehicles"))
                {
                    var items = root == null ? new List<XElement>()
                        : root.Descendants("InitDatas").Elements("Item").ToList();
                    ValidationResults.Add($"✓ Found {items.Count} vehicle entries");

                    int missingModel = items.Count(i => string.IsNullOrWhiteSpace(i.Element("modelName")?.Value));
                    if (missingModel > 0) ValidationResults.Add($"⚠ {missingModel} entries missing modelName");
                    else ValidationResults.Add("✓ All entries have modelName");

                    int missingHandling = items.Count(i => string.IsNullOrWhiteSpace(i.Element("handlingId")?.Value));
                    if (missingHandling > 0) ValidationResults.Add($"⚠ {missingHandling} entries missing handlingId");
                    else ValidationResults.Add("✓ All entries have handlingId");

                    var names = items.Select(i => i.Element("modelName")?.Value).Where(n => n != null).ToList();
                    var dupes = names.GroupBy(n => n).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
                    if (dupes.Count > 0) ValidationResults.Add($"✗ Duplicate modelNames: {string.Join(", ", dupes)}");
                    else ValidationResults.Add("✓ No duplicate modelNames");
                }
                else if (rootName == "CHandlingDataMgr" || fileName.Contains("handling"))
                {
                    var items = root == null ? new List<XElement>()
                        : root.Descendants("HandlingData").Elements("Item").ToList();
                    ValidationResults.Add($"✓ Found {items.Count} handling entries");
                    int missing = items.Count(i => string.IsNullOrWhiteSpace(i.Element("handlingName")?.Value));
                    if (missing > 0) ValidationResults.Add($"⚠ {missing} entries missing handlingName");
                    else ValidationResults.Add("✓ All entries have handlingName");
                }
                else if (rootName == "CVehicleModelInfoVarGlobal" || fileName.Contains("carcols"))
                {
                    var kits = root == null ? new List<XElement>()
                        : root.Descendants("Kits").Elements("Item").ToList();
                    ValidationResults.Add($"✓ Found {kits.Count} mod kits");
                    int missing = kits.Count(i => string.IsNullOrWhiteSpace(i.Element("kitName")?.Value));
                    if (missing > 0) ValidationResults.Add($"⚠ {missing} kits missing kitName");
                    else ValidationResults.Add("✓ All kits have kitName");
                }
                else if (rootName == "CVehicleModelInfoVariation" || fileName.Contains("carvariations"))
                {
                    var items = root == null ? new List<XElement>()
                        : root.Descendants("variationData").Elements("Item").ToList();
                    ValidationResults.Add($"✓ Found {items.Count} vehicle variations");
                }
                else if (rootName == "CVehicleMetadataMgr" || fileName.Contains("layout"))
                {
                    var items = root == null ? new List<XElement>()
                        : root.Descendants("AnimRateSets").Elements("Item").ToList();
                    ValidationResults.Add($"✓ Found {items.Count} anim rate sets");
                }
                else
                {
                    ValidationResults.Add($"ℹ Unknown file type: {rootName}");
                    var allItems = root?.Descendants("Item").Count() ?? 0;
                    ValidationResults.Add($"ℹ Total <Item> elements: {allItems}");
                }

                // 3. File size info
                var fileSize = new FileInfo(_selectedFilePath).Length;
                ValidationResults.Add($"ℹ File size: {fileSize / 1024.0:F1} KB");

                ShowSuccess($"Validation complete — {ValidationResults.Count} checks");
            }
            catch (Exception ex)
            {
                ValidationResults.Add($"✗ Validation error: {ex.Message}");
                ShowError($"Validation error: {ex.Message}");
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
                if (ValidationResults.Count == 0)
                {
                    ShowError("No validation results to export");
                    return;
                }

                var filePath = FileService.SaveFileDialog("txt", "validation_report.txt");
                if (filePath == null) return;

                ShowInfo("Exporting validation report...");
                IsLoading = true;

                var reportContent = new System.Text.StringBuilder();
                reportContent.AppendLine("=== VALIDATION REPORT ===");
                reportContent.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                if (!string.IsNullOrEmpty(_selectedFilePath))
                {
                    reportContent.AppendLine($"File: {Path.GetFileName(_selectedFilePath)}");
                }
                reportContent.AppendLine("=========================");
                reportContent.AppendLine();

                foreach (var result in ValidationResults)
                {
                    reportContent.AppendLine(result);
                }

                File.WriteAllText(filePath, reportContent.ToString());
                ShowSuccess($"Exported validation report to {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                ShowError($"Error exporting: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
