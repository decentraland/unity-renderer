using Cysharp.Threading.Tasks;
using DCL;
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
            public bool? sharedActive;

            public MapLayerStatus(IMapLayerController mapLayerController)
            {
                MapLayerController = mapLayerController;
            }
        }

        private static readonly MapLayer[] ALL_LAYERS = EnumUtils.Values<MapLayer>();

        private CancellationToken cancellationToken;

        private readonly IMapRendererComponentsFactory componentsFactory;

        private Dictionary<MapLayer, MapLayerStatus> layers;
        private List<IZoomScalingLayer> zoomScalingLayers;

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
            zoomScalingLayers = new List<IZoomScalingLayer>();

            try
            {
                MapRendererComponents components = await componentsFactory.Create(cancellationToken);
                cullingController = components.CullingController;
                mapCameraPool = components.MapCameraControllers;
                configurationInstance = components.ConfigurationInstance;

                foreach (IZoomScalingLayer zoomScalingLayer in components.ZoomScalingLayers)
                    zoomScalingLayers.Add(zoomScalingLayer);

                foreach (KeyValuePair<MapLayer, IMapLayerController> pair in components.Layers)
                {
                    pair.Value.Disable(cancellationToken);
                    layers[pair.Key] = new MapLayerStatus(pair.Value);
                }

                if (DataStore.i.featureFlags.flags.Get().IsInitialized)
                    OnFeatureFlagsChanged(null, null);
                else
                    DataStore.i.featureFlags.flags.OnChange += OnFeatureFlagsChanged;
            }
            catch
                (OperationCanceledException)
            {
                // just ignore
            }
            catch (Exception e) { Debug.LogException(e); }
        }

        private void OnFeatureFlagsChanged(FeatureFlag _, FeatureFlag __)
        {
            bool satelliteEnabled = DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("navmap-satellite-view"); // false if not initialized
            layers[MapLayer.SatelliteAtlas].sharedActive = satelliteEnabled;
            layers[MapLayer.ParcelsAtlas].sharedActive = !satelliteEnabled;
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
            IMapCameraControllerInternal mapCameraController = mapCameraPool.Get();
            mapCameraController.Initialize(cameraInput.TextureResolution, zoomValues, cameraInput.EnabledLayers);
            mapCameraController.OnReleasing += ReleaseCamera;

            if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("map_focus_home_or_user"))
                mapCameraController.ZoomChanged += OnCameraZoomChanged;

            mapCameraController.SetPositionAndZoom(cameraInput.Position, cameraInput.Zoom);

            return mapCameraController;
        }

        private void ReleaseCamera(IMapCameraControllerInternal mapCameraController)
        {
            mapCameraController.OnReleasing -= ReleaseCamera;

            if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("map_focus_home_or_user"))
            {
                mapCameraController.ZoomChanged -= OnCameraZoomChanged;
                foreach (IZoomScalingLayer layer in zoomScalingLayers)
                    layer.ResetToBaseScale();
            }

            DisableLayers(mapCameraController.EnabledLayers);
            mapCameraPool.Release(mapCameraController);
        }

        private void OnCameraZoomChanged(float baseZoom, float newZoom)
        {
            foreach (IZoomScalingLayer layer in zoomScalingLayers)
                layer.ApplyCameraZoom(baseZoom, newZoom);
        }

        public void SetSharedLayer(MapLayer mask, bool active)
        {
            foreach (MapLayer mapLayer in ALL_LAYERS)
            {
                if (!EnumUtils.HasFlag(mask, mapLayer) || !layers.TryGetValue(mapLayer, out MapLayerStatus mapLayerStatus) || mapLayerStatus.ActivityOwners == 0 || mapLayerStatus.sharedActive == active)
                    continue;

                mapLayerStatus.sharedActive = active;

                // Cancel activation/deactivation flow
                ResetCancellationSource(mapLayerStatus);

                if (active)
                    mapLayerStatus.MapLayerController.Enable(mapLayerStatus.CTS.Token).SuppressCancellationThrow().Forget();
                else
                    mapLayerStatus.MapLayerController.Disable(mapLayerStatus.CTS.Token).SuppressCancellationThrow().Forget();
            }
        }

        private void EnableLayers(MapLayer mask)
        {
            foreach (MapLayer mapLayer in ALL_LAYERS)
            {
                if (!EnumUtils.HasFlag(mask, mapLayer) || !layers.TryGetValue(mapLayer, out MapLayerStatus mapLayerStatus)) continue;

                if (mapLayerStatus.ActivityOwners == 0 && mapLayerStatus.sharedActive != false)
                {
                    // Cancel deactivation flow
                    ResetCancellationSource(mapLayerStatus);
                    mapLayerStatus.MapLayerController.Enable(mapLayerStatus.CTS.Token).SuppressCancellationThrow().Forget();
                }

                mapLayerStatus.ActivityOwners++;
            }
        }

        private void DisableLayers(MapLayer mask)
        {
            foreach (MapLayer mapLayer in ALL_LAYERS)
            {
                if (!EnumUtils.HasFlag(mask, mapLayer) || !layers.TryGetValue(mapLayer, out MapLayerStatus mapLayerStatus)) continue;

                mapLayerStatus.ActivityOwners = Math.Max(mapLayerStatus.ActivityOwners - 1, 0);

                if (mapLayerStatus.ActivityOwners == 0)
                {
                    // Cancel activation flow
                    ResetCancellationSource(mapLayerStatus);
                    mapLayerStatus.MapLayerController.Disable(mapLayerStatus.CTS.Token).SuppressCancellationThrow().Forget();
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
            foreach (MapLayerStatus status in layers.Values)
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
