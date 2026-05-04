using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class CarvariationsView : UserControl
    {
        public CarvariationsView()
        {
            InitializeComponent();
            DataContext = new CarvariationsViewModel();
        }
    }
}
