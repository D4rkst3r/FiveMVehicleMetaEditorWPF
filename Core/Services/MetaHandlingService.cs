using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FiveMVehicleMetaEditorWPF.Core.Models;

namespace FiveMVehicleMetaEditorWPF.Core.Services
{
    /// <summary>
    /// Service for loading, parsing, and saving handling.meta files
    /// Direct port from Python core/meta_handling.py
    /// </summary>
    public class MetaHandlingService
    {
        /// <summary>
        /// Load handling.meta file and extract handling entries
        /// </summary>
        public (bool Success, List<HandlingData>? HandlingEntries, string? Error) LoadHandlingMeta(string filePath)
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

                var handlingList = new List<HandlingData>();

                // Real GTA handling.meta: <CHandlingDataMgr><HandlingData><Item type="CHandlingData">
                var items = root.Descendants("HandlingData")
                    .Elements("Item")
                    .ToList();

                foreach (var item in items)
                {
                    // Real field name is "handlingName", not "handlingNameHash"
                    var handlingNameElem = item.Element("handlingName") ?? item.Element("handlingNameHash");
                    if (handlingNameElem?.Value == null || string.IsNullOrWhiteSpace(handlingNameElem.Value))
                        continue;

                    var data = new HandlingData
                    {
                        HandlingName = handlingNameElem.Value,
                        OriginalElement = item
                    };

                    // Parse numeric values
                    data.Mass = ParseFloatValue(item, "fMass") ?? data.Mass;
                    data.Dimensions_X = ParseFloatValue(item, "vecCentreOfMassDist", "x") ?? data.Dimensions_X;
                    data.Dimensions_Y = ParseFloatValue(item, "vecCentreOfMassDist", "y") ?? data.Dimensions_Y;
                    data.Dimensions_Z = ParseFloatValue(item, "vecCentreOfMassDist", "z") ?? data.Dimensions_Z;

                    // Transmission
                    data.AccelerationX = ParseFloatValue(item, "vecInertiaMultiplier", "x") ?? data.AccelerationX;
                    data.AccelerationY = ParseFloatValue(item, "vecInertiaMultiplier", "y") ?? data.AccelerationY;
                    data.AccelerationZ = ParseFloatValue(item, "vecInertiaMultiplier", "z") ?? data.AccelerationZ;
                    data.NumberOfGears = ParseIntValue(item, "nInitialDriveGears") ?? data.NumberOfGears;
                    data.TopSpeed = ParseFloatValue(item, "fInitialDriveMaxFlatVel") ?? data.TopSpeed;

                    // Steering & Braking
                    data.SteeringLock = ParseFloatValue(item, "fSteeringLock") ?? data.SteeringLock;
                    data.SteeringBias = ParseFloatValue(item, "fSteeringBias") ?? data.SteeringBias;
                    data.BrakeForce = ParseFloatValue(item, "fBrakeForce") ?? data.BrakeForce;
                    data.BrakeBias = ParseFloatValue(item, "fBrakeBiasFront") ?? data.BrakeBias;

                    // Suspension
                    data.SuspensionHeight = ParseFloatValue(item, "fSuspensionHeight") ?? data.SuspensionHeight;
                    data.SuspensionLowerLimit = ParseFloatValue(item, "fSuspensionLowerLimit") ?? data.SuspensionLowerLimit;
                    data.SuspensionUpperLimit = ParseFloatValue(item, "fSuspensionUpperLimit") ?? data.SuspensionUpperLimit;
                    data.SuspensionStiffness = ParseFloatValue(item, "fSuspensionStiffness") ?? data.SuspensionStiffness;
                    data.SuspensionDamping = ParseFloatValue(item, "fSuspensionDamping") ?? data.SuspensionDamping;

                    // Traction
                    data.TractionCurveMax = ParseFloatValue(item, "fTractionCurveMax") ?? data.TractionCurveMax;
                    data.TractionCurveMin = ParseFloatValue(item, "fTractionCurveMin") ?? data.TractionCurveMin;

                    // Advanced settings
                    data.Downforce = ParseFloatValue(item, "fDownforceModifier") ?? data.Downforce;
                    data.RollCentreHeightFront = ParseFloatValue(item, "fRollCentreHeightFront") ?? data.RollCentreHeightFront;
                    data.RollCentreHeightRear = ParseFloatValue(item, "fRollCentreHeightRear") ?? data.RollCentreHeightRear;

                    handlingList.Add(data);
                }

