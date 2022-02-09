using Teeditor.Common.Models.Tab;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.ViewModels
{
    public interface IToolViewModel
    {
        string Label { get; }
        string MenuText { get; }
        PathIcon MenuIcon { get; }

        void SetTab(ITab tab);
    }
}