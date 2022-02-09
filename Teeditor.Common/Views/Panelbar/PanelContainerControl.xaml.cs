using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.Views.Panelbar
{
    public sealed partial class PanelContainerControl : UserControl
    {
        public UIElement PanelContent
        {
            get => (UIElement)GetValue(PanelContentProperty);
            set => SetValue(PanelContentProperty, value);
        }

        public static readonly DependencyProperty PanelContentProperty =
            DependencyProperty.Register("Content", typeof(UIElement),
                typeof(PanelContainerControl), new PropertyMetadata(null, ContentProperty_ChangedCallback));

        public PanelContainerControl()
        {
            this.InitializeComponent();
        }

        private static void ContentProperty_ChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PanelContainerControl)d;

            control.ContentContainer.Children.Clear();
            control.ContentContainer.Children.Add((UIElement) e.NewValue);
        }
    }
}