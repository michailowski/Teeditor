using Windows.UI.Xaml.Controls;
using System.Collections.Specialized;
using System.Collections.Generic;
using Teeditor.Common.Views.Sidebar;
using Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Sidebar;
using Teeditor.Common.Models.Commands;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Views.Sidebar
{
    internal sealed partial class HistoryBoxControl : BoxControl
    {
        private new HistoryBoxViewModel ViewModel => (HistoryBoxViewModel)_viewModel;

        public HistoryBoxControl(HistoryBoxViewModel viewModel)
            : base(viewModel)
        {
            this.InitializeComponent();

            viewModel.CommandAdded += ViewModel_CommandAdded;
        }

        private void ViewModel_CommandAdded(object sender, object e)
        {
            ChangesHistoryListView.ScrollIntoView(e);
        }

        private void ChangesHistoryListView_ItemClick(object sender, ItemClickEventArgs e) =>
            ViewModel.GoToCommand(e.ClickedItem as IUndoRedoableCommand);
    }
}
