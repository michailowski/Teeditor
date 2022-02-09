
using System.ComponentModel;

namespace Teeditor.TeeWorlds.MapExtension.Internal.Models.Data.Logic
{
    internal interface ILayerSelection : INotifyPropertyChanged
    {
        bool IsEmpty { get; }
        bool IsTransformationAllowed { get; }

        void Clear();
        void FlipHorizontal();
        void FlipVertical();
        void Rotate(float amount);
    }
}
