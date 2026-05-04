using System;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class VIEWMODEL : BaseTabViewModel
    {
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExportCommand { get; }

        public VIEWMODEL()
        {
            LoadCommand = new RelayCommand(() => ShowSuccess("Load complete"));
            SaveCommand = new RelayCommand(() => ShowSuccess("Save complete"));
            ExportCommand = new RelayCommand(() => ShowSuccess("Export complete"));
        }
    }
}
