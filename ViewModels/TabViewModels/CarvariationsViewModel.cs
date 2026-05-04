using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class CarvariationsViewModel : BaseTabViewModel
    {
        private ObservableCollection<string> _variations = new();
        public ObservableCollection<string> Variations
        {
            get => _variations;
            set { _variations = value; OnPropertyChanged(); }
        }

        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExportCommand { get; }

        public CarvariationsViewModel()
        {
            LoadCommand = new RelayCommand(() => ShowSuccess("Load carvariations.meta"));
            SaveCommand = new RelayCommand(() => ShowSuccess("Save carvariations.meta"));
            ExportCommand = new RelayCommand(() => ShowSuccess("Export carvariations"));
        }
    }
}
