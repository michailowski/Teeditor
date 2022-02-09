using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar.PropertiesBox;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar.PropertiesBox
{
    internal sealed partial class MapEnvelopePropertiesView : UserControl
    {
        private MapEnvelopePropertiesViewModel ViewModel { get; }

        private MapEnvelopePropertiesView()
        {
            this.InitializeComponent();
        }

        public MapEnvelopePropertiesView(MapEnvelopePropertiesViewModel viewModel) : this()
        {
            DataContext = ViewModel = viewModel;
        }
    }
}
