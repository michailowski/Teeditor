using System;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Utilities;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.ViewModels
{
    public abstract class ToolViewModelBase : DynamicViewModel, IToolViewModel
    {
        public string Label { get; protected set; }

        public string MenuText { get; protected set; }

        public PathIcon MenuIcon { get; protected set; }

        public int Index
        {
            get => GetIndex();
            set => SetIndex(value);
        }

        public bool IsActive
        {
            get => GetActive();
            set => SetActive(value);
        }

        public ToolViewModelBase()
        {
            Label = "Default label";
            MenuText = "Default menu text";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["DefaultIconPath"]) };
        }

        public abstract void SetTab(ITab tab);

        private int GetIndex()
        {
            var obtained = ApplicationData.Current.LocalSettings.Values.TryGetValue($"ToolbarTool{Label.Trim()}Index", out var value);

            return obtained ? (int)value : 0;
        }

        private void SetIndex(int value)
        {
            ApplicationData.Current.LocalSettings.Values[$"ToolbarTool{Label.Trim()}Index"] = value;
        }

        private bool GetActive()
        {
            var obtained = ApplicationData.Current.LocalSettings.Values.TryGetValue($"ToolbarTool{Label.Trim()}Active", out var value);

            return obtained ? Convert.ToBoolean(value) : true;
        }

        private void SetActive(bool value)
        {
            ApplicationData.Current.LocalSettings.Values[$"ToolbarTool{Label.Trim()}Active"] = Convert.ToInt32(value);
        }
    }
}
