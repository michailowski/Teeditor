using Teeditor.Common.Models.IO;
using Teeditor.Common.Models.Tab;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic;

namespace Teeditor.TeeWorlds.MapExtension.Api
{
    public class MapTabFactory : TabFactoryBase
    {
        private MapFactory _mapFactory;

        public MapTabFactory()
            => _mapFactory = new MapFactory();

        protected override TabBase Create(IEditableFile editableFile, EditableEntityBase editableEntity)
            => new Tab((MapFile)editableFile, (Map)editableEntity);

        protected override EditableEntityBase GetEditableEntity(IEditableFile readeableFile)
        {
            var mapFile = readeableFile as MapFile;
            return _mapFactory.Create(mapFile.Payload);
        }

        protected override IEditableFile GetEditableFile()
            => new MapFile();
    }
}
