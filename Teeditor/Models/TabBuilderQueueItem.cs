using Teeditor.Common.Models.IO;
using Teeditor.Common.Models.Tab;

namespace Teeditor.Models
{
    internal class TabBuilderQueueItem
    {
        public string Extension { get; set; }
        public EditableFileInitStrategyBase InitStrategy { get; set; }
        public TabBase Tab { get; set; }
    }
}
