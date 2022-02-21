using System;
using System.Collections.Generic;
using System.ComponentModel;
using Teeditor.Common.Models.Sidebar;
using Teeditor.Common.Utilities;
using Teeditor.Common.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.Views.Sidebar
{
    [TypeDescriptionProvider(typeof(BoxControlDescriptionProvider<BoxControl, UserControl>))]
    public abstract class BoxControl : UserControl
    {
        protected BoxViewModelBase _viewModel;
        private long _contentPropertyChangedCallbackToken;

        public IBoxViewModel ViewModel => _viewModel;

        internal event EventHandler DockToggleNeeded;
        internal event EventHandler MoveUpNeeded;
        internal event EventHandler MoveDownNeeded;
        internal event EventHandler VisibilityChanged;

        internal event EventHandler<BoxControl> DropToUpNeeded;
        internal event EventHandler<BoxControl> DropToDownNeeded;

        protected BoxControl(BoxViewModelBase viewModel)
        {
            DataContext = _viewModel = viewModel;

            CanDrag = true;
            DragStarting += BoxControl_DragStarting;
            DropCompleted += BoxControl_DropCompleted;

            MinWidth = 220;
            Visibility = _viewModel.IsActive ? Visibility.Visible : Visibility.Collapsed;

            _contentPropertyChangedCallbackToken = RegisterPropertyChangedCallback(ContentProperty, PropertyChangedCallback);
            RegisterPropertyChangedCallback(VisibilityProperty, PropertyChangedCallback);
        }

        private void PropertyChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            if (dp == ContentProperty)
            {
                PrepareContainer();
            }
            else if (dp == VisibilityProperty)
            {
                _viewModel.IsActive = Visibility == Visibility.Visible;
                VisibilityChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private void PrepareContainer()
        {
            UnregisterPropertyChangedCallback(ContentProperty, _contentPropertyChangedCallbackToken);

            var content = Content;

            var boxContainer = new BoxContainerControl();
            Content = boxContainer;

            boxContainer.Label = _viewModel.Label;
            boxContainer.BoxContent = content;
            boxContainer.MenuItems = GetMenuFlyoutItems();

            boxContainer.CloseNeeded += BoxContainer_CloseNeeded;
            boxContainer.DockToggleNeeded += BoxContainer_DockToggleNeeded;
            boxContainer.MoveUpNeeded += BoxContainer_MoveUpNeeded;
            boxContainer.MoveDownNeeded += BoxContainer_MoveDownNeeded;

            boxContainer.DropToUpNeeded += BoxContainer_DropToUpNeeded;
            boxContainer.DropToDownNeeded += BoxContainer_DropToDownNeeded;
        }

        public void ToggleDock()
        {
            _viewModel.Dock = _viewModel.Dock == SidebarDock.Left ? SidebarDock.Right : SidebarDock.Left;
            DockToggleNeeded?.Invoke(this, EventArgs.Empty);
            DoAfterDockChanging();
        }

        protected virtual void DoAfterDockChanging()
        {
            // Do nothing
        }

        protected virtual IList<MenuFlyoutItemBase> GetMenuFlyoutItems()
        {
            return new List<MenuFlyoutItemBase>();
        }

        private void BoxContainer_MoveDownNeeded(object sender, EventArgs e)
        {
            MoveDownNeeded?.Invoke(this, EventArgs.Empty);
        }

        private void BoxContainer_MoveUpNeeded(object sender, EventArgs e)
        {
            MoveUpNeeded?.Invoke(this, EventArgs.Empty);
        }

        private void BoxContainer_DockToggleNeeded(object sender, EventArgs e) => ToggleDock();

        private void BoxContainer_DropToUpNeeded(object sender, BoxControl e)
        {
            DropToUpNeeded?.Invoke(this, e);

            if (ViewModel.Dock != e.ViewModel.Dock)
            {
                e.ToggleDock();
            }
        }

        private void BoxContainer_DropToDownNeeded(object sender, BoxControl e)
        {
            DropToDownNeeded?.Invoke(this, e);

            if (ViewModel.Dock != e.ViewModel.Dock)
            {
                e.ToggleDock();
            }
        }

        private void BoxContainer_CloseNeeded(object sender, EventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private async void BoxControl_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            var boxContainer = (BoxContainerControl)Content;
            var position = args.GetPosition(boxContainer.Header);

            if (position.X > boxContainer.Header.ActualSize.X || position.Y > boxContainer.Header.ActualSize.Y)
            {
                args.Cancel = true;
                return;
            }

            args.Data.Properties.Add("DraggedBox", this);
            args.Data.Properties.Add("DraggedBoxContainer", boxContainer);

            var deferral = args.GetDeferral();

            var bitmap = await UserInterface.GetUIBitmapAsync(sender, 80);
            var anchorPoint = new Windows.Foundation.Point(5, 5);

            args.DragUI.SetContentFromSoftwareBitmap(bitmap, anchorPoint);

            deferral.Complete();

            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 0);
        }

        private void BoxControl_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
        }
    }

    internal class BoxControlDescriptionProvider<TAbstract, TBase> : TypeDescriptionProvider
    {
        public BoxControlDescriptionProvider()
            : base(TypeDescriptor.GetProvider(typeof(TAbstract)))
        {
        }

        public override Type GetReflectionType(Type objectType, object instance)
        {
            if (objectType == typeof(TAbstract))
                return typeof(TBase);

            return base.GetReflectionType(objectType, instance);
        }

        public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
        {
            if (objectType == typeof(TAbstract))
                objectType = typeof(TBase);

            return base.CreateInstance(provider, objectType, argTypes, args);
        }
    }
}
