using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Input;
using System.Collections.Generic;
using Teeditor.Common.Views.Sidebar;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar
{
    internal sealed partial class ExplorerBoxControl : BoxControl
    {
        private new ExplorerBoxViewModel ViewModel => (ExplorerBoxViewModel) _viewModel;

        public ExplorerBoxControl(ExplorerBoxViewModel viewModel)
            : base(viewModel)
        {
            this.InitializeComponent();
            this.ContextRequested += ExplorerBoxControl_ContextRequested;
        }

        private void ExplorerBoxControl_ContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            DependencyObject parent = args.OriginalSource as DependencyObject;

            while (!(parent is ListViewItem))
            {
                if (parent == null)
                    return;

                parent = VisualTreeHelper.GetParent(parent);
            }

            var listViewItem = parent as ListViewItem;

            FlyoutShowOptions myOption = new FlyoutShowOptions();
            myOption.ShowMode = FlyoutShowMode.Standard;

            if (listViewItem.DataContext is MapGroup)
            {
                var group = listViewItem.DataContext as MapGroup;

                if (group.IsGameGroup)
                    GameGroupContextFlyout.ShowAt(listViewItem, myOption);
                else
                    CommonGroupContextFlyout.ShowAt(listViewItem, myOption);
            }
            else if (listViewItem.DataContext is MapTilesLayer)
            {
                var layer = listViewItem.DataContext as MapTilesLayer;

                if (layer.IsGameLayer)
                    GameLayerContextFlyout.ShowAt(listViewItem, myOption);
                else
                    CommonLayerContextFlyout.ShowAt(listViewItem, myOption);
            }
            else if (listViewItem.DataContext is MapQuadsLayer)
            {
                CommonLayerContextFlyout.ShowAt(listViewItem, myOption);
            }
        }

        private void GroupsListView_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Space:
                case Windows.System.VirtualKey.Left:
                case Windows.System.VirtualKey.Right:
                case Windows.System.VirtualKey.Up:
                case Windows.System.VirtualKey.Down:
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        private void PropertiesBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = (FrameworkElement) sender;

            if (button.DataContext is MapItem mapItem)
            {
                ViewModel.OpenPropertiesBox(mapItem);
            }
        }

        private void MapPropertiesBtn_Click(object sender, RoutedEventArgs e) => ViewModel.OpenMapPropertiesBox();

        private void MinimizeOtherGroupsBtn_Click(object sender, RoutedEventArgs e) =>
            ViewModel.MinimizeOtherGroups((sender as FrameworkElement).DataContext as MapGroup);

        private void MinimizeAllGroupsBtn_Click(object sender, RoutedEventArgs e) => ViewModel.MinimizeAllGroups();
        
        private void MaximizeAllGroupsBtn_Click(object sender, RoutedEventArgs e) => ViewModel.MaximizeAllGroups();
        
        private void CreateGroupBtn_Click(object sender, RoutedEventArgs e) => ViewModel.AddNewGroup();
        
        private void CreateTilesLayerBtn_Click(object sender, RoutedEventArgs e) =>
            ViewModel.AddTilesLayer((sender as FrameworkElement).DataContext as MapGroup);

        private void CreateQuadsLayerBtn_Click(object sender, RoutedEventArgs e) =>
            ViewModel.AddQuadsLayer((sender as FrameworkElement).DataContext as MapGroup);

        private void RemoveGroupBtn_Click(object sender, RoutedEventArgs e) =>
            ViewModel.RemoveGroup((sender as FrameworkElement).DataContext as MapGroup);
        
        private void RemoveLayerBtn_Click(object sender, RoutedEventArgs e) =>
            ViewModel.RemoveLayer((sender as FrameworkElement).DataContext as MapLayer);

        protected override IList<MenuFlyoutItemBase> GetMenuFlyoutItems()
        {
            return BoxMenuFlyout.Items;
        }
    }
}
