using System.ComponentModel;
using Windows.UI.Xaml.Controls;

namespace Teeditor.Common.Models.Properties
{
    public interface IPropertiesItem : INotifyPropertyChanged
    {
        string MenuText { get; set; }
        PathIcon MenuIcon { get; }
    }
}
