using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
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
            MapRendererConfiguration configuration,
            ICoordsUtils coordsUtils,
            IMapCullingController cullingController,
            CancellationToken cancellationToken)
        {
            var prefab = await GetPrefab(cancellationToken);

            IPlayerMarker CreateMarker(Transform parent)
            {
                var obj = Object.Instantiate(prefab, parent);
                coordsUtils.SetObjectScale(obj);
                obj.spriteRenderer.sortingOrder = MapRendererDrawOrder.PLAYER_MARKER;
                return new PlayerMarker(obj);
            }

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
        }

        internal async UniTask<PlayerMarkerObject> GetPrefab(CancellationToken cancellationToken) =>
            await addressablesProvider.Ref.GetAddressable<PlayerMarkerObject>(PLAYER_MARKER_ADDRESS, cancellationToken);
    }
}
