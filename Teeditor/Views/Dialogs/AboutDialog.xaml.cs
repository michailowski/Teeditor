using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Views.Dialogs
{
    public sealed partial class AboutDialog : ContentDialog
    {
        public AboutDialog()
        {
            this.InitializeComponent();
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
            => this.Hide();
    }
}
