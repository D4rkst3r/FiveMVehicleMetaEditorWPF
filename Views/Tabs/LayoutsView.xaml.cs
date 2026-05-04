using System.Windows;
using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class LayoutsView : UserControl
    {
        public LayoutsView()
        {
            InitializeComponent();
            // Set DataContext immediately so XAML bindings work from initialization
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            var mainVM = mainWindow?.DataContext as MainWindowViewModel;
            DataContext = new LayoutsViewModel(mainVM);
        }
    }
}
