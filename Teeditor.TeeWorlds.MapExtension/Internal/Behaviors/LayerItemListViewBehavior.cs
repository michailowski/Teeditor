using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml.Media.Animation;
using System;
using Teeditor.Common.Helpers;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Behaviors
{
    internal class LayerItemListViewBehavior : Behavior<ListViewItem>
    {
        protected override void OnAttached()
        {
            base.AssociatedObject.Tapped += AssociatedObject_Tapped;

            base.AssociatedObject.PointerEntered += AssociatedObject_PointerEntered;
            base.AssociatedObject.PointerExited += AssociatedObject_PointerExited;

            base.OnAttached();
        }

        private void AssociatedObject_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var listViewItem = sender as ListViewItem;

            var btns = VisualHierarchyHelper.FindChild<StackPanel>(listViewItem, "Buttons");

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation();
            Storyboard.SetTargetName(animation, btns.Name);
            Storyboard.SetTarget(animation, btns);
            Storyboard.SetTargetProperty(animation, "Width");
            animation.EnableDependentAnimation = true;
            animation.From = btns.Width;
            animation.To = 0;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void AssociatedObject_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var listViewItem = sender as ListViewItem;

            var btns = VisualHierarchyHelper.FindChild<StackPanel>(listViewItem, "Buttons");
            var btnsGrid = btns.Children[0] as Grid;

            double width = 0;

            for (int i = 0; i < btnsGrid.ColumnDefinitions.Count; i++)
            {
                width += btnsGrid.ColumnDefinitions[i].Width.Value;
            }

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation();
            Storyboard.SetTargetName(animation, btns.Name);
            Storyboard.SetTarget(animation, btns);
            Storyboard.SetTargetProperty(animation, "Width");
            animation.EnableDependentAnimation = true;
            animation.From = btns.Width;
            animation.To = width;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void AssociatedObject_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var listViewItem = sender as ListViewItem;
            var currentLayer = listViewItem.DataContext as MapLayer;

            DependencyObject parent = listViewItem;
            while (!(parent is ListViewItem && (parent as ListViewItem).DataContext is MapGroup))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            var groupItem = parent as ListViewItem;
            var currentGroup = groupItem.DataContext as MapGroup;

            while (!(parent is ListView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            var listView = parent as ListView;
            var viewModel = (ExplorerBoxViewModel)listView.DataContext;
            var groups = listView.Items;

            foreach (MapGroup group in groups)
            {
                if (group.Guid != currentGroup.Guid)
                {
                    group.UnselectLayers();

                    if (group.IsSelected)
                    {
                        group.IsSelected = false;
                    }
                }
            }

            viewModel.Selection.Layer = currentLayer;
            viewModel.Selection.Group = currentGroup;

            currentGroup.IsSelected = true;
        }

        protected override void OnDetaching()
        {
            base.AssociatedObject.Tapped -= AssociatedObject_Tapped;

            base.OnDetaching();
        }
    }
}
