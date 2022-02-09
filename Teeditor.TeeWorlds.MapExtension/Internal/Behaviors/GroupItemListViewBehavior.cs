using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Microsoft.Xaml.Interactivity;
using System;
using Windows.UI.Xaml.Media.Animation;
using Teeditor.Common.Helpers;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Behaviors
{
    internal class GroupItemListViewBehavior : Behavior<ListViewItem>
    {
        protected override void OnAttached()
        {
            base.AssociatedObject.Tapped += AssociatedObject_Tapped;
            base.AssociatedObject.DoubleTapped += AssociatedObject_DoubleTapped;
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

        private void AssociatedObject_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var listViewItem = sender as ListViewItem;

            if (e.OriginalSource is TextBlock && (e.OriginalSource as TextBlock).Name == "GroupName")
            {
                var currentGroup = listViewItem.DataContext as MapGroup;
                if (currentGroup.HasLayers)
                {
                    currentGroup.IsExpanded = !currentGroup.IsExpanded;
                }
            }
        }

        private void AssociatedObject_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var listViewItem = sender as ListViewItem;
            var currentGroup = listViewItem.DataContext as MapGroup;

            if (e.OriginalSource is Grid && (e.OriginalSource as Grid).Name == "ExpandCollapseChevron")
            {
                if (currentGroup.HasLayers)
                {
                    currentGroup.IsExpanded = !currentGroup.IsExpanded;
                }
            }

            DependencyObject parent = listViewItem;
            while (!(parent is ListView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            var listView = parent as ListView;
            var viewModel = (ExplorerBoxViewModel)listView.DataContext;

            foreach (MapGroup group in listView.Items)
            {
                if (group.Guid != currentGroup.Guid)
                {
                    group.UnselectLayers();
                }
            }

            if (viewModel.Selection.Group != currentGroup)
                viewModel.Selection.Layer = null;

            viewModel.Selection.Group = currentGroup;
        }

        protected override void OnDetaching()
        {
            base.AssociatedObject.Tapped -= AssociatedObject_Tapped;
            base.AssociatedObject.DoubleTapped -= AssociatedObject_DoubleTapped;

            base.OnDetaching();
        }
    }
}
