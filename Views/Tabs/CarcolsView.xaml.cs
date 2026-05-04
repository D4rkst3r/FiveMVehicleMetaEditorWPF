using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class CarcolsView : UserControl
    {
        public CarcolsView()
        {
            InitializeComponent();
            DataContext = new CarcolsViewModel();
        }
    }
}
