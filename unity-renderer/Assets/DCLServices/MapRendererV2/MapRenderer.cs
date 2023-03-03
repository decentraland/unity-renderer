using Cysharp.Threading.Tasks;
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

        private IMapCullingController cullingController;
        private IObjectPool<IMapCameraControllerInternal> mapCameraPool;

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

                await foreach (var (layerType, layer) in components.Layers)
                    layers[layerType] = new MapLayerStatus(layer);
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
            const int MAX_TEXTURE_SIZE = 2048;
            const int MIN_ZOOM = 100;
            const int MAX_ZOOM = 500;

            // Clamp texture to the maximum size allowed, preserving aspect ratio
            Vector2Int zoomValues = cameraInput.ZoomValues;
            zoomValues.x = Mathf.Max(zoomValues.x, MIN_ZOOM);
            zoomValues.y = Mathf.Min(zoomValues.y, MAX_ZOOM);
            Vector2 textureRes = cameraInput.TextureResolution;
            float factor = Mathf.Min(1, MAX_TEXTURE_SIZE / Mathf.Max(textureRes.x, textureRes.y));

            EnableLayers(cameraInput.EnabledLayers);
            var mapCameraController = mapCameraPool.Get();
            mapCameraController.OnReleasing += ReleaseCamera;
            mapCameraController.Initialize(Vector2Int.FloorToInt(textureRes * factor), zoomValues, cameraInput.EnabledLayers);
            mapCameraController.SetZoom(cameraInput.Zoom);
            mapCameraController.SetPosition(cameraInput.Position);
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
        }
    }
}
