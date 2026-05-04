using System.Windows;
using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    /// <summary>
    /// Interaction logic for HandlingView.xaml
    /// </summary>
    public partial class HandlingView : UserControl
    {
        public HandlingView()
        {
            InitializeComponent();
            // Set DataContext immediately so XAML bindings work from initialization
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            var mainVM = mainWindow?.DataContext as MainWindowViewModel;
            DataContext = new HandlingViewModel(mainVM);
        }
    }
}
