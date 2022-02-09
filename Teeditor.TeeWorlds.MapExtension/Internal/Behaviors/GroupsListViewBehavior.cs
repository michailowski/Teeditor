using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using DataPackageOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation;
using System.Linq;
using Microsoft.Xaml.Interactivity;
using System;
using System.Diagnostics;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Behaviors
{
    internal class GroupsListViewBehavior : Behavior<ListView>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.CanDragItems = true;
            this.AssociatedObject.DragItemsStarting += AssociatedObject_DragItemsStarting;
            this.AssociatedObject.DragOver += AssociatedObject_DragOver;
            this.AssociatedObject.Drop += AssociatedObject_Drop;
            this.AssociatedObject.ContainerContentChanging += AssociatedObject_ContainerContentChanging;

            base.OnAttached();
        }

        private void AssociatedObject_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            for (int i = 0; i < sender.Items.Count; i++)
            {
                var container = sender.ContainerFromItem(sender.Items[i]) as ListViewItem;

                if (container != null)
                {
                    container.Tag = i + 1;
                }
            }
        }

        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            if (e.DataView != null &&
                e.DataView.Properties != null &&
                e.DataView.Properties.Any(x => x.Key == "Layer" && x.Value is MapLayer))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView != null &&
                e.DataView.Properties != null &&
                e.DataView.Properties.Any(x => x.Key == "Layer" && x.Value is MapLayer) &&
                e.DataView.Properties.Any(x => x.Key == "LayerGroup" && x.Value is MapGroup) &&
                e.AcceptedOperation != DataPackageOperation.None)
            {
                try
                {
                    var def = e.GetDeferral();

                    var item = e.Data.Properties.FirstOrDefault(x => x.Key == "Layer");
                    var layer = item.Value as MapLayer;

                    var sourceGroupProperty = e.Data.Properties.FirstOrDefault(x => x.Key == "LayerGroup");
                    var sourceGroup = sourceGroupProperty.Value as MapGroup;

                    var targetListView = sender as ListView;
                    var targetGroups = targetListView.Items;

                    var insertionPanel = targetListView.ItemsPanelRoot as IInsertionPanel;

                    int aboveIndex = -1;
                    int belowIndex = -1;
                    var point = e.GetPosition(insertionPanel as UIElement);
                    insertionPanel.GetInsertionIndexes(point, out aboveIndex, out belowIndex);

                    var listViewAboveItem = targetListView.ContainerFromIndex(aboveIndex);
                    var listViewBelowItem = targetListView.ContainerFromIndex(belowIndex);

                    if (listViewAboveItem != null)
                    {
                        var groupAbove = targetListView.ItemFromContainer(listViewAboveItem) as MapGroup;
                        var targetGroup = targetGroups[targetGroups.IndexOf(groupAbove) + 1] as MapGroup;

                        if (targetGroup.UIElement != null && sourceGroup.UIElement != null && layer.UIElement != null)
                        {
                            sourceGroup.UIElement.Children.Remove(layer.UIElement);
                            targetGroup.UIElement.Children.Add(layer.UIElement);
                        }

                        targetGroup.Add(layer);
                        sourceGroup.Remove(layer);
                    }
                    else if (listViewBelowItem != null)
                    {
                        var groupBelow = targetListView.ItemFromContainer(listViewBelowItem) as MapGroup;
                        var targetGroup = targetGroups[targetGroups.IndexOf(groupBelow)] as MapGroup;

                        if (targetGroup.UIElement != null && sourceGroup.UIElement != null && layer.UIElement != null)
                        {
                            sourceGroup.UIElement.Children.Remove(layer.UIElement);
                            targetGroup.UIElement.Children.Add(layer.UIElement);
                        }

                        targetGroup.Add(layer);
                        sourceGroup.Remove(layer);
                    }
                    else
                    {
                        e.AcceptedOperation = DataPackageOperation.None;
                    }

                    def.Complete();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            else
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }

        private void AssociatedObject_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var listView = sender as ListView;

            listView.CanReorderItems = true;

            for (int i = 0; i < listView.Items.Count; i++)
            {
                var container = listView.ContainerFromItem(listView.Items[i]) as ListViewItem;

                if (container != null)
                {
                    container.AllowDrop = false;

                    var containerGrid = container.Content as Grid;
                    if (containerGrid.Children[1] is ListView)
                    {
                        var containerListView = containerGrid.Children[1] as ListView;
                        containerListView.CanReorderItems = false;
                    }
                }
            }


        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.DragItemsStarting -= AssociatedObject_DragItemsStarting;
            this.AssociatedObject.DragOver -= AssociatedObject_DragOver;
            this.AssociatedObject.Drop -= AssociatedObject_Drop;
            this.AssociatedObject.ContainerContentChanging -= AssociatedObject_ContainerContentChanging;
        }
    }
}
