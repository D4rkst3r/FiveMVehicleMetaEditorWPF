using System.Windows;
using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class GeneratorView : UserControl
    {
        public GeneratorView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                var mainVM = mainWindow?.DataContext as MainWindowViewModel;
                DataContext = new GeneratorViewModel(mainVM);
            };
        }
    }
}
