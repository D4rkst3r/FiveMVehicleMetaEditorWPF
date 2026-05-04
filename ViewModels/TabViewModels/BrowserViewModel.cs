using System.Collections.ObjectModel;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class BrowserViewModel : BaseTabViewModel
    {
        private int _vehicleCount = 0;
        private int _layoutCount = 0;
        private string _selectedFile = "";

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

        public int LayoutCount
        {
            get => _layoutCount;
            set
            {
                if (_layoutCount != value)
                {
                    _layoutCount = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedFile
        {
            get => _selectedFile;
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<string> RecentFiles { get; } = new();
        public ObservableCollection<string> Vehicles { get; } = new();

        public ICommand LoadVehiclesCommand { get; }
        public ICommand LoadHandlingCommand { get; }
        public ICommand LoadLayoutsCommand { get; }

        public BrowserViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            LoadVehiclesCommand = new RelayCommand(_ => OnLoadVehicles());
            LoadHandlingCommand = new RelayCommand(_ => OnLoadHandling());
            LoadLayoutsCommand = new RelayCommand(_ => OnLoadLayouts());

            ShowInfo("Ready to load meta files");
        }

        private void OnLoadVehicles()
        {
            ShowInfo("Load vehicles.meta...");
            // TODO: Implement file dialog and loading
        }

        private void OnLoadHandling()
        {
            ShowInfo("Load handling.meta...");
            // TODO: Implement file dialog and loading
        }

        private void OnLoadLayouts()
        {
            ShowInfo("Load vehiclelayouts.meta...");
            // TODO: Implement file dialog and loading
        }
    }
}
