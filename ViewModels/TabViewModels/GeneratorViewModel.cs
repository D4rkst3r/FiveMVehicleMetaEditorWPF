using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using System.Xml.Linq;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Services;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class GeneratorViewModel : BaseTabViewModel
    {
        // ── Identity ──────────────────────────────────────────────────────────────
        private string _modelName = "";
        private string _txdName = "";
        private string _handlingId = "";
        private string _gameName = "";
        private string _vehicleMakeName = "ROCKSTAR";
        private string _audioNameHash = "";

        // ── Classification ────────────────────────────────────────────────────────
        private string _vehicleClass = "VC_SEDAN";
        private string _vehicleType = "VEHICLE_TYPE_CAR";
        private string _layout = "LAYOUT_STD_CAR";

        // ── Physical ─────────────────────────────────────────────────────────────
        private int _seats = 4;
        private int _frequency = 10;
        private int _defaultBodyHealth = 1000;
        private int _plateType = 0;
        private int _driveableDoors = 3;
        private float _wheelScale = 0.268f;
        private float _wheelScaleRear = 0.268f;
        private float _dirtLevelMax = 0.5f;
        private float _weaponForceMult = 1.0f;
        private int _swankness = 0;
        private int _maxNum = 10;

        // ── Output options ────────────────────────────────────────────────────────
        private bool _generateHandling = false;
        private bool _generateFxManifest = false;
        private bool _generateCarcols = false;

        // ── Presets ──────────────────────────────────────────────────────────────
        public List<string> VehiclePresets { get; } = new()
        {
            "— Preset —",
            "Sport Car", "SUV", "Truck", "Van",
            "Police Car", "Ambulance", "Fire Truck",
            "Motorcycle", "Helicopter", "Boat"
        };

        public List<string> VehicleTypes { get; } = new()
        {
            "VEHICLE_TYPE_CAR", "VEHICLE_TYPE_TRUCK", "VEHICLE_TYPE_BIKE",
            "VEHICLE_TYPE_BOAT", "VEHICLE_TYPE_HELI", "VEHICLE_TYPE_PLANE",
            "VEHICLE_TYPE_SUBMARINE", "VEHICLE_TYPE_TRAILER"
        };

        public List<string> VehicleClasses { get; } = new()
        {
            "VC_COMPACT", "VC_SEDAN", "VC_SUV", "VC_COUPE", "VC_MUSCLE",
            "VC_SPORTS_CLASSIC", "VC_SPORTS", "VC_SUPER", "VC_MOTORCYCLE",
            "VC_OFFROAD", "VC_INDUSTRIAL", "VC_UTILITY", "VC_VANS",
            "VC_BOATS", "VC_HELICOPTERS", "VC_PLANES", "VC_SERVICE",
            "VC_EMERGENCY", "VC_MILITARY", "VC_COMMERCIAL"
        };

        public List<string> Layouts { get; } = new()
        {
            "LAYOUT_STD_CAR", "LAYOUT_STD_CAR_REAR", "LAYOUT_STD_CAR_BIG",
            "LAYOUT_STD_VAN", "LAYOUT_STD_TRUCK", "LAYOUT_STD_COACH",
            "LAYOUT_STD_BIKE", "LAYOUT_STD_QUAD", "LAYOUT_STD_BOAT",
            "LAYOUT_STD_HELI", "LAYOUT_STD_PLANE", "LAYOUT_STD_AEROPLANE",
            "LAYOUT_STD_INDUSTRIAL"
        };

        // ── Properties ────────────────────────────────────────────────────────────
        public string ModelName { get => _modelName; set { _modelName = value; OnPropertyChanged(); AutoFillDependents(); } }
        public string TxdName { get => _txdName; set { _txdName = value; OnPropertyChanged(); } }
        public string HandlingId { get => _handlingId; set { _handlingId = value; OnPropertyChanged(); } }
        public string GameName { get => _gameName; set { _gameName = value; OnPropertyChanged(); } }
        public string VehicleMakeName { get => _vehicleMakeName; set { _vehicleMakeName = value; OnPropertyChanged(); } }
        public string AudioNameHash { get => _audioNameHash; set { _audioNameHash = value; OnPropertyChanged(); } }
        public string VehicleClass { get => _vehicleClass; set { _vehicleClass = value; OnPropertyChanged(); } }
        public string VehicleType { get => _vehicleType; set { _vehicleType = value; OnPropertyChanged(); } }
        public string Layout { get => _layout; set { _layout = value; OnPropertyChanged(); } }
        public int Seats { get => _seats; set { _seats = value; OnPropertyChanged(); } }
        public int Frequency { get => _frequency; set { _frequency = value; OnPropertyChanged(); } }
        public int DefaultBodyHealth { get => _defaultBodyHealth; set { _defaultBodyHealth = value; OnPropertyChanged(); } }
        public int PlateType { get => _plateType; set { _plateType = value; OnPropertyChanged(); } }
        public int DriveableDoors { get => _driveableDoors; set { _driveableDoors = value; OnPropertyChanged(); } }
        public float WheelScale { get => _wheelScale; set { _wheelScale = value; OnPropertyChanged(); } }
        public float WheelScaleRear { get => _wheelScaleRear; set { _wheelScaleRear = value; OnPropertyChanged(); } }
        public float DirtLevelMax { get => _dirtLevelMax; set { _dirtLevelMax = value; OnPropertyChanged(); } }
        public float WeaponForceMult { get => _weaponForceMult; set { _weaponForceMult = value; OnPropertyChanged(); } }
        public int Swankness { get => _swankness; set { _swankness = value; OnPropertyChanged(); } }
        public int MaxNum { get => _maxNum; set { _maxNum = value; OnPropertyChanged(); } }
        public bool GenerateHandling { get => _generateHandling; set { _generateHandling = value; OnPropertyChanged(); } }
        public bool GenerateFxManifest { get => _generateFxManifest; set { _generateFxManifest = value; OnPropertyChanged(); } }
        public bool GenerateCarcols { get => _generateCarcols; set { _generateCarcols = value; OnPropertyChanged(); } }

        // Commands
        public ICommand GenerateCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ApplyPresetCommand { get; }

        public GeneratorViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            GenerateCommand = new RelayCommand(ExecuteGenerate);
            ResetCommand = new RelayCommand(ExecuteReset);
            ApplyPresetCommand = new RelayCommand(param =>
            {
                if (param is string p) ApplyPreset(p);
            });
            ShowInfo("Fill in the fields and click Generate to create meta files");
        }

        private void AutoFillDependents()
        {
            if (string.IsNullOrWhiteSpace(_txdName))
            {
                OnPropertyChanged(nameof(TxdName));
            }
            if (string.IsNullOrWhiteSpace(_handlingId) && !string.IsNullOrWhiteSpace(_modelName))
            {
                // Don't auto-fill HandlingId — user should pick it explicitly
            }
        }

        private void ApplyPreset(string presetName)
        {
            switch (presetName)
            {
                case "Sport Car":
                    VehicleType = "VEHICLE_TYPE_CAR"; VehicleClass = "VC_SPORTS";
                    Layout = "LAYOUT_STD_CAR"; Seats = 2; HandlingId = "SPORTSCAR";
                    DefaultBodyHealth = 1000; Frequency = 5; Swankness = 3;
                    break;
                case "SUV":
                    VehicleType = "VEHICLE_TYPE_CAR"; VehicleClass = "VC_SUV";
                    Layout = "LAYOUT_STD_CAR_BIG"; Seats = 4; HandlingId = "SUV";
                    DefaultBodyHealth = 1200; Frequency = 8;
                    break;
                case "Truck":
                    VehicleType = "VEHICLE_TYPE_TRUCK"; VehicleClass = "VC_COMMERCIAL";
                    Layout = "LAYOUT_STD_TRUCK"; Seats = 2; HandlingId = "TRUCK";
                    DefaultBodyHealth = 2000; Frequency = 4; WheelScale = 0.39f;
                    break;
                case "Van":
                    VehicleType = "VEHICLE_TYPE_CAR"; VehicleClass = "VC_VANS";
                    Layout = "LAYOUT_STD_VAN"; Seats = 2; HandlingId = "MULE";
                    DefaultBodyHealth = 1500; DriveableDoors = 2;
                    break;
                case "Police Car":
                    VehicleType = "VEHICLE_TYPE_CAR"; VehicleClass = "VC_EMERGENCY";
                    Layout = "LAYOUT_STD_CAR"; Seats = 4; HandlingId = "POLICE";
                    DefaultBodyHealth = 1500; Frequency = 30;
                    break;
                case "Ambulance":
                    VehicleType = "VEHICLE_TYPE_CAR"; VehicleClass = "VC_EMERGENCY";
                    Layout = "LAYOUT_STD_VAN"; Seats = 4; HandlingId = "AMBULANCE";
                    DefaultBodyHealth = 2000; Frequency = 6;
                    break;
                case "Fire Truck":
                    VehicleType = "VEHICLE_TYPE_TRUCK"; VehicleClass = "VC_EMERGENCY";
                    Layout = "LAYOUT_STD_TRUCK"; Seats = 4; HandlingId = "FIRETRUK";
                    DefaultBodyHealth = 3000; Frequency = 4;
                    break;
                case "Motorcycle":
                    VehicleType = "VEHICLE_TYPE_BIKE"; VehicleClass = "VC_MOTORCYCLE";
                    Layout = "LAYOUT_STD_BIKE"; Seats = 2; HandlingId = "AKUMA";
                    DefaultBodyHealth = 500; Frequency = 6; WheelScale = 0.3f;
                    break;
                case "Helicopter":
                    VehicleType = "VEHICLE_TYPE_HELI"; VehicleClass = "VC_HELICOPTERS";
                    Layout = "LAYOUT_STD_HELI"; Seats = 4; HandlingId = "POLMAV";
                    DefaultBodyHealth = 2000; Frequency = 2;
                    break;
                case "Boat":
                    VehicleType = "VEHICLE_TYPE_BOAT"; VehicleClass = "VC_BOATS";
                    Layout = "LAYOUT_STD_BOAT"; Seats = 4; HandlingId = "DINGHY";
                    DefaultBodyHealth = 1200; Frequency = 3;
                    break;
            }
            ShowSuccess($"Preset '{presetName}' applied");
        }

        private void ExecuteGenerate()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ModelName)) { ShowError("Model Name is required"); return; }
                if (string.IsNullOrWhiteSpace(HandlingId)) { ShowError("Handling ID is required"); return; }

                IsLoading = true;
                ShowInfo("Generating files...");

                var txd = string.IsNullOrWhiteSpace(TxdName) ? ModelName : TxdName;
                var game = string.IsNullOrWhiteSpace(GameName) ? ModelName.ToUpper() : GameName;
                var folder = FileService.SaveFileDialog("vehicles", $"{ModelName}_vehicles.meta");
                if (folder == null) return;

                var dir = Path.GetDirectoryName(folder)!;
                var generatedFiles = new List<string>();

                // ── vehicles.meta ──────────────────────────────────────────────
                var vehiclesDoc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement("CVehicleModelInfo__InitDataList",
                        new XElement("InitDatas",
                            new XElement("Item",
                                new XElement("modelName", ModelName.ToLower()),
                                new XElement("txdName", txd.ToLower()),
                                new XElement("handlingId", HandlingId.ToUpper()),
                                new XElement("gameName", game),
                                new XElement("vehicleMakeName", VehicleMakeName.ToUpper()),
                                new XElement("expressionDictName", "null"),
                                new XElement("expressionName", "null"),
                                new XElement("animConvRoofDictName", "null"),
                                new XElement("animConvRoofName", "null"),
                                new XElement("ptfxAssetName", "null"),
                                new XElement("audioNameHash", string.IsNullOrWhiteSpace(AudioNameHash) ? "" : AudioNameHash),
                                new XElement("layout", Layout),
                                new XElement("cameraName", "DEFAULT_FOLLOW_VEHICLE_CAMERA"),
                                new XElement("aimCameraName", "DEFAULT_THIRD_PERSON_VEHICLE_AIM_CAMERA"),
                                new XElement("bonnetCameraName", "VEHICLE_BONNET_CAMERA_MID_NEAR"),
                                new XElement("povCameraName", "REDUCED_NEAR_CLIP_POV_CAMERA"),
                                new XElement("vehicleClass", VehicleClass),
                                new XElement("vehicleType", VehicleType),
                                new XElement("wheelScale", new XAttribute("value", WheelScale.ToString("F3"))),
                                new XElement("wheelScaleRear", new XAttribute("value", WheelScaleRear.ToString("F3"))),
                                new XElement("dirtLevelMin", new XAttribute("value", "0.0")),
                                new XElement("dirtLevelMax", new XAttribute("value", DirtLevelMax.ToString("F1"))),
                                new XElement("defaultBodyHealth", new XAttribute("value", DefaultBodyHealth)),
                                new XElement("weaponForceMult", new XAttribute("value", WeaponForceMult.ToString("F2"))),
                                new XElement("swankness", new XAttribute("value", Swankness)),
                                new XElement("maxNum", new XAttribute("value", MaxNum)),
                                new XElement("frequency", new XAttribute("value", Frequency)),
                                new XElement("seats", new XAttribute("value", Seats)),
                                new XElement("plateType", new XAttribute("value", PlateType)),
                                new XElement("driveableDoors", new XAttribute("value", DriveableDoors)),
                                new XElement("flags", BuildFlags()),
                                new XElement("type", VehicleType)
                            )
                        ),
                        new XElement("txdRelationships")
                    )
                );
                vehiclesDoc.Save(folder);
                generatedFiles.Add(Path.GetFileName(folder));

                // ── handling.meta stub ────────────────────────────────────────
                if (GenerateHandling)
                {
                    var handlingPath = Path.Combine(dir, $"{ModelName}_handling.meta");
                    var handlingDoc = new XDocument(
                        new XDeclaration("1.0", "UTF-8", null),
                        new XElement("CHandlingDataMgr",
                            new XElement("HandlingData",
                                new XElement("Item",
                                    new XElement("handlingName", HandlingId.ToUpper()),
                                    new XElement("fMass", new XAttribute("value", "1500.000000")),
                                    new XElement("fInitialDragCoeff", new XAttribute("value", "11.00000")),
                                    new XElement("fDownforceModifier", new XAttribute("value", "0.00000")),
                                    new XElement("fPopUpLightRotation", new XAttribute("value", "0.00000")),
                                    new XElement("vecCentreOfMassDist", new XAttribute("x", "0.000000"), new XAttribute("y", "0.000000"), new XAttribute("z", "-0.100000")),
                                    new XElement("vecInertiaMultiplier", new XAttribute("x", "1.000000"), new XAttribute("y", "1.400000"), new XAttribute("z", "1.400000")),
                                    new XElement("fPercentSubmerged", new XAttribute("value", "85.000000")),
                                    new XElement("vecSeatOffsetDistX", new XAttribute("value", "0.000000")),
                                    new XElement("vecSeatOffsetDistY", new XAttribute("value", "0.000000")),
                                    new XElement("vecSeatOffsetDistZ", new XAttribute("value", "0.000000")),
                                    new XElement("nInitialDriveGears", new XAttribute("value", "5")),
                                    new XElement("fInitialDriveForce", new XAttribute("value", "0.18")),
                                    new XElement("fDriveInertia", new XAttribute("value", "1.000000")),
                                    new XElement("fClutchChangeRateScaleUpShift", new XAttribute("value", "2.50")),
                                    new XElement("fClutchChangeRateScaleDownShift", new XAttribute("value", "2.50")),
                                    new XElement("fInitialDriveMaxFlatVel", new XAttribute("value", "120.0")),
                                    new XElement("fBrakeForce", new XAttribute("value", "0.700000")),
                                    new XElement("fBrakeBiasFront", new XAttribute("value", "0.500000")),
                                    new XElement("fHandBrakeForce", new XAttribute("value", "0.900000")),
                                    new XElement("fSteeringLock", new XAttribute("value", "35.000000")),
                                    new XElement("fTractionCurveMax", new XAttribute("value", "2.100000")),
                                    new XElement("fTractionCurveMin", new XAttribute("value", "1.700000")),
                                    new XElement("fTractionCurveLateral", new XAttribute("value", "22.500000")),
                                    new XElement("fTractionSpringDeltaMax", new XAttribute("value", "0.150000")),
                                    new XElement("fLowSpeedTractionLossMult", new XAttribute("value", "1.0")),
                                    new XElement("fCamberStiffnessBetweenAxles", new XAttribute("value", "0.00000")),
                                    new XElement("fTractionBiasFront", new XAttribute("value", "0.47")),
                                    new XElement("fTractionLossMult", new XAttribute("value", "1.000000")),
                                    new XElement("fSuspensionForce", new XAttribute("value", "2.500000")),
                                    new XElement("fSuspensionCompDamp", new XAttribute("value", "1.400000")),
                                    new XElement("fSuspensionReboundDamp", new XAttribute("value", "2.000000")),
                                    new XElement("fSuspensionUpperLimit", new XAttribute("value", "0.120000")),
                                    new XElement("fSuspensionLowerLimit", new XAttribute("value", "-0.150000")),
                                    new XElement("fSuspensionRaise", new XAttribute("value", "0.00000")),
                                    new XElement("fSuspensionBiasFront", new XAttribute("value", "0.5")),
                                    new XElement("fAntiRollBarForce", new XAttribute("value", "0.000000")),
                                    new XElement("fAntiRollBarBiasFront", new XAttribute("value", "0.500000")),
                                    new XElement("fRollCentreHeightFront", new XAttribute("value", "0.500000")),
                                    new XElement("fRollCentreHeightRear", new XAttribute("value", "0.500000")),
                                    new XElement("fCollisionDamageMult", new XAttribute("value", "1.000000")),
                                    new XElement("fWeaponDamageMult", new XAttribute("value", "1.000000")),
                                    new XElement("fDeformationDamageMult", new XAttribute("value", "1.000000")),
                                    new XElement("fEngineDamageMult", new XAttribute("value", "1.500000")),
                                    new XElement("fPetrolTankVolume", new XAttribute("value", "65.0")),
                                    new XElement("fOilVolume", new XAttribute("value", "3.5")),
                                    new XElement("fSeatOffsetDistX", new XAttribute("value", "0.000000")),
                                    new XElement("fSeatOffsetDistY", new XAttribute("value", "0.000000")),
                                    new XElement("fSeatOffsetDistZ", new XAttribute("value", "0.000000")),
                                    new XElement("nMonetaryValue", new XAttribute("value", "50000")),
                                    new XElement("handlingFlags", "0"),
                                    new XElement("modelFlags", "0"),
                                    new XElement("vehicleClass", "VC_SEDAN")
                                )
                            )
                        )
                    );
                    handlingDoc.Save(handlingPath);
                    generatedFiles.Add(Path.GetFileName(handlingPath));
                }

                // ── carcols.meta stub ─────────────────────────────────────────
                if (GenerateCarcols)
                {
                    var carcolsPath = Path.Combine(dir, $"{ModelName}_carcols.meta");
                    var carcolsDoc = new XDocument(
                        new XDeclaration("1.0", "UTF-8", null),
                        new XElement("CVehicleModelInfoVarGlobal",
                            new XElement("Kits",
                                new XElement("Item",
                                    new XElement("kitName", $"{ModelName.ToLower()}_modkit"),
                                    new XElement("id", new XAttribute("value", "1")),
                                    new XElement("kitType", "KIT_TYPE_STANDARD"),
                                    new XElement("visibleMods"),
                                    new XElement("statMods"),
                                    new XElement("linkMods")
                                )
                            ),
                            new XElement("Sirens")
                        )
                    );
                    carcolsDoc.Save(carcolsPath);
                    generatedFiles.Add(Path.GetFileName(carcolsPath));
                }

                // ── fxmanifest.lua ────────────────────────────────────────────
                if (GenerateFxManifest)
                {
                    var manifestPath = Path.Combine(dir, "fxmanifest.lua");
                    var manifestFiles = new List<string> { $"stream/{ModelName.ToLower()}.yft", $"stream/{ModelName.ToLower()}_hi.yft", $"stream/{ModelName.ToLower()}.ytd" };
                    var manifestContent = $@"fx_version 'cerulean'
game 'gta5'

name '{ModelName.ToUpper()}'
description 'Custom vehicle add-on: {GameName}'
author 'Generated by FiveM Vehicle Meta Editor v2.0'
version '1.0.0'

files {{
  'data/**/*',
  'stream/**/*',
}}

data_file 'VEHICLE_METADATA_FILE' 'data/{ModelName.ToLower()}_vehicles.meta'
{(GenerateHandling ? $"data_file 'HANDLING_FILE' 'data/{ModelName.ToLower()}_handling.meta'" : "")}
{(GenerateCarcols ? $"data_file 'CARCOLS_FILE' 'data/{ModelName.ToLower()}_carcols.meta'" : "")}
";
                    File.WriteAllText(manifestPath, manifestContent.Trim());
                    generatedFiles.Add("fxmanifest.lua");
                }

                ShowSuccess($"Generated: {string.Join(", ", generatedFiles)}");
            }
            catch (Exception ex)
            {
                ShowError($"Error generating: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private string BuildFlags()
        {
            var flags = new List<string>();
            if (VehicleClass.Contains("EMERGENCY")) flags.Add("FLAG_IS_EMERGENCY_VEHICLE");
            if (VehicleClass.Contains("MILITARY")) flags.Add("FLAG_MILITARY");
            if (VehicleClass.Contains("EMERGENCY") && HandlingId.ToUpper().Contains("POLICE")) flags.Add("FLAG_LAW_ENFORCEMENT");
            flags.Add("FLAG_HAS_LIVERY");
            return string.Join(" ", flags);
        }

        private void ExecuteReset()
        {
            ModelName = ""; TxdName = ""; HandlingId = ""; GameName = "";
            VehicleMakeName = "ROCKSTAR"; AudioNameHash = "";
            VehicleClass = "VC_SEDAN"; VehicleType = "VEHICLE_TYPE_CAR";
            Layout = "LAYOUT_STD_CAR";
            Seats = 4; Frequency = 10; DefaultBodyHealth = 1000;
            PlateType = 0; DriveableDoors = 3;
            WheelScale = 0.268f; WheelScaleRear = 0.268f;
            DirtLevelMax = 0.5f; WeaponForceMult = 1.0f;
            Swankness = 0; MaxNum = 10;
            GenerateHandling = false; GenerateFxManifest = false; GenerateCarcols = false;
            ShowInfo("Fields reset to defaults");
        }
    }
}
