using Microsoft.UI.Xaml.Controls;
using Teeditor.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Views.Dialogs
{
    public sealed partial class SettingsDialog : ContentDialog
    {
        public SettingsDialog()
        {
            this.InitializeComponent();

            var theme = AppThemeSettings.GetTheme();

            switch (theme)
            {
                case "Dark":
                    ThemeRadioButtons.SelectedIndex = 0;
                    break;
                case "Light":
                    ThemeRadioButtons.SelectedIndex = 1;
                    break;
                default:
                    ThemeRadioButtons.SelectedIndex = 2;
                    break;
            }

            AppThemeSettings.InitializeElementTheme();

            ThemeRadioButtons.SelectionChanged += ThemeRadioButtons_SelectionChanged;
        }

        private void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is RadioButtons rb)
            {
                string themeName = rb.SelectedItem as string;
                AppThemeSettings.SetApplicationTheme(themeName);
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
            => this.Hide();
    }
}
