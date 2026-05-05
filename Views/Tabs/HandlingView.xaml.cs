using System.Windows;
using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class HandlingView : UserControl
    {
        public HandlingView()
        {
            InitializeComponent();
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            var mainVM = mainWindow?.DataContext as MainWindowViewModel;
            DataContext = new HandlingViewModel(mainVM);
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void OnFileDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files?.Length > 0 && DataContext is HandlingViewModel vm)
                vm.LoadHandlingFile(files[0]);
        }
    }
}
