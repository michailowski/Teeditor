using Teeditor.Common.Models.Commands;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data
{
    internal class MapInfo : MapItem
    {
        private string _author;
        private string _mapVersion;
        private string _credits;
        private string _license;

        [ModificationCommandLabel("Map author changed")]
        public string Author
        {
            get => _author;
            set => Set(ref _author, value, nameof(_author));
        }

        [ModificationCommandLabel("Map version changed")]
        public string MapVersion
        {
            get => _mapVersion;
            set => Set(ref _mapVersion, value, nameof(_mapVersion));
        }

        [ModificationCommandLabel("Map credits changed")]
        public string Credits
        {
            get => _credits;
            set => Set(ref _credits, value, nameof(_credits));
        }

        [ModificationCommandLabel("Map license changed")]
        public string License
        {
            get => _license;
            set => Set(ref _license, value, nameof(_license));
        }
    }
}
