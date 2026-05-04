using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    /// <summary>
    /// Interaction logic for HandlingView.xaml
    /// </summary>
    public partial class HandlingView : UserControl
    {
        public HandlingView()
        {
            InitializeComponent();
            DataContext = new HandlingViewModel();
        }
    }
}
