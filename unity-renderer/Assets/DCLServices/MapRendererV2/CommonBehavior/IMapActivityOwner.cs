using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using System.Collections.Generic;

namespace DCLServices.MapRendererV2.CommonBehavior
{
    public interface IMapActivityOwner
    {
        IReadOnlyDictionary<MapLayer, IMapLayerParameter> LayersParameters { get; }
    }
}
