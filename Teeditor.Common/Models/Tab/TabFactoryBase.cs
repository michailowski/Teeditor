using System.Threading.Tasks;
using Teeditor.Common.Models.IO;

namespace Teeditor.Common.Models.Tab
{
    public abstract class TabFactoryBase
    {
        public async Task<TabBase> CreateAsync(EditableFileInitStrategyBase initStrategy)
        {
            var editableFile = GetEditableFile();
            var initializedEditableFile = await initStrategy.Process(editableFile);
            var editableEntity = GetEditableEntity(initializedEditableFile);

            return Create(initializedEditableFile, editableEntity);
        }

        protected abstract TabBase Create(IEditableFile editableFile, EditableEntityBase editableEntity);
        protected abstract IEditableFile GetEditableFile();
        protected abstract EditableEntityBase GetEditableEntity(IEditableFile editableFile);
    }
}
