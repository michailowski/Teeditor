using Microsoft.Graphics.Canvas;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Teeditor.TeeWorlds.MapExtension.Internal.Models.Data;
using Teeditor_TeeWorlds_Direct3DInterop;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Logic.SceneManager.Drawers
{
    internal interface ILayerDrawStrategy
    {
        bool TryGetDrawingTask(out Task drawingTask, DrawingTaskArgs data);
    }
}
