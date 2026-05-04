using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FiveMVehicleMetaEditorWPF.Core.Services
{
    /// <summary>
    /// Manages file backups with timestamped versions (max 5 per file)
    /// </summary>
    public class BackupManager
    {
        private readonly string _backupDir;
        private const int MaxBackupsPerFile = 5;

        public BackupManager(string backupDirectory = ".backups")
        {
            _backupDir = backupDirectory;
            if (!Directory.Exists(_backupDir))
                Directory.CreateDirectory(_backupDir);
        }

        /// <summary>
        /// Create a backup before saving
        /// </summary>
        public bool CreateBackup(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                var fileName = Path.GetFileName(filePath);
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var backupName = $"{fileName.Replace(".", "_")}_{timestamp}.bak";
                var backupPath = Path.Combine(_backupDir, backupName);

                File.Copy(filePath, backupPath, true);

                // Clean old backups
                CleanupOldBackups(fileName);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get list of available backups for a file
        /// </summary>
        public List<(string BackupPath, DateTime DateTime)> GetAvailableBackups(string fileName)
        {
            try
            {
                var baseName = fileName.Replace(".", "_");
                var backupFiles = Directory.GetFiles(_backupDir, $"{baseName}_*.bak")
                    .OrderByDescending(File.GetLastWriteTime)
                    .ToList();

                var result = new List<(string, DateTime)>();
                foreach (var file in backupFiles)
                {
                    var lastWrite = File.GetLastWriteTime(file);
                    result.Add((file, lastWrite));
                }

                return result;
            }
            catch
            {
                return new List<(string, DateTime)>();
            }
        }

        /// <summary>
        /// Restore from backup
        /// </summary>
        public bool RestoreBackup(string backupPath, string targetPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return false;

                // Create backup of current file first
                if (File.Exists(targetPath))
                    CreateBackup(targetPath);

                File.Copy(backupPath, targetPath, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Remove old backups (keep max 5)
        /// </summary>
        private void CleanupOldBackups(string fileName)
        {
            try
            {
                var baseName = fileName.Replace(".", "_");
                var backupFiles = Directory.GetFiles(_backupDir, $"{baseName}_*.bak")
                    .OrderByDescending(File.GetLastWriteTime)
                    .ToList();

                if (backupFiles.Count > MaxBackupsPerFile)
                {
                    for (int i = MaxBackupsPerFile; i < backupFiles.Count; i++)
                    {
                        File.Delete(backupFiles[i]);
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        /// <summary>
        /// Delete all backups for a file
        /// </summary>
        public void ClearBackups(string fileName)
        {
            try
            {
                var baseName = fileName.Replace(".", "_");
                var backupFiles = Directory.GetFiles(_backupDir, $"{baseName}_*.bak");
                foreach (var file in backupFiles)
                {
                    File.Delete(file);
                }
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}
