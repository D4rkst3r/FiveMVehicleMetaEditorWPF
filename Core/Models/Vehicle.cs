using System;
using System.Collections.Generic;

namespace FiveMVehicleMetaEditorWPF.Core.Models
{
    /// <summary>
    /// Represents a single vehicle entry from vehicles.meta
    /// </summary>
    public class Vehicle
    {
        public string ModelName { get; set; } = "";
        public string TxdName { get; set; } = "";
        public string GameName { get; set; } = "";
        public string VehicleMakeName { get; set; } = "";
        public string HandlingId { get; set; } = "";
        public string AudioNameHash { get; set; } = "0";
        public string VehicleType { get; set; } = "AUTOMOBILE";
        public int Class { get; set; } = 0;
        public string VehicleClass { get; set; } = "VC_SEDAN";
        public string Layout { get; set; } = "LAYOUT_STD_CAR";
        public string CameraName { get; set; } = "default";
        public string AimCameraName { get; set; } = "default";
        public int Seats { get; set; } = 4;
        public float WheelScale { get; set; } = 1.0f;
        public float WheelScaleRear { get; set; } = 1.0f;
        public int DefaultBodyHealth { get; set; } = 1000;
        public float DirtLevelMax { get; set; } = 0.5f;
        public float DamageMapScale { get; set; } = 1.0f;
        public int MaxNum { get; set; } = 1;
        public int Frequency { get; set; } = 5;
        public int Swankness { get; set; } = 0;
        public int PlateType { get; set; } = 0;
        public int DriveableDoors { get; set; } = 3;
        public float WeaponForceMult { get; set; } = 1.0f;
        public List<string> Flags { get; set; } = new();

        // POV Camera Offset
        public float PovCameraOffsetX { get; set; } = 0.0f;
        public float PovCameraOffsetY { get; set; } = 0.0f;
        public float PovCameraOffsetZ { get; set; } = 0.0f;

        // Original XML element (for reconstruction)
        public object? OriginalElement { get; set; }

        public override string ToString() => $"{ModelName} ({VehicleMakeName})";

        public bool HasFlag(string flag) => Flags.Contains(flag);

        public void AddFlag(string flag)
        {
            if (!HasFlag(flag))
                Flags.Add(flag);
        }

        public void RemoveFlag(string flag) => Flags.Remove(flag);
    }
}
