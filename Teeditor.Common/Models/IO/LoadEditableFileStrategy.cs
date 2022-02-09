using System.Threading.Tasks;
using Windows.Storage;

namespace Teeditor.Common.Models.IO
{
    public class LoadEditableFileStrategy : EditableFileInitStrategyBase
    {
        private StorageFile _storageFile;

        public LoadEditableFileStrategy(StorageFile storageFile)
            => _storageFile = storageFile;

        public async override Task<IEditableFile> Process(IEditableFile editableFile)
        {
            await editableFile.LoadAsync(_storageFile);

            return editableFile;
        }
    }
}
