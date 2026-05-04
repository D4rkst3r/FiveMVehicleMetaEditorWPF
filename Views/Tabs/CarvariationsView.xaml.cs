using System.Windows;
using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class CarvariationsView : UserControl
    {
        public CarvariationsView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                var mainVM = mainWindow?.DataContext as MainWindowViewModel;
                DataContext = new CarvariationsViewModel(mainVM);
            };
        }
    }
}
