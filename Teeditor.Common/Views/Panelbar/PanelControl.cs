using System;
using System.ComponentModel;
using Teeditor.Common.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.Views.Panelbar
{

    [TypeDescriptionProvider(typeof(PanelControlDescriptionProvider<PanelControl, UserControl>))]
    public abstract class PanelControl : UserControl
    {
        protected PanelViewModelBase _viewModel;
        private long _contentPropertyChangedCallbackToken;

        public IPanelViewModel ViewModel => _viewModel;

        protected PanelControl(PanelViewModelBase viewModel)
        {
            DataContext = _viewModel = viewModel;

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

            var panelContainer = new PanelContainerControl();
            Content = panelContainer;

            panelContainer.PanelContent = content;
        }
    }

    internal class PanelControlDescriptionProvider<TAbstract, TBase> : TypeDescriptionProvider
    {
        public PanelControlDescriptionProvider()
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
