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

        // Engine & Drive
        public float DriveForce { get; set; } = 0.3f;        // fInitialDriveForce
        public float DriveForceFront { get; set; } = 0.0f;   // fInitialDriveForceFront (AWD front bias 0=RWD,1=FWD)
        public int NumberOfGears { get; set; } = 6;
        public float TopSpeed { get; set; } = 200f;

        // Transmission (inertia multiplier)
        public float AccelerationX { get; set; } = 0.3f;
        public float AccelerationY { get; set; } = 0.0f;
        public float AccelerationZ { get; set; } = 0.0f;

        // Steering & Braking
        public float SteeringLock { get; set; } = 45f;
        public float SteeringBias { get; set; } = 0f;
        public float BrakeForce { get; set; } = 1f;
        public float BrakeBias { get; set; } = 0.5f;
        public float HandbrakeForce { get; set; } = 2f;      // fHandBrakeForce

        // Suspension
        public float SuspensionHeight { get; set; } = 0.1f;
        public float SuspensionLowerLimit { get; set; } = 0.0f;
        public float SuspensionUpperLimit { get; set; } = 0.3f;
        public float SuspensionStiffness { get; set; } = 1f;
        public float SuspensionDamping { get; set; } = 0.05f;
        public float SuspensionRaise { get; set; } = 0.0f;   // fSuspensionForce (raise amount)
        public float AntiRollBarStiffFront { get; set; } = 0.0f;
        public float AntiRollBarStiffRear { get; set; } = 0.0f;

        // Traction
        public float TractionCurveMax { get; set; } = 1.3f;
        public float TractionCurveMin { get; set; } = 0.8f;
        public float TractionCurveLateral { get; set; } = 22.5f;
        public float TractionSpringDeltaMax { get; set; } = 0.15f;
        public float TractionBiasFront { get; set; } = 0.47f; // 0=rear,1=front
        public float TractionLossMult { get; set; } = 1f;

        // Damage
        public float DeformationDamageMult { get; set; } = 1f;
        public float CollisionDamageMult { get; set; } = 1f;
        public float EngineDamageMult { get; set; } = 1f;
        public float PetrolTankVolume { get; set; } = 65f;

        // Advanced settings
        public float Downforce { get; set; } = 0.3f;
        public float RollCentreHeightFront { get; set; } = 0.5f;
        public float RollCentreHeightRear { get; set; } = 0.5f;
        public float CamberStiffness { get; set; } = 0.0f;
        public float WeaponForceMult { get; set; } = 1.0f;

        // Original XML element
        public object? OriginalElement { get; set; }

        public override string ToString() => HandlingName;
    }
}
