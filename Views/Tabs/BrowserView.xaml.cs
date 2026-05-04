using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class BrowserView : UserControl
    {
        public BrowserView()
        {
            InitializeComponent();
            DataContext = new BrowserViewModel();
        }
    }
}
