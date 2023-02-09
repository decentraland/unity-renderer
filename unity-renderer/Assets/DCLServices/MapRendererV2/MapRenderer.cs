using Cysharp.Threading.Tasks;
using DCLServices.MapRendererV2.ComponentsFactory;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.MapRendererV2
{
    public partial class MapRenderer : IMapRenderer
    {
        private class MapLayerStatus
        {
            public readonly IMapLayerController MapLayerController;
            public int ActivityOwners;
            public CancellationTokenSource CTS;

            public MapLayerStatus(IMapLayerController mapLayerController)
            {
                MapLayerController = mapLayerController;
            }
        }

        private static readonly MapLayer[] ALL_LAYERS = EnumUtils.Values<MapLayer>();

        private CancellationToken cancellationToken;

        private readonly IMapRendererComponentsFactory componentsFactory;

        private Dictionary<MapLayer, MapLayerStatus> layers;

        public MapRenderer(IMapRendererComponentsFactory componentsFactory)
        {
            this.componentsFactory = componentsFactory;
        }

        public void Initialize() { }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            layers = new Dictionary<MapLayer, MapLayerStatus>();

            await foreach (var (layerType, layer) in componentsFactory.Create(cancellationToken))
                layers[layerType] = new MapLayerStatus(layer);
        }

        public IMapCameraController RentCamera(in MapCameraInput cameraInput)
        {
            // take in instance from the pool
            // IMapCameraController.OnDisposed += ReleaseCamera
            EnableLayers(cameraInput.EnabledLayers);

            throw new NotImplementedException();
        }

        private void ReleaseCamera(IMapCameraController mapCameraController)
        {
            mapCameraController.OnDisposed -= ReleaseCamera;
            DisableLayers(mapCameraController.EnabledLayers);
            throw new NotImplementedException();
            // return to pool
        }

        private void EnableLayers(MapLayer mask)
        {
            foreach (MapLayer mapLayer in ALL_LAYERS)
            {
                if (EnumUtils.HasFlag(mask, mapLayer)
                    && layers.TryGetValue(mapLayer, out var mapLayerStatus))
                {
                    if (mapLayerStatus.ActivityOwners == 0)
                    {
                        // Cancel deactivation flow
                        ResetCancellationSource(mapLayerStatus);
                        mapLayerStatus.MapLayerController.Enable(mapLayerStatus.CTS.Token).SuppressCancellationThrow().Forget();
                    }

                    mapLayerStatus.ActivityOwners++;
                }
            }
        }

        private void DisableLayers(MapLayer mask)
        {
            foreach (MapLayer mapLayer in ALL_LAYERS)
            {
                if (EnumUtils.HasFlag(mask, mapLayer)
                    && layers.TryGetValue(mapLayer, out var mapLayerStatus))
                {
                    if (--mapLayerStatus.ActivityOwners == 0)
                    {
                        // Cancel activation flow
                        ResetCancellationSource(mapLayerStatus);
                        mapLayerStatus.MapLayerController.Disable(mapLayerStatus.CTS.Token).SuppressCancellationThrow().Forget();
                    }
                }
            }
        }

        private void ResetCancellationSource(MapLayerStatus mapLayerStatus)
        {
            if (mapLayerStatus.CTS != null)
            {
                mapLayerStatus.CTS.Cancel();
                mapLayerStatus.CTS.Dispose();
            }

            mapLayerStatus.CTS = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        public void Dispose()
        {
            foreach (var status in layers.Values)
            {
                if (status.CTS != null)
                {
                    status.CTS.Cancel();
                    status.CTS.Dispose();
                    status.CTS = null;
                }

                status.MapLayerController.Dispose();
            }
        }
    }
}
