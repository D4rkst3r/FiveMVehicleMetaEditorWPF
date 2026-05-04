using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class MergerView : UserControl
    {
        public MergerView()
        {
            InitializeComponent();
            DataContext = new MergerViewModel();
        }
    }
}
