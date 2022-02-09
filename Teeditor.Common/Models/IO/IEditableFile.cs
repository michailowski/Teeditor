using System.Threading.Tasks;
using Windows.Storage;

namespace Teeditor.Common.Models.IO
{
    public interface IEditableFile
    {
        string Name { get; }
        string Extension { get; }
        string MruToken { get; }
        bool IsStored { get; }

        void Create(ProjectType projectType);
        Task LoadAsync(StorageFile storageFile);
        Task SaveAsync(IEditableEntity editableEntity);
        Task SaveAsAsync(IEditableEntity editableEntity, StorageFile storageFile);
    }
}
