using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    internal readonly struct MapRendererComponents
    {
        public readonly IUniTaskAsyncEnumerable<(MapLayer, IMapLayerController)> Layers;
        public readonly IMapCullingController CullingController;
        public readonly IObjectPool<IMapCameraControllerInternal> MapCameraControllers;

        public MapRendererComponents(IUniTaskAsyncEnumerable<(MapLayer, IMapLayerController)> layers, IMapCullingController cullingController, IObjectPool<IMapCameraControllerInternal> mapCameraControllers)
        {
            Layers = layers;
            CullingController = cullingController;
            MapCameraControllers = mapCameraControllers;
        }
    }
}
