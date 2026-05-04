using System;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class GeneratorViewModel : BaseTabViewModel
    {
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExportCommand { get; }

        public GeneratorViewModel()
        {
            LoadCommand = new RelayCommand(() => ShowSuccess("Load template"));
            SaveCommand = new RelayCommand(() => ShowSuccess("Save generated vehicles"));
            ExportCommand = new RelayCommand(() => ShowSuccess("Export generated data"));
        }
    }
}
