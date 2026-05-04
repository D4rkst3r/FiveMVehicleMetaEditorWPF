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
            // Set DataContext immediately so XAML bindings work
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            var mainVM = mainWindow?.DataContext as MainWindowViewModel;
            DataContext = new BrowserViewModel(mainVM);
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
