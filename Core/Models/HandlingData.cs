namespace FiveMVehicleMetaEditorWPF.Core.Models
{
    /// <summary>
    /// Represents handling (physics) data for a vehicle from handling.meta
    /// </summary>
    public class HandlingData
    {
        public string HandlingName { get; set; } = "";

        // Mass & Dimensions
        public float Mass { get; set; } = 1500f;
        public float Dimensions_X { get; set; } = 2f;
        public float Dimensions_Y { get; set; } = 4f;
        public float Dimensions_Z { get; set; } = 1.5f;

        // Transmission
        public float AccelerationX { get; set; } = 0.3f;
        public float AccelerationY { get; set; } = 0.0f;
        public float AccelerationZ { get; set; } = 0.0f;
        public int NumberOfGears { get; set; } = 6;
        public float TopSpeed { get; set; } = 200f;

        // Steering & Braking
        public float SteeringLock { get; set; } = 45f;
        public float SteeringBias { get; set; } = 0f;
        public float BrakeForce { get; set; } = 1f;
        public float BrakeBias { get; set; } = 0.5f;

        // Suspension
        public float SuspensionHeight { get; set; } = 0.1f;
        public float SuspensionLowerLimit { get; set; } = 0.0f;
        public float SuspensionUpperLimit { get; set; } = 0.3f;
        public float SuspensionStiffness { get; set; } = 1f;
        public float SuspensionDamping { get; set; } = 0.05f;

        // Traction
        public float TractionCurveMax { get; set; } = 1.3f;
        public float TractionCurveMin { get; set; } = 0.8f;

        // Advanced settings
        public float Downforce { get; set; } = 0.3f;
        public float RollCentreHeightFront { get; set; } = 0.5f;
        public float RollCentreHeightRear { get; set; } = 0.5f;

        // Original XML element
        public object? OriginalElement { get; set; }

        public override string ToString() => HandlingName;
    }
}
