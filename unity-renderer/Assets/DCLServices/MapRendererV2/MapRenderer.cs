using Cysharp.Threading.Tasks;
using DCL;
using DCL.Helpers;
using DCLServices.MapRendererV2.CommonBehavior;
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
            public readonly Dictionary<IMapActivityOwner, IReadOnlyDictionary<MapLayer, IMapLayerParameter>> ActivityOwners = new ();
            public readonly Stack<IMapActivityOwner> ownersStack = new ();
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

            EnableLayers(cameraInput.ActivityOwner, cameraInput.EnabledLayers, cameraInput.LayerParameters);
            IMapCameraControllerInternal mapCameraController = mapCameraPool.Get();
            mapCameraController.Initialize(cameraInput.TextureResolution, zoomValues, cameraInput.EnabledLayers);
            mapCameraController.OnReleasing += ReleaseCamera;

            if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("map_focus_home_or_user"))
                mapCameraController.ZoomChanged += OnCameraZoomChanged;

            mapCameraController.SetPositionAndZoom(cameraInput.Position, cameraInput.Zoom);

            return mapCameraController;
        }

        private void ReleaseCamera(IMapActivityOwner owner, IMapCameraControllerInternal mapCameraController)
        {
            mapCameraController.OnReleasing -= ReleaseCamera;

            if (DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("map_focus_home_or_user"))
            {
                mapCameraController.ZoomChanged -= OnCameraZoomChanged;

                foreach (IZoomScalingLayer layer in zoomScalingLayers)
                    layer.ResetToBaseScale();
            }

            DisableLayers(owner, mapCameraController.EnabledLayers);
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
                if (!EnumUtils.HasFlag(mask, mapLayer) || !layers.TryGetValue(mapLayer, out MapLayerStatus mapLayerStatus) || mapLayerStatus.ActivityOwners.Count == 0 || mapLayerStatus.sharedActive == active)
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

        private void EnableLayers(IMapActivityOwner owner, MapLayer mask, IReadOnlyDictionary<MapLayer, IMapLayerParameter> layerParameters)
        {
            foreach (MapLayer mapLayer in ALL_LAYERS)
            {
                if (!EnumUtils.HasFlag(mask, mapLayer) || !layers.TryGetValue(mapLayer, out MapLayerStatus mapLayerStatus)) continue;

                if (layerParameters.TryGetValue(mapLayer, out IMapLayerParameter parameter))
                    mapLayerStatus.MapLayerController.SetParameter(parameter);

                if (mapLayerStatus.ownersStack.Count == 0 && mapLayerStatus.sharedActive != false)
                {
                    // Cancel deactivation flow
                    ResetCancellationSource(mapLayerStatus);
                    mapLayerStatus.MapLayerController.Enable(mapLayerStatus.CTS.Token).SuppressCancellationThrow().Forget();
                }

                mapLayerStatus.ActivityOwners.Add(owner, layerParameters);
                mapLayerStatus.ownersStack.Push(owner);
            }
        }

        private void DisableLayers(IMapActivityOwner owner, MapLayer mask)
        {
            foreach (MapLayer mapLayer in ALL_LAYERS)
            {
                if (!EnumUtils.HasFlag(mask, mapLayer) || !layers.TryGetValue(mapLayer, out MapLayerStatus mapLayerStatus)) continue;

                if (mapLayerStatus.ActivityOwners.ContainsKey(owner))
                    mapLayerStatus.ActivityOwners.Remove(owner);

                mapLayerStatus.ownersStack.Pop();

                if (mapLayerStatus.ownersStack.Count == 0)
                {
                    // Cancel activation flow
                    ResetCancellationSource(mapLayerStatus);
                    mapLayerStatus.MapLayerController.Disable(mapLayerStatus.CTS.Token).SuppressCancellationThrow().Forget();
                }
                else
                {
                    IReadOnlyDictionary<MapLayer, IMapLayerParameter> parametersByLayer = mapLayerStatus.ActivityOwners[mapLayerStatus.ownersStack.Peek()];

                    if (parametersByLayer.ContainsKey(mapLayer))
                        mapLayerStatus.MapLayerController.SetParameter(parametersByLayer[mapLayer]);
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
