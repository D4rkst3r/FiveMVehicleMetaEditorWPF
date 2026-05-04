using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Win32;

namespace FiveMVehicleMetaEditorWPF.Core.Services
{
    /// <summary>
    /// Service for file dialogs and file operations
    /// </summary>
    public class FileService
    {
        /// <summary>
        /// Shows an open file dialog for meta files
        /// </summary>
        /// <param name="fileType">Type of file to open (vehicles, handling, layouts, etc.)</param>
        /// <returns>Selected file path or null if cancelled</returns>
        public static string? OpenFileDialog(string fileType = "Meta")
        {
            var dialog = new OpenFileDialog
            {
                Title = $"Open {fileType} File",
                Filter = GetFileFilter(fileType),
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        /// <summary>
        /// Shows a save file dialog for meta files
        /// </summary>
        /// <param name="fileType">Type of file to save</param>
        /// <param name="defaultFileName">Default file name</param>
        /// <returns>Selected file path or null if cancelled</returns>
        public static string? SaveFileDialog(string fileType = "Meta", string defaultFileName = "")
        {
            var dialog = new SaveFileDialog
            {
                Title = $"Save {fileType} File",
                Filter = GetFileFilter(fileType),
                FileName = defaultFileName,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        /// <summary>
        /// Shows a file browser dialog for selecting multiple files
        /// </summary>
        /// <param name="fileType">Type of files to select</param>
        /// <returns>List of selected file paths or empty list if cancelled</returns>
        public static List<string> OpenMultipleFilesDialog(string fileType = "Meta")
        {
            var dialog = new OpenFileDialog
            {
                Title = $"Open {fileType} Files",
                Filter = GetFileFilter(fileType),
                Multiselect = true,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            var result = new List<string>();
            if (dialog.ShowDialog() == true)
            {
                result.AddRange(dialog.FileNames);
            }
            return result;
        }


        /// <summary>
        /// Gets the file filter string for different file types
        /// </summary>
        private static string GetFileFilter(string fileType) => fileType.ToLower() switch
        {
            "vehicles" => "Vehicle Meta Files (vehicles.meta)|vehicles.meta|XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            "handling" => "Handling Files (handling.meta)|handling.meta|XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            "layouts" => "Layout Files (vehiclelayouts.meta)|vehiclelayouts.meta|XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            "carvariations" => "Car Variations Files (carvariations.meta)|carvariations.meta|XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            "carcols" => "Car Colors Files (carcols.meta)|carcols.meta|XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            "json" => "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            "meta" => "Meta Files (*.meta)|*.meta|XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
            _ => "All Files (*.*)|*.*"
        };

        /// <summary>
        /// Shows an error message dialog
        /// </summary>
        public static void ShowError(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Shows a success message dialog
        /// </summary>
        public static void ShowSuccess(string title, string message)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Shows a confirmation dialog
        /// </summary>
        /// <returns>True if user clicks Yes, false otherwise</returns>
        public static bool ShowConfirmation(string title, string message)
        {
            return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes;
        }
    }
}
