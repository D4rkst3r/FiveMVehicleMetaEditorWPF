using System.Windows.Controls;
using System.Windows.Input;
using FiveMVehicleMetaEditorWPF.ViewModels;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class BrowserView : UserControl
    {
        public BrowserView()
        {
            InitializeComponent();
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            var mainVM = mainWindow?.DataContext as MainWindowViewModel;
            var vm = new BrowserViewModel(mainVM);
            DataContext = vm;

            // Wire callbacks so "Load + Navigate" actually pushes data into the target tab
            vm.OnVehiclesFileLoaded = path =>
            {
                if (mainWindow?.VehiclesTabView?.DataContext is VehiclesViewModel vehiclesVM)
                    vehiclesVM.LoadFromPath(path);
            };
            vm.OnHandlingFileLoaded = path =>
            {
                if (mainWindow?.HandlingTabView?.DataContext is HandlingViewModel handlingVM)
                    handlingVM.LoadHandlingFile(path);
            };
            vm.OnLayoutsFileLoaded = path =>
            {
                if (mainWindow?.LayoutsTabView?.DataContext is LayoutsViewModel layoutsVM)
                    layoutsVM.LoadLayoutsFile(path);
            };
        }

        private void RecentFile_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is BrowserViewModel vm &&
                sender is ListBoxItem { DataContext: RecentFileItem item })
            {
                vm.OpenRecentFileCommand.Execute(item);
            }
        }
    }
}
