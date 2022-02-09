using System;
using Teeditor.Common.Models.Panelbar;
using Teeditor.Common.Views.Panelbar;
using Teeditor.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Views
{
    internal sealed partial class PanelbarControl : UserControl
    {
        public PanelbarViewModel Source
        {
            get => (PanelbarViewModel)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(PanelbarViewModel),
                typeof(PanelbarControl), new PropertyMetadata(null, SourceProperty_ChangedCallback));

        public PanelbarControl()
        {
            this.InitializeComponent();
        }

        private static void SourceProperty_ChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;

            var control = (PanelbarControl)d;

            var oldViewModel = (PanelbarViewModel)e.OldValue;

            if (oldViewModel != null)
            {
                oldViewModel.TabUpdated -= control.Source_TabUpdated;
            }

            if (e.NewValue == null)
                return;

            var newViewModel = (PanelbarViewModel)e.NewValue;
            newViewModel.TabUpdated += control.Source_TabUpdated;
        }

        private void Source_TabUpdated(object sender, EventArgs e)
            => ResetPanel();

        private void ResetPanel()
        {
            PanelView.Child = null;

            PanelList.ItemsSource = Source.Panels;
            PanelList.SelectedIndex = Source.SelectedItemIndex;
        }

        private void PanelList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                PanelView.Child = null;

                var item = e.AddedItems[0] as PanelItem;
                Source.SetSelectedPanel(item);

                PanelView.Child = item.Panel;
            }
            else if (e.RemovedItems.Count > 0)
            {
                var item = e.RemovedItems[0] as PanelItem;
                PanelView.Child = null;
            }
        }

        private void PanelList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var list = sender as ListView;
            var listItem = list.ContainerFromItem(e.ClickedItem) as ListViewItem;

            if (listItem.IsSelected)
            {
                listItem.IsSelected = false;
                list.SelectionMode = ListViewSelectionMode.None;
            }
            else
            {
                list.SelectionMode = ListViewSelectionMode.Single;
                listItem.IsSelected = true;
            }
        }
    }
}