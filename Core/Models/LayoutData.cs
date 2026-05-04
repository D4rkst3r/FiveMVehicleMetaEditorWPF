using System.Collections.Generic;

namespace FiveMVehicleMetaEditorWPF.Core.Models
{
    /// <summary>
    /// Represents seating and door layout from vehiclelayouts.meta
    /// </summary>
    public class LayoutData
    {
        public string LayoutName { get; set; } = "";
        public string Category { get; set; } = "Standard Vehicles"; // Standard Vehicles, Emergency Services, etc.

        // Seats configuration
        public Dictionary<int, string> Seats { get; set; } = new();

        // Doors configuration
        public Dictionary<int, string> Doors { get; set; } = new();

        // Special properties
        public bool HasRoof { get; set; } = true;
        public bool HasTrunk { get; set; } = true;
        public bool HasHood { get; set; } = true;

        // Cover extents for seat covers
        public Dictionary<string, (float Min, float Max)> CoverExtents { get; set; } = new();

        // Original XML element
        public object? OriginalElement { get; set; }

        public override string ToString() => LayoutName;

        public void AddSeat(int seatId, string seatName)
        {
            Seats[seatId] = seatName;
        }

        public void AddDoor(int doorId, string doorName)
        {
            Doors[doorId] = doorName;
        }
    }
}
