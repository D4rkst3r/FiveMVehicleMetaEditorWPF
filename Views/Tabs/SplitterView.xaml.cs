using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class SplitterView : UserControl
    {
        public SplitterView()
        {
            InitializeComponent();
            DataContext = new SplitterViewModel();
        }
    }
}
