using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Views.Dialogs
{
    public sealed partial class ShortcutReferenceDialog : ContentDialog
    {
        public ShortcutReferenceDialog()
        {
            this.InitializeComponent();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
            => this.Hide();
    }
}
