using System;
using System.ComponentModel;
using Teeditor.Common.Utilities;
using Teeditor.Common.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.Views.Toolbar
{

    [TypeDescriptionProvider(typeof(ToolControlDescriptionProvider<ToolControl, UserControl>))]
    public abstract class ToolControl : UserControl
    {
        protected ToolViewModelBase _viewModel;
        private long _contentPropertyChangedCallbackToken;

        public IToolViewModel ViewModel => _viewModel;

        internal event EventHandler<ToolControl> DropToLeftNeeded;
        internal event EventHandler<ToolControl> DropToRightNeeded;

        protected ToolControl(ToolViewModelBase viewModel)
        {
            DataContext = _viewModel = viewModel;

            CanDrag = true;
            DragStarting += ToolControl_DragStarting;
            DropCompleted += ToolControl_DropCompleted;

            _contentPropertyChangedCallbackToken = RegisterPropertyChangedCallback(ContentProperty, PropertyChangedCallback);
        }

        private void PropertyChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            if (dp == ContentProperty)
            {
                PrepareContainer();
            }
        }

        private void PrepareContainer()
        {
            UnregisterPropertyChangedCallback(ContentProperty, _contentPropertyChangedCallbackToken);

            var content = Content;

            var toolContainer = new ToolContainerControl();
            Content = toolContainer;

            toolContainer.ToolContent = content;

            toolContainer.DropToLeftNeeded += ToolContainer_DropToLeftNeeded;
            toolContainer.DropToRightNeeded += ToolContainer_DropToRightNeeded;
        }

        private void ToolContainer_DropToRightNeeded(object sender, ToolControl e)
            => DropToRightNeeded?.Invoke(this, e);

        private void ToolContainer_DropToLeftNeeded(object sender, ToolControl e)
            => DropToLeftNeeded?.Invoke(this, e);

        private async void ToolControl_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            var toolContainer = (ToolContainerControl)Content;
            var position = args.GetPosition(toolContainer.Divider);

            if (position.X > toolContainer.Divider.ActualSize.X)
            {
                args.Cancel = true;
                return;
            }

            args.Data.Properties.Add("DraggedTool", this);
            args.Data.Properties.Add("DraggedToolContainer", toolContainer);

            var deferral = args.GetDeferral();

            var bitmap = await UserInterface.GetUIBitmapAsync(sender, 80);
            var anchorPoint = new Windows.Foundation.Point(5, 5);

            args.DragUI.SetContentFromSoftwareBitmap(bitmap, anchorPoint);

            deferral.Complete();

            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeAll, 0);

            Visibility = Visibility.Collapsed;
        }

        private void ToolControl_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
            Visibility = Visibility.Visible;
        }
    }

    internal class ToolControlDescriptionProvider<TAbstract, TBase> : TypeDescriptionProvider
    {
        public ToolControlDescriptionProvider()
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
