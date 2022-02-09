using System;
using Teeditor.Common.Models.Toolbar;
using Teeditor.ViewModels.Toolbar;
using Teeditor.Views.Toolbar.Tools;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace Teeditor.Views
{
    internal sealed partial class ToolbarControl : UserControl
    {
        public ToolbarViewModel Source
        {
            get => (ToolbarViewModel)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ToolbarViewModel),
                typeof(ToolbarControl), new PropertyMetadata(null, SourceProperty_ChangedCallback));

        private MainToolControl _mainToolControl;

        public ToolbarControl()
        {
            this.InitializeComponent();
        }

        private static void SourceProperty_ChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;

            var control = (ToolbarControl) d;

            var oldViewModel = (ToolbarViewModel) e.OldValue;

            if (oldViewModel != null)
            {
                oldViewModel.TabUpdated -= control.Source_TabUpdated;
                oldViewModel.ToolOrderChanged -= control.Source_ToolOrderChanged;
            }

            if (e.NewValue == null)
                return;

            var newViewModel = (ToolbarViewModel) e.NewValue;
            newViewModel.TabUpdated += control.Source_TabUpdated;
            newViewModel.ToolOrderChanged += control.Source_ToolOrderChanged;

            control._mainToolControl = new MainToolControl(newViewModel.MainToolViewModel);
        }

        private void Source_ToolOrderChanged(object sender, ToolbarItemChangedEventArgs e)
            => ResetItems();

        private void Source_TabUpdated(object sender, EventArgs e)
            => ResetItems();

        private void ResetItems()
        {
            Tools.Children.Clear();

            if (Source?.Tools == null)
                return;

            Tools.Children.Add(_mainToolControl);

            foreach (var tool in Source.Tools)
            {
                Tools.Children.Add(tool);
            }
        }
    }
}
