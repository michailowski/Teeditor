using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using DataPackageOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation;
using Windows.UI.Xaml.Media;
using System.Linq;
using Microsoft.Xaml.Interactivity;
using System;
using System.Diagnostics;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Behaviors
{
    internal class LayersListViewBehavior : Behavior<ListView>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.CanDragItems = true;
            this.AssociatedObject.DragItemsStarting += AssociatedObject_DragItemsStarting;
            this.AssociatedObject.Drop += AssociatedObject_Drop;

            base.OnAttached();
        }

        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView != null &&
                e.DataView.Properties != null &&
                e.DataView.Properties.Any(x => x.Key == "Layer" && x.Value is MapLayer) &&
                e.DataView.Properties.Any(x => x.Key == "LayerGroup" && x.Value is MapGroup))
            {
                try
                {
                    var def = e.GetDeferral();

                    var item = e.Data.Properties.FirstOrDefault(x => x.Key == "Layer");
                    var layer = item.Value as MapLayer;

                    var sourceGroupProperty = e.Data.Properties.FirstOrDefault(x => x.Key == "LayerGroup");
                    var sourceGroup = sourceGroupProperty.Value as MapGroup;

                    var targetListView = sender as ListView;
                    var targetGroup = targetListView.DataContext as MapGroup;

                    var insertionPanel = targetListView.ItemsPanelRoot as IInsertionPanel;

                    int aboveIndex = -1;
                    int belowIndex = -1;
                    var point = e.GetPosition(insertionPanel as UIElement);
                    insertionPanel.GetInsertionIndexes(point, out aboveIndex, out belowIndex);

                    var treeViewAboveItem = targetListView.ContainerFromIndex(aboveIndex);
                    var treeViewBelowItem = targetListView.ContainerFromIndex(belowIndex);

                    if (treeViewAboveItem != null)
                    {
                        var layerAbove = targetListView.ItemFromContainer(treeViewAboveItem) as MapLayer;

                        if (targetGroup.UIElement != null && sourceGroup.UIElement != null && layer.UIElement != null)
                        {
                            sourceGroup.UIElement.Children.Remove(layer.UIElement);
                            targetGroup.UIElement.Children.Insert(targetGroup.Layers.IndexOf(layerAbove) + 1, layer.UIElement);
                        }

                        targetGroup.Insert(targetGroup.Layers.IndexOf(layerAbove) + 1, layer);
                    }
                    else if (treeViewBelowItem != null)
                    {
                        var layerBelow = targetListView.ItemFromContainer(treeViewBelowItem) as MapLayer;

                        if (targetGroup.UIElement != null && sourceGroup.UIElement != null && layer.UIElement != null)
                        {
                            sourceGroup.UIElement.Children.Remove(layer.UIElement);
                            targetGroup.UIElement.Children.Insert(targetGroup.Layers.IndexOf(layerBelow), layer.UIElement);
                        }

                        targetGroup.Insert(targetGroup.Layers.IndexOf(layerBelow), layer);
                    }
                    else
                    {
                        if (targetGroup.UIElement != null && sourceGroup.UIElement != null && layer.UIElement != null)
                        {
                            sourceGroup.UIElement.Children.Remove(layer.UIElement);
                            targetGroup.UIElement.Children.Add(layer.UIElement);
                        }

                        targetGroup.Add(layer);
                    }
                                        
                    sourceGroup.Remove(layer);

                    e.AcceptedOperation = DataPackageOperation.None;

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
            e.Data.RequestedOperation = DataPackageOperation.Move;

            if (e.Items != null && e.Items.Any())
            {
                var listView = sender as ListView;

                e.Data.Properties.Add("Layer", e.Items.FirstOrDefault());
                e.Data.Properties.Add("LayerGroup", listView.DataContext);

                DependencyObject parent = VisualTreeHelper.GetParent(listView);

                while (!(parent is ListView))
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                var groupsListView = parent as ListView;

                for (int i = 0; i < groupsListView.Items.Count; i++)
                {
                    var container = groupsListView.ContainerFromItem(groupsListView.Items[i]) as ListViewItem;

                    if (container != null)
                    {
                        container.AllowDrop = true;

                        var containerGrid = container.Content as Grid;
                        if (containerGrid.Children[1] is ListView)
                        {
                            var containerListView = containerGrid.Children[1] as ListView;
                            containerListView.CanReorderItems = true;
                        }
                    }
                }

                groupsListView.CanReorderItems = false;
            }
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.DragItemsStarting -= AssociatedObject_DragItemsStarting;
            this.AssociatedObject.Drop -= AssociatedObject_Drop;

            base.OnDetaching();
        }
    }
}
