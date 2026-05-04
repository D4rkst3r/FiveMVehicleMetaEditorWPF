using System.Windows;
using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class MergerView : UserControl
    {
        public MergerView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                var mainVM = mainWindow?.DataContext as MainWindowViewModel;
                DataContext = new MergerViewModel(mainVM);
            };
        }
    }
}
