using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class LayoutsView : UserControl
    {
        public LayoutsView()
        {
            InitializeComponent();
            DataContext = new LayoutsViewModel();
        }
    }
}