                return (true, handlingList, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error loading handling.meta: {ex.Message}");
            }
        }

        /// <summary>
        /// Save handling data back to XML file
        /// </summary>
        public (bool Success, string? Error) SaveHandlingMeta(string filePath, List<HandlingData> handlingList)
        {
            try
            {
                // Create backup before saving
                var backupPath = filePath + ".backup";
                if (File.Exists(filePath))
                    File.Copy(filePath, backupPath, true);

                var doc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement("CHandlingDataMgr",
                        new XElement("HandlingData",
                            handlingList.Select(h => CreateHandlingElement(h))
                        )
                    )
                );

                doc.Save(filePath, SaveOptions.OmitDuplicateNamespaces);
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, $"Error saving handling.meta: {ex.Message}");
            }
        }

        /// <summary>
        /// Filter handling entries by search term
        /// </summary>
        public List<HandlingData> FilterHandling(List<HandlingData> handlingList, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return handlingList;

            return handlingList
                .Where(h => h.HandlingName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Create XML element from HandlingData object
        /// </summary>
        private XElement CreateHandlingElement(HandlingData handling)
        {
            var elem = new XElement("Item",
                new XElement("handlingNameHash", handling.HandlingName),
                new XElement("fMass", new XAttribute("value", handling.Mass)),
                new XElement("vecCentreOfMassDist",
                    new XAttribute("x", handling.Dimensions_X),
                    new XAttribute("y", handling.Dimensions_Y),
                    new XAttribute("z", handling.Dimensions_Z)),
                new XElement("vecInertiaMultiplier",
                    new XAttribute("x", handling.AccelerationX),
                    new XAttribute("y", handling.AccelerationY),
                    new XAttribute("z", handling.AccelerationZ)),
                new XElement("nInitialDriveGears", new XAttribute("value", handling.NumberOfGears)),
                new XElement("fInitialDriveMaxFlatVel", new XAttribute("value", handling.TopSpeed)),
                new XElement("fSteeringLock", new XAttribute("value", handling.SteeringLock)),
                new XElement("fSteeringBias", new XAttribute("value", handling.SteeringBias)),
                new XElement("fBrakeForce", new XAttribute("value", handling.BrakeForce)),
                new XElement("fBrakeBiasFront", new XAttribute("value", handling.BrakeBias)),
                new XElement("fSuspensionHeight", new XAttribute("value", handling.SuspensionHeight)),
                new XElement("fSuspensionLowerLimit", new XAttribute("value", handling.SuspensionLowerLimit)),
                new XElement("fSuspensionUpperLimit", new XAttribute("value", handling.SuspensionUpperLimit)),
                new XElement("fSuspensionStiffness", new XAttribute("value", handling.SuspensionStiffness)),
                new XElement("fSuspensionDamping", new XAttribute("value", handling.SuspensionDamping)),
                new XElement("fTractionCurveMax", new XAttribute("value", handling.TractionCurveMax)),
                new XElement("fTractionCurveMin", new XAttribute("value", handling.TractionCurveMin)),
                new XElement("fDownforceModifier", new XAttribute("value", handling.Downforce)),
                new XElement("fRollCentreHeightFront", new XAttribute("value", handling.RollCentreHeightFront)),
                new XElement("fRollCentreHeightRear", new XAttribute("value", handling.RollCentreHeightRear))
            );

            return elem;
        }

        /// <summary>
        /// Helper method to parse float value from XML element
        /// </summary>
        private float? ParseFloatValue(XElement item, string elementName)
        {
            var elem = item.Element(elementName);
            if (elem != null && float.TryParse(elem.Attribute("value")?.Value, out var value))
                return value;
            return null;
        }

        /// <summary>
        /// Helper method to parse float value from XML element with attribute
        /// </summary>
        private float? ParseFloatValue(XElement item, string elementName, string attributeName)
        {
            var elem = item.Element(elementName);
            if (elem != null && float.TryParse(elem.Attribute(attributeName)?.Value, out var value))
                return value;
            return null;
        }

        /// <summary>
        /// Helper method to parse integer value from XML element
        /// </summary>
        private int? ParseIntValue(XElement item, string elementName)
        {
            var elem = item.Element(elementName);
            if (elem != null && int.TryParse(elem.Attribute("value")?.Value, out var value))
                return value;
            return null;
        }
    }
}
