using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Factories
{
    internal abstract class MapItemFactory
    {
        public abstract MapItem Create();
        public abstract MapItem Create(IMapItemDTO mapItemDTO, MapFilePayload payload);
    }
}
