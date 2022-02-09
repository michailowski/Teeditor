using System.Collections.Generic;
using Teeditor.Common.Models.Bindable;
using Teeditor.Common.Models.IO;
using Teeditor.Models;

namespace Teeditor.ViewModels.Dialogs
{
    public class ProjectCreateDialogViewModel : BindableBase
    {
        private ProjectType _selectedItem;

        public IReadOnlyCollection<ProjectType> ProjectTypes => ProjectTypesContainer.Items;

        public ProjectType SelectedItem
        {
            get => _selectedItem;
            set => Set(ref _selectedItem, value);
        }
    }
}