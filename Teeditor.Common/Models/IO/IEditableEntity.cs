using System;
using Teeditor.Common.Models.ModificationObserving;

namespace Teeditor.Common.Models.IO
{
    public interface IEditableEntity : IModificationObservable
    {
        bool IsLoading { get; }

        event EventHandler Loaded;
    }
}
