using System.Threading.Tasks;
using Windows.Storage;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload;
using Teeditor.Common.Models.IO;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO
{
    internal class MapFile : EditableFileBase
    {
        public MapFilePayload Payload { get; private set; }

        protected override void ProcessCreating()
        {
            Payload = new MapFilePayload();
        }

        protected override async Task ProcessLoading()
        {
            Payload = await MapFileReader.ReadAsync(_storageFile);
        }

        protected override async Task ProcessSaving(IEditableEntity editableEntity)
        {
            await MapFileWriter.WriteAsync((Map)editableEntity, _storageFile);
        }

        protected override async Task ProcessSavingAs(IEditableEntity editableEntity, StorageFile storageFile)
        {
            await MapFileWriter.WriteAsync((Map)editableEntity, storageFile);
        }
    }
}