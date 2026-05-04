using System.Collections.ObjectModel;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;
using FiveMVehicleMetaEditorWPF.Core.Models;
using FiveMVehicleMetaEditorWPF.Core.Services;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class VehiclesViewModel : BaseTabViewModel
    {
        private readonly MetaVehiclesService _metaService = new();
        private Vehicle? _selectedVehicle;
        private int _vehicleCount = 0;

        public ObservableCollection<Vehicle> Vehicles { get; } = new();
        public ObservableCollection<Vehicle> FilteredVehicles { get; } = new();

        public Vehicle? SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                if (_selectedVehicle != value)
                {
                    _selectedVehicle = value;
                    OnPropertyChanged();
                }
            }
        }

        public int VehicleCount
        {
            get => _vehicleCount;
            set
            {
                if (_vehicleCount != value)
                {
                    _vehicleCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand EditVehicleCommand { get; }

        public VehiclesViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            EditVehicleCommand = new RelayCommand(param =>
            {
                if (param is Vehicle vehicle)
                    SelectedVehicle = vehicle;
            });

            ShowInfo("Load vehicles.meta to get started");
        }

        protected override void OnLoad()
        {
            ShowInfo("Loading vehicles.meta...");
            IsLoading = true;
            // TODO: Open file dialog and load
        }

        protected override void OnSave()
        {
            ShowInfo("Saving vehicles.meta...");
            IsLoading = true;
            // TODO: Save vehicles
        }

        protected override void OnExport()
        {
            ShowInfo("Exporting vehicles...");
            // TODO: Export to JSON
        }

        protected override void OnSearchChanged(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                FilteredVehicles.Clear();
                foreach (var v in Vehicles)
                    FilteredVehicles.Add(v);
            }
            else
            {
                var filtered = _metaService.FilterVehicles(
                    new System.Collections.Generic.List<Vehicle>(Vehicles),
                    searchText
                );
                FilteredVehicles.Clear();
                foreach (var v in filtered)
                    FilteredVehicles.Add(v);
            }
        }
    }
}
