using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DCL;
using DCL.Providers;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.UsersMarkers.HotArea;
using MainScripts.DCL.Helpers.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    internal struct HotUsersMarkersInstaller
    {
        private const string HOT_USER_MARKER_ADDRESS = "HotUserMarker";
        private const int HOT_USER_MARKERS_PREWARM_COUNT = 30;

        private Service<IAddressableResourceProvider> addressablesProvider;

        public async UniTask Install(
            Dictionary<MapLayer, IMapLayerController> writer,
            MapRendererConfiguration configuration,
            ICoordsUtils coordsUtils,
            IMapCullingController cullingController,
            CancellationToken cancellationToken)
        {
            var prefab = await GetPrefab(cancellationToken);

            void SetSortingOrder(HotUserMarkerObject obj)
            {
                obj.sprite.sortingOrder = MapRendererDrawOrder.HOT_USER_MARKERS;
            }

            var objectsPool = new UnityObjectPool<HotUserMarkerObject>(prefab, configuration.HotUserMarkersRoot, actionOnCreate: SetSortingOrder, defaultCapacity: HOT_USER_MARKERS_PREWARM_COUNT);

            IHotUserMarker CreateWrap() =>
                new HotUserMarker(objectsPool, cullingController, coordsUtils, CommonScriptableObjects.worldOffset);

            var wrapsPool = new ObjectPool<IHotUserMarker>(CreateWrap, actionOnRelease: m => m.Dispose(), defaultCapacity: HOT_USER_MARKERS_PREWARM_COUNT);

            var controller = new UsersMarkersHotAreaController(DataStore.i.player.otherPlayers, objectsPool, wrapsPool, HOT_USER_MARKERS_PREWARM_COUNT, configuration.HotUserMarkersRoot, coordsUtils, cullingController, MapRendererDrawOrder.HOT_USER_MARKERS);

            await controller.Initialize(cancellationToken);
            writer.Add(MapLayer.HotUsersMarkers, controller);
        }

        internal async Task<HotUserMarkerObject> GetPrefab(CancellationToken cancellationToken) =>
            await addressablesProvider.Ref.GetAddressable<HotUserMarkerObject>(HOT_USER_MARKER_ADDRESS, cancellationToken);
    }
}
