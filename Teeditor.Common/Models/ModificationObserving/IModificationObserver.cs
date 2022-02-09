
using System.ComponentModel;

namespace Teeditor.Common.Models.ModificationObserving
{
    public interface IModificationObserver : INotifyPropertyChanged
    {
        bool IsModified { get; }
    }
}
