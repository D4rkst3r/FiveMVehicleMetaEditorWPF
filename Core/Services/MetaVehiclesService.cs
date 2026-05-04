using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FiveMVehicleMetaEditorWPF.Core.Models;

namespace FiveMVehicleMetaEditorWPF.Core.Services
{
    /// <summary>
    /// Service for loading, parsing, and saving vehicles.meta files
    /// Direct port from Python core/meta_vehicles.py
    /// </summary>
    public class MetaVehiclesService
    {
        /// <summary>
        /// Load vehicles.meta file and extract vehicle entries
        /// </summary>
        public (bool Success, List<Vehicle>? Vehicles, string? Error) LoadVehiclesMeta(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return (false, null, $"File not found: {filePath}");

                var content = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                var doc = XDocument.Parse(content);
                var root = doc.Root;

                if (root == null)
                    return (false, null, "Invalid XML: No root element");

                var vehicles = new List<Vehicle>();

                // Find all InitDatas/Item elements
                var items = root.Descendants("InitDatas")
                    .Elements("Item")
                    .ToList();

                foreach (var item in items)
                {
                    var modelNameElem = item.Element("modelName");
                    if (modelNameElem?.Value == null || string.IsNullOrWhiteSpace(modelNameElem.Value))
                        continue;

                    var vehicle = new Vehicle
                    {
                        ModelName = modelNameElem.Value,
                        TxdName = item.Element("txdName")?.Value ?? "",
                        GameName = item.Element("gameName")?.Value ?? "",
                        VehicleMakeName = item.Element("vehicleMakeName")?.Value ?? "",
                        HandlingId = item.Element("handlingId")?.Value ?? "",
                        AudioNameHash = item.Element("audioNameHash")?.Value ?? "0",
                        VehicleType = item.Element("vehicleType")?.Value ?? "AUTOMOBILE",
                        Class = int.TryParse(item.Element("class")?.Attribute("value")?.Value, out var cls) ? cls : 0,
                        VehicleClass = item.Element("vehicleClass")?.Value ?? "VC_SEDAN",
                        Layout = item.Element("layout")?.Value ?? "LAYOUT_STD_CAR",
                        CameraName = item.Element("cameraName")?.Value ?? "default",
                        AimCameraName = item.Element("aimCameraName")?.Value ?? "default",
                        Seats = int.TryParse(item.Element("seats")?.Attribute("value")?.Value, out var seats) ? seats : 4,
                        WheelScale = float.TryParse(item.Element("wheelScale")?.Attribute("value")?.Value, out var ws) ? ws : 1.0f,
                        WheelScaleRear = float.TryParse(item.Element("wheelScaleRear")?.Attribute("value")?.Value, out var wsr) ? wsr : 1.0f,
                        DefaultBodyHealth = int.TryParse(item.Element("defaultBodyHealth")?.Attribute("value")?.Value, out var dbh) ? dbh : 1000,
                        DirtLevelMax = float.TryParse(item.Element("dirtLevelMax")?.Attribute("value")?.Value, out var dlm) ? dlm : 0.5f,
                        DamageMapScale = float.TryParse(item.Element("damageMapScale")?.Attribute("value")?.Value, out var dms) ? dms : 1.0f,
                        MaxNum = int.TryParse(item.Element("maxNum")?.Attribute("value")?.Value, out var mn) ? mn : 1,
                        Frequency = int.TryParse(item.Element("frequency")?.Attribute("value")?.Value, out var freq) ? freq : 5,
                        Swankness = int.TryParse(item.Element("swankness")?.Attribute("value")?.Value, out var sw) ? sw : 0,
                        PlateType = int.TryParse(item.Element("plateType")?.Attribute("value")?.Value, out var pt) ? pt : 0,
                        DriveableDoors = int.TryParse(item.Element("driveableDoors")?.Attribute("value")?.Value, out var dd) ? dd : 3,
                        WeaponForceMult = float.TryParse(item.Element("weaponForceMult")?.Attribute("value")?.Value, out var wfm) ? wfm : 1.0f,
                        OriginalElement = item
                    };

                    // Parse flags
                    var flagsElem = item.Element("flags");
                    if (flagsElem?.Value != null)
                    {
                        vehicle.Flags = flagsElem.Value
                            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .ToList();
                    }

                    // Parse POV Camera Offset
                    var povElem = item.Element("PovCameraOffset");
                    if (povElem != null)
                    {
                        vehicle.PovCameraOffsetX = float.TryParse(povElem.Attribute("x")?.Value, out var x) ? x : 0.0f;
                        vehicle.PovCameraOffsetY = float.TryParse(povElem.Attribute("y")?.Value, out var y) ? y : 0.0f;
                        vehicle.PovCameraOffsetZ = float.TryParse(povElem.Attribute("z")?.Value, out var z) ? z : 0.0f;
                    }

                    vehicles.Add(vehicle);
                }

                return (true, vehicles, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error loading vehicles.meta: {ex.Message}");
            }
        }

