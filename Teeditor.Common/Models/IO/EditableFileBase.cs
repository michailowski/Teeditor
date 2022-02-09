using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Teeditor.Common.Models.IO
{
    public abstract class EditableFileBase : IEditableFile
    {
        protected StorageFile _storageFile;

        public string Name { get; protected set; }
        public string Extension { get; protected set; }
        public string MruToken { get; protected set; }
        public bool IsStored => _storageFile != null;

        public void Create(ProjectType projectType)
        {
            Name = projectType.GeneratedName;
            Extension = Path.GetExtension(projectType.Extension);

            ProcessCreating();
        }

        public async Task LoadAsync(StorageFile storageFile)
        {
            _storageFile = storageFile;

            Name = Path.GetFileNameWithoutExtension(_storageFile.Name);
            Extension = Path.GetExtension(_storageFile.Name);
            MruToken = MostRecentlyUsedList.Add(_storageFile);

            await ProcessLoading();
        }

        public async Task SaveAsAsync(IEditableEntity editableEntity, StorageFile storageFile)
        {
            await ProcessSavingAs(editableEntity, storageFile);

            _storageFile = storageFile;
        }

        public async Task SaveAsync(IEditableEntity editableEntity)
            => await ProcessSaving(editableEntity);

        protected abstract void ProcessCreating();
        protected abstract Task ProcessLoading();
        protected abstract Task ProcessSavingAs(IEditableEntity editableEntity, StorageFile storageFile);
        protected abstract Task ProcessSaving(IEditableEntity editableEntity);
    }
}
