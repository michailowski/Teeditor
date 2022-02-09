using System.Collections.Generic;
using Teeditor.TeeWorlds.MapExtension.Internal.DataTransferObjects;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.IO.Payload
{
    internal class MapFilePayloadItems
    {
        public IMapItemDTO VersionDTO { get; set; }
        public IMapItemDTO InfoDTO { get; set; }
        public List<IMapItemDTO> ImageDTOs { get; set; }
        public List<IMapItemDTO> GroupDTOs { get; set; }
        public List<IMapItemDTO> LayerDTOs { get; set; }
        public List<IMapItemDTO> EnvelopeDTOs { get; set; }
        public List<IMapItemDTO> EnvelopePointDTOs { get; set; }

        public MapFilePayloadItems()
        {
            ImageDTOs = new List<IMapItemDTO>();
            GroupDTOs = new List<IMapItemDTO>();
            LayerDTOs = new List<IMapItemDTO>();
            EnvelopeDTOs = new List<IMapItemDTO>();
            EnvelopePointDTOs = new List<IMapItemDTO>();
        }
    }
}
