using System;
using System.ComponentModel;
using Teeditor.Common.Models.Bindable;

namespace Teeditor.Common.Models.ModificationObserving
{
    public interface IModificationObservable : INotifyPropertyChanged, INotifyPropertyModificated
    {
        event EventHandler Modified;
    }
}
