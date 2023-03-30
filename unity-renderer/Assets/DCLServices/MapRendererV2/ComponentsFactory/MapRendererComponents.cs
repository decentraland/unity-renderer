using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using System.Collections.Generic;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    internal readonly struct MapRendererComponents
    {
        public readonly MapRendererConfiguration ConfigurationInstance;
        public readonly IReadOnlyDictionary<MapLayer, IMapLayerController> Layers;
        public readonly IMapCullingController CullingController;
        public readonly IObjectPool<IMapCameraControllerInternal> MapCameraControllers;

        public MapRendererComponents(MapRendererConfiguration configurationInstance, IReadOnlyDictionary<MapLayer, IMapLayerController> layers,
            IMapCullingController cullingController, IObjectPool<IMapCameraControllerInternal> mapCameraControllers)
        {
            ConfigurationInstance = configurationInstance;
            Layers = layers;
            CullingController = cullingController;
            MapCameraControllers = mapCameraControllers;
        }
    }
}
