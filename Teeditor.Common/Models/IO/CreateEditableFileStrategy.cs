using System.Threading.Tasks;

namespace Teeditor.Common.Models.IO
{
    public class CreateEditableFileStrategy : EditableFileInitStrategyBase
    {
        private ProjectType _projectType;

        public CreateEditableFileStrategy(ProjectType projectType)
        {
            _projectType = projectType;
        }

        public override async Task<IEditableFile> Process(IEditableFile editableFile)
        {
            editableFile.Create(_projectType);

            return editableFile;
        }
    }
}
