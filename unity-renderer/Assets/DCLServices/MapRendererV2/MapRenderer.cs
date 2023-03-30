using Cysharp.Threading.Tasks;
using DCL.Helpers;
using DCLServices.MapRendererV2.ComponentsFactory;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;

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
        private MapRendererConfiguration configurationInstance;

        private IObjectPool<IMapCameraControllerInternal> mapCameraPool;

        internal IMapCullingController cullingController { get; private set; }

        public MapRenderer(IMapRendererComponentsFactory componentsFactory)
        {
            this.componentsFactory = componentsFactory;
        }

        public void Initialize() { }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            layers = new Dictionary<MapLayer, MapLayerStatus>();

            try
            {
                var components = await componentsFactory.Create(cancellationToken);
                cullingController = components.CullingController;
                mapCameraPool = components.MapCameraControllers;
                configurationInstance = components.ConfigurationInstance;

                foreach (var pair in components.Layers)
                    layers[pair.Key] = new MapLayerStatus(pair.Value);
            }
            catch (OperationCanceledException)
            {
                // just ignore
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public IMapCameraController RentCamera(in MapCameraInput cameraInput)
        {
            const int MIN_ZOOM = 5;
            const int MAX_ZOOM = 300;

            // Clamp texture to the maximum size allowed, preserving aspect ratio
            Vector2Int zoomValues = cameraInput.ZoomValues;
            zoomValues.x = Mathf.Max(zoomValues.x, MIN_ZOOM);
            zoomValues.y = Mathf.Min(zoomValues.y, MAX_ZOOM);

            EnableLayers(cameraInput.EnabledLayers);
            var mapCameraController = mapCameraPool.Get();
            mapCameraController.OnReleasing += ReleaseCamera;
            mapCameraController.Initialize(cameraInput.TextureResolution, zoomValues, cameraInput.EnabledLayers);
            mapCameraController.SetPositionAndZoom(cameraInput.Position, cameraInput.Zoom);

            return mapCameraController;
        }

        private void ReleaseCamera(IMapCameraControllerInternal mapCameraController)
        {
            mapCameraController.OnReleasing -= ReleaseCamera;
            DisableLayers(mapCameraController.EnabledLayers);
            mapCameraPool.Release(mapCameraController);
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

            cullingController?.Dispose();

            if (configurationInstance)
                Utils.SafeDestroy(configurationInstance.gameObject);
        }
    }
}
