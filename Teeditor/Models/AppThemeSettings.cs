using Windows.Storage;
using Windows.UI.Xaml;

namespace Teeditor.Models
{
    internal static class AppThemeSettings
    {
        public static void InitializeAppTheme()
        {
            var localSettings = ApplicationData.Current.LocalSettings;

            var value = localSettings.Values["currentTheme"];
            var appTheme = ApplicationTheme.Dark;

            if (value == null)
            {
                localSettings.Values["currentTheme"] = "Dark";
                appTheme = ApplicationTheme.Dark;
            }
            else
            {
                switch (value)
                {
                    case "Dark":
                        appTheme = ApplicationTheme.Dark;
                        break;
                    case "Light":
                        appTheme = ApplicationTheme.Light;
                        break;
                    default:
                        appTheme = Application.Current.RequestedTheme;
                        break;
                }
            }

            Application.Current.RequestedTheme = appTheme;
        }

        public static void InitializeElementTheme()
        {
            var elementTheme = Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;

            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                if (frameworkElement.RequestedTheme == ElementTheme.Default)
                {
                    frameworkElement.RequestedTheme = elementTheme;
                }
            }
        }

        public static string GetTheme()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            var theme = (string)localSettings.Values["currentTheme"];

            return theme == null ? "System" : theme;
        }

        public static void SetApplicationTheme(string themeName)
        {
            ElementTheme theme;

            switch (themeName)
            {
                case "Dark":
                    theme = ElementTheme.Dark;
                    break;
                case "Light":
                    theme = ElementTheme.Light;
                    break;
                default:
                    theme = Application.Current.RequestedTheme == ApplicationTheme.Dark ? ElementTheme.Dark : ElementTheme.Light;
                    break;
            }

            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["currentTheme"] = themeName;

            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                if (frameworkElement.RequestedTheme != theme)
                {
                    frameworkElement.RequestedTheme = theme;
                }
            }
        }
    }
}
