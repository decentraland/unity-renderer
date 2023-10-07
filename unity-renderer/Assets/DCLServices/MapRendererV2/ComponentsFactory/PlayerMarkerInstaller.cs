using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.PlayerMarker;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    internal struct PlayerMarkerInstaller
    {
        private const string PLAYER_MARKER_ADDRESS = "PlayerMarker";

        private Service<IAddressableResourceProvider> addressablesProvider;

        public async UniTask Install(
            Dictionary<MapLayer, IMapLayerController> writer,
            List<IZoomScalingLayer> zoomScalingWriter,
            MapRendererConfiguration configuration,
            ICoordsUtils coordsUtils,
            IMapCullingController cullingController,
            CancellationToken cancellationToken)
        {
            PlayerMarkerObject prefab = await GetPrefab(cancellationToken);

            var controller = new PlayerMarkerController(
                CreateMarker,
                DataStore.i.player.playerWorldPosition,
                CommonScriptableObjects.cameraForward,
                configuration.PlayerMarkerRoot,
                coordsUtils,
                cullingController
            );

            controller.Initialize();

            writer.Add(MapLayer.PlayerMarker, controller);
            zoomScalingWriter.Add(controller);
            return;

            IPlayerMarker CreateMarker(Transform parent)
            {
                PlayerMarkerObject pmObject = Object.Instantiate(prefab, parent);
                coordsUtils.SetObjectScale(pmObject);
                pmObject.SetSortingOrder(MapRendererDrawOrder.PLAYER_MARKER);

                if (DataStore.i.featureFlags.flags.Get().IsInitialized)
                    pmObject.SetAnimatedCircleVisibility(DataStore.i.featureFlags.flags.Get().IsFeatureEnabled("map_focus_home_or_user"));
                else
                    DataStore.i.featureFlags.flags.OnChange += (current, previous) => { pmObject.SetAnimatedCircleVisibility(current.IsFeatureEnabled("map_focus_home_or_user")); };

                return new PlayerMarker(pmObject);
            }
        }

        internal async UniTask<PlayerMarkerObject> GetPrefab(CancellationToken cancellationToken) =>
            await addressablesProvider.Ref.GetAddressable<PlayerMarkerObject>(PLAYER_MARKER_ADDRESS, cancellationToken);
    }
}
