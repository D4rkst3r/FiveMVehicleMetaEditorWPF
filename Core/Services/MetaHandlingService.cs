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
    /// </summary>
    public class MetaHandlingService
    {
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

                var items = root.Descendants("HandlingData")
                    .Elements("Item")
                    .ToList();

                foreach (var item in items)
                {
                    var handlingNameElem = item.Element("handlingName") ?? item.Element("handlingNameHash");
                    if (handlingNameElem?.Value == null || string.IsNullOrWhiteSpace(handlingNameElem.Value))
                        continue;

                    var data = new HandlingData
                    {
                        HandlingName = handlingNameElem.Value,
                        OriginalElement = item
                    };

                    // Mass & Dimensions
                    data.Mass = ParseFloat(item, "fMass") ?? data.Mass;
                    data.Dimensions_X = ParseFloatAttr(item, "vecCentreOfMassDist", "x") ?? data.Dimensions_X;
                    data.Dimensions_Y = ParseFloatAttr(item, "vecCentreOfMassDist", "y") ?? data.Dimensions_Y;
                    data.Dimensions_Z = ParseFloatAttr(item, "vecCentreOfMassDist", "z") ?? data.Dimensions_Z;

                    // Engine & Drive
                    data.DriveForce = ParseFloat(item, "fInitialDriveForce") ?? data.DriveForce;
                    data.DriveForceFront = ParseFloat(item, "fInitialDriveForceFront") ?? data.DriveForceFront;
                    data.NumberOfGears = ParseInt(item, "nInitialDriveGears") ?? data.NumberOfGears;
                    data.TopSpeed = ParseFloat(item, "fInitialDriveMaxFlatVel") ?? data.TopSpeed;

                    // Transmission (inertia)
                    data.AccelerationX = ParseFloatAttr(item, "vecInertiaMultiplier", "x") ?? data.AccelerationX;
                    data.AccelerationY = ParseFloatAttr(item, "vecInertiaMultiplier", "y") ?? data.AccelerationY;
                    data.AccelerationZ = ParseFloatAttr(item, "vecInertiaMultiplier", "z") ?? data.AccelerationZ;

                    // Steering & Braking
                    data.SteeringLock = ParseFloat(item, "fSteeringLock") ?? data.SteeringLock;
                    data.SteeringBias = ParseFloat(item, "fSteeringBias") ?? data.SteeringBias;
                    data.BrakeForce = ParseFloat(item, "fBrakeForce") ?? data.BrakeForce;
                    data.BrakeBias = ParseFloat(item, "fBrakeBiasFront") ?? data.BrakeBias;
                    data.HandbrakeForce = ParseFloat(item, "fHandBrakeForce") ?? data.HandbrakeForce;

                    // Suspension
                    data.SuspensionHeight = ParseFloat(item, "fSuspensionHeight") ?? data.SuspensionHeight;
                    data.SuspensionLowerLimit = ParseFloat(item, "fSuspensionLowerLimit") ?? data.SuspensionLowerLimit;
                    data.SuspensionUpperLimit = ParseFloat(item, "fSuspensionUpperLimit") ?? data.SuspensionUpperLimit;
                    data.SuspensionStiffness = ParseFloat(item, "fSuspensionStiffness") ?? data.SuspensionStiffness;
                    data.SuspensionDamping = ParseFloat(item, "fSuspensionDamping") ?? data.SuspensionDamping;
                    data.SuspensionRaise = ParseFloat(item, "fSuspensionForce") ?? data.SuspensionRaise;
                    data.AntiRollBarStiffFront = ParseFloat(item, "fAntiRollBarStiffness") ?? data.AntiRollBarStiffFront;
                    data.AntiRollBarStiffRear = ParseFloat(item, "fAntiRollBarForce") ?? data.AntiRollBarStiffRear;

                    // Traction
                    data.TractionCurveMax = ParseFloat(item, "fTractionCurveMax") ?? data.TractionCurveMax;
                    data.TractionCurveMin = ParseFloat(item, "fTractionCurveMin") ?? data.TractionCurveMin;
                    data.TractionCurveLateral = ParseFloat(item, "fTractionCurveLateral") ?? data.TractionCurveLateral;
                    data.TractionSpringDeltaMax = ParseFloat(item, "fTractionSpringDeltaMax") ?? data.TractionSpringDeltaMax;
                    data.TractionBiasFront = ParseFloat(item, "fTractionBiasFront") ?? data.TractionBiasFront;
                    data.TractionLossMult = ParseFloat(item, "fTractionLossMult") ?? data.TractionLossMult;

                    // Damage
                    data.DeformationDamageMult = ParseFloat(item, "fDeformationDamageMult") ?? data.DeformationDamageMult;
                    data.CollisionDamageMult = ParseFloat(item, "fCollisionDamageMult") ?? data.CollisionDamageMult;
                    data.EngineDamageMult = ParseFloat(item, "fEngineDamageMult") ?? data.EngineDamageMult;
                    data.PetrolTankVolume = ParseFloat(item, "fPetrolTankVolume") ?? data.PetrolTankVolume;

                    // Advanced
                    data.Downforce = ParseFloat(item, "fDownforceModifier") ?? data.Downforce;
                    data.RollCentreHeightFront = ParseFloat(item, "fRollCentreHeightFront") ?? data.RollCentreHeightFront;
                    data.RollCentreHeightRear = ParseFloat(item, "fRollCentreHeightRear") ?? data.RollCentreHeightRear;
                    data.CamberStiffness = ParseFloat(item, "fCamberStiffnessBetweenAxles") ?? data.CamberStiffness;
                    data.WeaponForceMult = ParseFloat(item, "fWeaponDamageMult") ?? data.WeaponForceMult;

                    handlingList.Add(data);
                }

                return (true, handlingList, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error loading handling.meta: {ex.Message}");
            }
        }

        public (bool Success, string? Error) SaveHandlingMeta(string filePath, List<HandlingData> handlingList)
        {
            try
            {
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

        public List<HandlingData> FilterHandling(List<HandlingData> handlingList, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return handlingList;

            return handlingList
                .Where(h => h.HandlingName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private XElement CreateHandlingElement(HandlingData h)
        {
            return new XElement("Item",
                new XElement("handlingName", h.HandlingName),
                new XElement("fMass", new XAttribute("value", h.Mass)),
                new XElement("vecCentreOfMassDist",
                    new XAttribute("x", h.Dimensions_X),
                    new XAttribute("y", h.Dimensions_Y),
                    new XAttribute("z", h.Dimensions_Z)),
                new XElement("vecInertiaMultiplier",
                    new XAttribute("x", h.AccelerationX),
                    new XAttribute("y", h.AccelerationY),
                    new XAttribute("z", h.AccelerationZ)),
                new XElement("fInitialDriveForce", new XAttribute("value", h.DriveForce)),
                new XElement("fInitialDriveForceFront", new XAttribute("value", h.DriveForceFront)),
                new XElement("nInitialDriveGears", new XAttribute("value", h.NumberOfGears)),
                new XElement("fInitialDriveMaxFlatVel", new XAttribute("value", h.TopSpeed)),
                new XElement("fSteeringLock", new XAttribute("value", h.SteeringLock)),
                new XElement("fSteeringBias", new XAttribute("value", h.SteeringBias)),
                new XElement("fBrakeForce", new XAttribute("value", h.BrakeForce)),
                new XElement("fBrakeBiasFront", new XAttribute("value", h.BrakeBias)),
                new XElement("fHandBrakeForce", new XAttribute("value", h.HandbrakeForce)),
                new XElement("fSuspensionHeight", new XAttribute("value", h.SuspensionHeight)),
                new XElement("fSuspensionLowerLimit", new XAttribute("value", h.SuspensionLowerLimit)),
                new XElement("fSuspensionUpperLimit", new XAttribute("value", h.SuspensionUpperLimit)),
                new XElement("fSuspensionStiffness", new XAttribute("value", h.SuspensionStiffness)),
                new XElement("fSuspensionDamping", new XAttribute("value", h.SuspensionDamping)),
                new XElement("fSuspensionForce", new XAttribute("value", h.SuspensionRaise)),
                new XElement("fAntiRollBarStiffness", new XAttribute("value", h.AntiRollBarStiffFront)),
                new XElement("fAntiRollBarForce", new XAttribute("value", h.AntiRollBarStiffRear)),
                new XElement("fTractionCurveMax", new XAttribute("value", h.TractionCurveMax)),
                new XElement("fTractionCurveMin", new XAttribute("value", h.TractionCurveMin)),
                new XElement("fTractionCurveLateral", new XAttribute("value", h.TractionCurveLateral)),
                new XElement("fTractionSpringDeltaMax", new XAttribute("value", h.TractionSpringDeltaMax)),
                new XElement("fTractionBiasFront", new XAttribute("value", h.TractionBiasFront)),
                new XElement("fTractionLossMult", new XAttribute("value", h.TractionLossMult)),
                new XElement("fDeformationDamageMult", new XAttribute("value", h.DeformationDamageMult)),
                new XElement("fCollisionDamageMult", new XAttribute("value", h.CollisionDamageMult)),
                new XElement("fEngineDamageMult", new XAttribute("value", h.EngineDamageMult)),
                new XElement("fPetrolTankVolume", new XAttribute("value", h.PetrolTankVolume)),
                new XElement("fDownforceModifier", new XAttribute("value", h.Downforce)),
                new XElement("fRollCentreHeightFront", new XAttribute("value", h.RollCentreHeightFront)),
                new XElement("fRollCentreHeightRear", new XAttribute("value", h.RollCentreHeightRear)),
                new XElement("fCamberStiffnessBetweenAxles", new XAttribute("value", h.CamberStiffness)),
                new XElement("fWeaponDamageMult", new XAttribute("value", h.WeaponForceMult))
            );
        }

        private float? ParseFloat(XElement item, string elementName)
        {
            var elem = item.Element(elementName);
            if (elem == null) return null;
            var val = elem.Attribute("value")?.Value ?? elem.Value;
            if (float.TryParse(val, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var result))
                return result;
            return null;
        }

        private float? ParseFloatAttr(XElement item, string elementName, string attributeName)
        {
            var elem = item.Element(elementName);
            if (elem == null) return null;
            if (float.TryParse(elem.Attribute(attributeName)?.Value,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var result))
                return result;
            return null;
        }

        private int? ParseInt(XElement item, string elementName)
        {
            var elem = item.Element(elementName);
            if (elem == null) return null;
            var val = elem.Attribute("value")?.Value ?? elem.Value;
            if (int.TryParse(val, out var result))
                return result;
            return null;
        }
    }
}
