using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    internal readonly struct MapRendererComponents
    {
        public readonly IUniTaskAsyncEnumerable<(MapLayer, IMapLayerController)> Layers;
        public readonly IMapCullingController CullingController;

        public MapRendererComponents(IUniTaskAsyncEnumerable<(MapLayer, IMapLayerController)> layers, IMapCullingController cullingController)
        {
            Layers = layers;
            CullingController = cullingController;
        }
    }
}
