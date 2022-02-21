using System;
using Teeditor.Common.Models.Sidebar;
using Teeditor.Common.Models.Tab;
using Teeditor.Common.Utilities;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.ViewModels
{
    public abstract class BoxViewModelBase : DynamicViewModel, IBoxViewModel
    {
        public string Label { get; protected set; }

        public string MenuText { get; protected set; }

        public PathIcon MenuIcon { get; protected set; }

        public SidebarDock DefaultDock { get; protected set; }

        public SidebarDock Dock
        {
            get => GetDock();
            set => SetDock(value);
        }

        public int Index 
        { 
            get => GetIndex();
            set => SetIndex(value);
        }

        public bool DefaultActive { get; protected set; }

        public bool IsActive
        {
            get => GetActive();
            set => SetActive(value);
        }

        public BoxViewModelBase()
        {
            Label = "Default label";
            MenuText = "Default menu text";
            MenuIcon = new PathIcon() { Data = UserInterface.PathMarkupToGeometry((string)Application.Current.Resources["ExplorerBoxIconPath"]) };
            DefaultDock = SidebarDock.Left;
            DefaultActive = false;
        }

        public abstract void SetTab(ITab tab);

        private SidebarDock GetDock()
        {
            var obtained = ApplicationData.Current.LocalSettings.Values.TryGetValue($"SidebarBox{Label.Trim()}Dock", out var value);

            return obtained ? (SidebarDock)value : DefaultDock;
        }

        private void SetDock(SidebarDock value)
        {
            ApplicationData.Current.LocalSettings.Values[$"SidebarBox{Label.Trim()}Dock"] = (int)value;
        }

        private int GetIndex()
        {
            var obtained = ApplicationData.Current.LocalSettings.Values.TryGetValue($"SidebarBox{Label.Trim()}Index", out var value);

            return obtained ? (int)value : 0;
        }

        private void SetIndex(int value)
        {
            ApplicationData.Current.LocalSettings.Values[$"SidebarBox{Label.Trim()}Index"] = value;
        }

        private bool GetActive()
        {
            var obtained = ApplicationData.Current.LocalSettings.Values.TryGetValue($"SidebarBox{Label.Trim()}Active", out var value);

            return obtained ? Convert.ToBoolean(value) : DefaultActive;
        }

        private void SetActive(bool value)
        {
            ApplicationData.Current.LocalSettings.Values[$"SidebarBox{Label.Trim()}Active"] = Convert.ToInt32(value);
        }
    }
}
