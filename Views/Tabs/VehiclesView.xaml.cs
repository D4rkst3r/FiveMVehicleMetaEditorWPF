using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class VehiclesView : UserControl
    {
        public VehiclesView()
        {
            InitializeComponent();
            DataContext = new VehiclesViewModel();
        }
    }
}
