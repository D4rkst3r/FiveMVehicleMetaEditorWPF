using System;
using System.IO;
using System.Windows.Input;
using System.Xml.Linq;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Services;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class GeneratorViewModel : BaseTabViewModel
    {
        private string _modelName = "";
        private string _txdName = "";
        private string _handlingId = "";
        private string _gameName = "";
        private string _vehicleMakeName = "";
        private string _vehicleClass = "VC_SEDAN";
        private string _vehicleType = "AUTOMOBILE";
        private int _seats = 4;

        public string ModelName
        {
            get => _modelName;
            set { _modelName = value; OnPropertyChanged(); }
        }
        public string TxdName
        {
            get => _txdName;
            set { _txdName = value; OnPropertyChanged(); }
        }
        public string HandlingId
        {
            get => _handlingId;
            set { _handlingId = value; OnPropertyChanged(); }
        }
        public string GameName
        {
            get => _gameName;
            set { _gameName = value; OnPropertyChanged(); }
        }
        public string VehicleMakeName
        {
            get => _vehicleMakeName;
            set { _vehicleMakeName = value; OnPropertyChanged(); }
        }
        public string VehicleClass
        {
            get => _vehicleClass;
            set { _vehicleClass = value; OnPropertyChanged(); }
        }
        public string VehicleType
        {
            get => _vehicleType;
            set { _vehicleType = value; OnPropertyChanged(); }
        }
        public int Seats
        {
            get => _seats;
            set { _seats = value; OnPropertyChanged(); }
        }

        public ICommand GenerateCommand { get; }
        public ICommand ResetCommand { get; }

        public GeneratorViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            GenerateCommand = new RelayCommand(ExecuteGenerate);
            ResetCommand = new RelayCommand(ExecuteReset);

            ShowInfo("Fill in the fields and click Generate to create a vehicles.meta");
        }

        private void ExecuteGenerate()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ModelName))
                {
                    ShowError("Model Name is required");
                    return;
                }
                if (string.IsNullOrWhiteSpace(HandlingId))
                {
                    ShowError("Handling ID is required");
                    return;
                }

                IsLoading = true;
                ShowInfo("Generating vehicles.meta...");

                var txd = string.IsNullOrWhiteSpace(TxdName) ? ModelName : TxdName;
                var game = string.IsNullOrWhiteSpace(GameName) ? ModelName.ToUpper() : GameName;

                var doc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", null),
                    new XElement("CVehicleModelInfo__InitDataList",
                        new XElement("InitDatas",
                            new XElement("Item",
                                new XElement("modelName", ModelName),
                                new XElement("txdName", txd),
                                new XElement("handlingId", HandlingId),
                                new XElement("gameName", game),
                                new XElement("vehicleMakeName", VehicleMakeName),
                                new XElement("expressionDictName", "null"),
                                new XElement("expressionName", "null"),
                                new XElement("animConvRoofDictName", "null"),
                                new XElement("animConvRoofName", "null"),
                                new XElement("ptfxAssetName", "null"),
                                new XElement("audioNameHash"),
                                new XElement("layout", "LAYOUT_STD_CAR"),
                                new XElement("cameraName", "DEFAULT_FOLLOW_VEHICLE_CAMERA"),
                                new XElement("aimCameraName", "DEFAULT_THIRD_PERSON_VEHICLE_AIM_CAMERA"),
                                new XElement("bonnetCameraName", "VEHICLE_BONNET_CAMERA_MID_NEAR"),
                                new XElement("povCameraName", "REDUCED_NEAR_CLIP_POV_CAMERA"),
                                new XElement("vehicleClass", VehicleClass),
                                new XElement("vehicleType", VehicleType),
                                new XElement("wheelScale", new XAttribute("value", "0.268")),
                                new XElement("wheelScaleRear", new XAttribute("value", "0.268")),
                                new XElement("dirtLevelMin", new XAttribute("value", "0.0")),
                                new XElement("dirtLevelMax", new XAttribute("value", "0.5")),
                                new XElement("seats", new XAttribute("value", Seats)),
                                new XElement("maxNum", new XAttribute("value", "10")),
                                new XElement("frequency", new XAttribute("value", "10")),
                                new XElement("flags", "FLAG_HAS_LIVERY"),
                                new XElement("type", "VEHICLE_TYPE_CAR")
                            )
                        )
                    )
                );

                var savePath = FileService.SaveFileDialog("vehicles", $"{ModelName}.meta");
                if (savePath == null) return;

                doc.Save(savePath);
                ShowSuccess($"Generated {Path.GetFileName(savePath)} successfully!");
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

        private void ExecuteReset()
        {
            ModelName = "";
            TxdName = "";
            HandlingId = "";
            GameName = "";
            VehicleMakeName = "";
            VehicleClass = "VC_SEDAN";
            VehicleType = "AUTOMOBILE";
            Seats = 4;
            ShowInfo("Fields reset to defaults");
        }
    }
}
