using Teeditor.Common.Models.Properties;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic
{
    internal class PropertiesManager : PropertiesManagerBase
    {
        public PropertiesManager()
        {
            TryAddProperty("IsHighDetailEnabled", new PropertiesItem<bool>(true, "Enable High Detail", "IsHighDetailEnabledIconPath"));
            TryAddProperty("IsGridEnabled", new PropertiesItem<bool>(false, "Show Grid", "IsGridEnabledIconPath"));
            TryAddProperty("IsProofBordersEnabled", new PropertiesItem<bool>(false, "Show Proof Borders", "IsProofBordersEnabledIconPath"));
        }
    }
}
