using System.Windows.Controls;
using FiveMVehicleMetaEditorWPF.ViewModels.TabViewModels;

namespace FiveMVehicleMetaEditorWPF.Views.Tabs
{
    public partial class GeneratorView : UserControl
    {
        public GeneratorView()
        {
            InitializeComponent();
            DataContext = new GeneratorViewModel();
        }
    }
}
