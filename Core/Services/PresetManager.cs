using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace FiveMVehicleMetaEditorWPF.Core.Services
{
    /// <summary>
    /// Manages custom presets (saved configurations)
    /// </summary>
    public class PresetManager
    {
        private readonly string _presetsDir;

        public PresetManager(string presetsDirectory = "custom_presets")
        {
            _presetsDir = presetsDirectory;
            if (!Directory.Exists(_presetsDir))
                Directory.CreateDirectory(_presetsDir);
        }

        /// <summary>
        /// Save a preset (JSON)
        /// </summary>
        public bool SavePreset(string presetName, string presetType, JObject data)
        {
            try
            {
                var fileName = $"{presetType}_{presetName}.json";
                var filePath = Path.Combine(_presetsDir, fileName);

                var json = data.ToString();
                File.WriteAllText(filePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Load a preset
        /// </summary>
        public (bool Success, JObject? Data) LoadPreset(string presetName, string presetType)
        {
            try
            {
                var fileName = $"{presetType}_{presetName}.json";
                var filePath = Path.Combine(_presetsDir, fileName);

                if (!File.Exists(filePath))
                    return (false, null);

                var json = File.ReadAllText(filePath);
                var data = JObject.Parse(json);
                return (true, data);
            }
            catch
            {
                return (false, null);
            }
        }

        /// <summary>
        /// Get all preset names of a type
        /// </summary>
        public List<string> GetPresetNames(string presetType)
        {
            try
            {
                return Directory.GetFiles(_presetsDir, $"{presetType}_*.json")
                    .Select(f => Path.GetFileNameWithoutExtension(f))
                    .Select(f => f.Substring(presetType.Length + 1)) // Remove "type_" prefix
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Delete a preset
        /// </summary>
        public bool DeletePreset(string presetName, string presetType)
        {
            try
            {
                var fileName = $"{presetType}_{presetName}.json";
                var filePath = Path.Combine(_presetsDir, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Rename a preset
        /// </summary>
        public bool RenamePreset(string oldName, string newName, string presetType)
        {
            try
            {
                var oldFile = Path.Combine(_presetsDir, $"{presetType}_{oldName}.json");
                var newFile = Path.Combine(_presetsDir, $"{presetType}_{newName}.json");

                if (!File.Exists(oldFile))
                    return false;

                File.Move(oldFile, newFile, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if preset exists
        /// </summary>
        public bool PresetExists(string presetName, string presetType)
        {
            var filePath = Path.Combine(_presetsDir, $"{presetType}_{presetName}.json");
            return File.Exists(filePath);
        }
    }
}
