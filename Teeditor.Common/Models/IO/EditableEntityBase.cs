using System;
using Teeditor.Common.Models.Bindable;

namespace Teeditor.Common.Models.IO
{
    public abstract class EditableEntityBase : BindableBase, IEditableEntity
    {
        private bool _isLoading = true;

        public bool IsLoading
        {
            get => _isLoading;
            protected set => Set(ref _isLoading, value);
        }

        public abstract event EventHandler Loaded;
        public virtual event EventHandler Modified;
    }
}
