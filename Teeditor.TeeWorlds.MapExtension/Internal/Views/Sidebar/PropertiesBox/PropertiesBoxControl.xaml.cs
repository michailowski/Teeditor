using System.ComponentModel;
using Teeditor.Common.Views.Sidebar;
using Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar.PropertiesBox;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar.PropertiesBox;
using Windows.UI.Xaml;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar
{
    internal sealed partial class PropertiesBoxControl : BoxControl
    {
        private new PropertiesBoxViewModel ViewModel => (PropertiesBoxViewModel)_viewModel;

        public PropertiesBoxControl(PropertiesBoxViewModel viewModel)
            : base(viewModel)
        {
            this.InitializeComponent();
            this.RegisterPropertyChangedCallback(VisibilityProperty, PropertiesBoxControl_VisibilityChanged);

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "PropertyBoxItem")
            {
                PropertyViewContainer.Content = GetMappedPropertyItemToView(ViewModel?.PropertyBoxItem);
            }
        }

        private UIElement GetMappedPropertyItemToView(MapItem mapItem)
        {
            if (mapItem == null)
                return null;

            if (mapItem is MapInfo)
            {
                var viewModel = new MapInfoPropertiesViewModel((MapInfo)mapItem);
                return new MapInfoPropertiesView(viewModel);
            }
            else if (mapItem is MapGroup && (mapItem as MapGroup).IsGameGroup == false)
            {
                var viewModel = new MapCommonGroupPropertiesViewModel((MapGroup)mapItem);
                return new MapCommonGroupPropertiesView(viewModel);
            }
            else if (mapItem is MapTilesLayer && (mapItem as MapTilesLayer).IsGameLayer == true)
            {
                var viewModel = new MapGameLayerPropertiesViewModel((MapTilesLayer)mapItem);
                return new MapGameLayerPropertiesView(viewModel);
            }
            else if (mapItem is MapTilesLayer && (mapItem as MapTilesLayer).IsGameLayer == false)
            {
                var viewModel = new MapTilesLayerPropertiesViewModel((MapTilesLayer)mapItem, ViewModel?.Map?.ImagesContainer, ViewModel?.Map?.EnvelopesContainer);
                return new MapTilesLayerPropertiesView(viewModel);
            }
            else if (mapItem is MapQuadsLayer)
            {
                var viewModel = new MapQuadsLayerPropertiesViewModel((MapQuadsLayer)mapItem, ViewModel?.Map?.ImagesContainer);
                return new MapQuadsLayerPropertiesView(viewModel);
            }
            else if (mapItem is MapEnvelope)
            {
                var viewModel = new MapEnvelopePropertiesViewModel((MapEnvelope)mapItem);
                return new MapEnvelopePropertiesView(viewModel);
            }

            return null;
        }

        private void PropertiesBoxControl_VisibilityChanged(DependencyObject sender, DependencyProperty dp)
        {
            if (base.Visibility == Visibility.Collapsed)
            {
                ViewModel.ResetPropertyBoxItem();
            }
        }
    }
}
