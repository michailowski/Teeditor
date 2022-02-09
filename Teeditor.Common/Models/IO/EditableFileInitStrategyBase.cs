using System.Threading.Tasks;

namespace Teeditor.Common.Models.IO
{
    public abstract class EditableFileInitStrategyBase
    {
        public abstract Task<IEditableFile> Process(IEditableFile editableFile);
    }
}
