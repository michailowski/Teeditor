using System.Runtime.CompilerServices;
using Teeditor.Common.Models.Properties;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Utilities;
using Teeditor.Common.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.TeeWorlds.MapExtension.Internal.ViewModels.Toolbar
{
    internal class ViewToolViewModel : ToolViewModelBase
    {
        private PropertiesManagerBase _propertiesManager;

        public bool IsHighDetailEnabled
        {
            get => TryGetProperty<bool>();
            set => TrySetProperty(value);
        }

        public bool IsGridEnabled
        {
            get => TryGetProperty<bool>();
            set => TrySetProperty(value);
        }

        public bool IsProofBordersEnabled
        {
            get => TryGetProperty<bool>();
            set => TrySetProperty(value);
        }

        public ViewToolViewModel()
        {
            Label = "View Tool";
            MenuText = "View Tool";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["ViewToolIconPath"]) };
        }

        private T TryGetProperty<T>([CallerMemberName] string name = null) where T : struct
            => _propertiesManager?.GetProperty<T>(name).Value ?? default;

        private void TrySetProperty<T>(T value, [CallerMemberName] string name = null) where T : struct
        {
            var settingResult = _propertiesManager?.TrySetPropertyValue(value, name) ?? false;

            if (settingResult == false)
                return;

            OnPropertyChanged(name);
        }

        public override void SetTab(ITab tab)
        {
            if (_propertiesManager != null)
            {
                _propertiesManager.PropertyValueUpdated -= PropertiesManager_PropertyValueUpdated;
            }

            _propertiesManager = tab?.PropertiesManager;

            _propertiesManager.PropertyValueUpdated += PropertiesManager_PropertyValueUpdated;

            RaiseChanges();
        }

        private void PropertiesManager_PropertyValueUpdated(string name)
            => OnPropertyChanged(name);

        private void RaiseChanges()
        {
            OnPropertyChanged("IsHighDetailEnabled");
            OnPropertyChanged("IsGridEnabled");
            OnPropertyChanged("IsProofBordersEnabled");
        }
    }
}
