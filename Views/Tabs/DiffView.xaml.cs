using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class DiffView : UserControl
    {
        public DiffView()
        {
            InitializeComponent();
            var mainWindow = System.Windows.Application.Current.MainWindow as MainWindow;
            var mainVM = mainWindow?.DataContext as MainWindowViewModel;
            DataContext = new DiffViewModel(mainVM);
        }
    }
}
