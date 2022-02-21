using Teeditor.Common.Models.Tab;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.ViewModels
{
    public interface IToolViewModel
    {
        int Index { get; set; }
        string Label { get; }
        string MenuText { get; }
        PathIcon MenuIcon { get; }
        bool IsActive { get; }

        void SetTab(ITab tab);
    }
}