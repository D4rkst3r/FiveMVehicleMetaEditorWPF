using System;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class MergerViewModel : BaseTabViewModel
    {
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExportCommand { get; }

        public MergerViewModel()
        {
            LoadCommand = new RelayCommand(() => ShowSuccess("Load files to merge"));
            SaveCommand = new RelayCommand(() => ShowSuccess("Save merged result"));
            ExportCommand = new RelayCommand(() => ShowSuccess("Export merged data"));
        }
    }
}
