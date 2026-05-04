using System;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class CarcolsViewModel : BaseTabViewModel
    {
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExportCommand { get; }

        public CarcolsViewModel()
        {
            LoadCommand = new RelayCommand(() => ShowSuccess("Load carcols.meta"));
            SaveCommand = new RelayCommand(() => ShowSuccess("Save carcols.meta"));
            ExportCommand = new RelayCommand(() => ShowSuccess("Export carcols"));
        }
    }
}
