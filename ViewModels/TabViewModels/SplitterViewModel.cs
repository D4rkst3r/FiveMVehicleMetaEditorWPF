using System;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.Core;

namespace FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels
{
    public class SplitterViewModel : BaseTabViewModel
    {
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ExportCommand { get; }

        public SplitterViewModel(MainWindowViewModel? mainVM = null) : base(mainVM)
        {
            LoadCommand = new RelayCommand(() => ShowSuccess("Load file to split"));
            SaveCommand = new RelayCommand(() => ShowSuccess("Save split files"));
            ExportCommand = new RelayCommand(() => ShowSuccess("Export split data"));
        }
    }
}