        /// <summary>
        /// Save vehicles back to XML file
        /// </summary>
        public (bool Success, string? Error) SaveVehiclesMeta(string filePath, List<Vehicle> vehicles)
        {
            try
            {
                // Create backup before saving
                var backupPath = filePath + ".backup";
                if (File.Exists(filePath))
                    File.Copy(filePath, backupPath, true);

                var doc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement("CVehicleMetadataStructures",
                        new XElement("InitDataList",
                            vehicles.Select(v => CreateVehicleElement(v))
                        )
                    )
                );

                doc.Save(filePath, SaveOptions.OmitDuplicateNamespaces);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error saving vehicles.meta: {ex.Message}");
            }
        }

        /// <summary>
        /// Create XML element from Vehicle object
        /// </summary>
        private XElement CreateVehicleElement(Vehicle vehicle)
        {
            var elem = new XElement("Item",
                new XElement("modelName", vehicle.ModelName),
                new XElement("txdName", vehicle.TxdName),
                new XElement("gameName", vehicle.GameName),
                new XElement("vehicleMakeName", vehicle.VehicleMakeName),
                new XElement("handlingId", vehicle.HandlingId),
                new XElement("audioNameHash", vehicle.AudioNameHash),
                new XElement("vehicleType", vehicle.VehicleType),
                new XElement("class", new XAttribute("value", vehicle.Class)),
                new XElement("vehicleClass", vehicle.VehicleClass),
                new XElement("layout", vehicle.Layout),
                new XElement("cameraName", vehicle.CameraName),
                new XElement("aimCameraName", vehicle.AimCameraName),
                new XElement("seats", new XAttribute("value", vehicle.Seats)),
                new XElement("wheelScale", new XAttribute("value", vehicle.WheelScale)),
                new XElement("wheelScaleRear", new XAttribute("value", vehicle.WheelScaleRear)),
                new XElement("defaultBodyHealth", new XAttribute("value", vehicle.DefaultBodyHealth)),
                new XElement("dirtLevelMax", new XAttribute("value", vehicle.DirtLevelMax)),
                new XElement("damageMapScale", new XAttribute("value", vehicle.DamageMapScale)),
                new XElement("maxNum", new XAttribute("value", vehicle.MaxNum)),
                new XElement("frequency", new XAttribute("value", vehicle.Frequency)),
                new XElement("swankness", new XAttribute("value", vehicle.Swankness)),
                new XElement("plateType", new XAttribute("value", vehicle.PlateType)),
                new XElement("driveableDoors", new XAttribute("value", vehicle.DriveableDoors)),
                new XElement("weaponForceMult", new XAttribute("value", vehicle.WeaponForceMult)),
                new XElement("flags", string.Join(" ", vehicle.Flags)),
                new XElement("PovCameraOffset",
                    new XAttribute("x", vehicle.PovCameraOffsetX),
                    new XAttribute("y", vehicle.PovCameraOffsetY),
                    new XAttribute("z", vehicle.PovCameraOffsetZ)
                )
            );

            return elem;
        }

        /// <summary>
        /// Filter vehicles by search term
        /// </summary>
        public List<Vehicle> FilterVehicles(List<Vehicle> vehicles, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return vehicles;

            var term = searchTerm.ToLower();
            return vehicles.Where(v =>
                v.ModelName.ToLower().Contains(term) ||
                v.GameName.ToLower().Contains(term) ||
                v.VehicleMakeName.ToLower().Contains(term) ||
                v.HandlingId.ToLower().Contains(term)
            ).ToList();
        }
    }
}
