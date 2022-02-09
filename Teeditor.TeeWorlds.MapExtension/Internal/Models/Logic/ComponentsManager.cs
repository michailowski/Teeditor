using Microsoft.Graphics.Canvas;
using Teeditor.Common.Models.Components;
using Teeditor_TeeWorlds_Direct3DInterop;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic
{
    internal class ComponentsManager : IComponentsManager
    {
        public GraphicsComponent GraphicsComponent { get; }

        public ComponentsManager()
        {
            var device = CanvasDevice.GetSharedDevice();

            GraphicsComponent = new GraphicsComponent(device, 0, 0);
        }
    }
}
