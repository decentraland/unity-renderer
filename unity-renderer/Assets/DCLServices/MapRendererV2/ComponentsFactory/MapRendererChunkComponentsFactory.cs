using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DCL;
using DCL.Providers;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.Atlas;
using DCLServices.MapRendererV2.MapLayers.UsersMarkers.ColdArea;
using DCLServices.MapRendererV2.MapLayers.UsersMarkers.HotArea;
using MainScripts.DCL.Helpers.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    public class MapRendererChunkComponentsFactory : IMapRendererComponentsFactory
    {
        private const string ATLAS_CHUNK_ADDRESS = "AtlasChunk";
        private const string COLD_USER_MARKER_ADDRESS = "ColdUserMarker";
        private const string HOT_USER_MARKER_ADDRESS = "HotUserMarker";
        private const string MAP_CONFIGURATION_ADDRESS = "MapRendererConfiguration";

        private const int ATLAS_DRAW_ORDER = 1;
        private const int COLD_USER_MARKERS_DRAW_ORDER = 10;
        private const int HOT_USER_MARKERS_DRAW_ORDER = 11;
        private const int ATLAS_CHUNK_SIZE = 250;

        private const int COLD_USER_MARKERS_LIMIT = 100;
        private const int HOT_USER_MARKERS_PREWARM_COUNT = 30;

        private const int PARCEL_SIZE = 20;

        private Service<IAddressableResourceProvider> addressablesProvider;

        private IAddressableResourceProvider AddressableProvider => addressablesProvider.Ref;

        MapRendererComponents IMapRendererComponentsFactory.Create(CancellationToken cancellationToken)
        {
            // TODO implement Culling Controller
            IMapCullingController cullingController = null;

            var enumerator = UniTaskAsyncEnumerable.Create<(MapLayer, IMapLayerController)>(async (writer, token) =>
            {
                var configuration = await AddressableProvider.GetAddressable<MapRendererConfiguration>(MAP_CONFIGURATION_ADDRESS, cancellationToken);
                var coordsUtils = new ChunkCoordsUtils(PARCEL_SIZE);

                async UniTask CreateAtlas()
                {
                    var atlasChunkPrefab = await AddressableProvider.GetAddressable<SpriteRenderer>(ATLAS_CHUNK_ADDRESS, cancellationToken);
                    var chunkAtlas = new ChunkAtlasController(configuration.AtlasRoot, atlasChunkPrefab, ATLAS_DRAW_ORDER, ATLAS_CHUNK_SIZE,
                        coordsUtils, cullingController, ChunkController.CreateChunk);

                    // initialize Atlas but don't block the flow (to accelerate loading time)
                    chunkAtlas.Initialize(cancellationToken).SuppressCancellationThrow().Forget();

                    await writer.YieldAsync((MapLayer.Atlas, chunkAtlas));
                }

                async UniTask CreateColdUserMarkers()
                {
                    var prefab = await AddressableProvider.GetAddressable<ColdUserMarkerObject>(COLD_USER_MARKER_ADDRESS, cancellationToken);

                    var controller = new UsersMarkersColdAreaController(configuration.ColdUserMarkersRoot, prefab, ColdUserMarker.Create,
                        DataStore.i.realm.realmName, CommonScriptableObjects.playerCoords, KernelConfig.i, coordsUtils, cullingController, COLD_USER_MARKERS_DRAW_ORDER, COLD_USER_MARKERS_LIMIT);

                    await controller.Initialize(cancellationToken).SuppressCancellationThrow();
                    await writer.YieldAsync((MapLayer.ColdUsersMarkers, controller));
                }

                async UniTask CreateHotUserMarkers()
                {
                    var prefab = await AddressableProvider.GetAddressable<HotUserMarkerObject>(HOT_USER_MARKER_ADDRESS, cancellationToken);

                    HotUserMarkerObject CreateObject()
                    {
                        var instance = Object.Instantiate(prefab, configuration.HotUserMarkersRoot);
                        instance.sprite.sortingOrder = HOT_USER_MARKERS_DRAW_ORDER;
                        return instance;
                    }

                    var objectsPool = new ObjectPool<HotUserMarkerObject>(CreateObject, actionOnDestroy: Object.Destroy, defaultCapacity: HOT_USER_MARKERS_PREWARM_COUNT);

                    IHotUserMarker CreateWrap() =>
                        new HotUserMarker(objectsPool, cullingController, coordsUtils, CommonScriptableObjects.worldOffset);

                    var wrapsPool = new ObjectPool<IHotUserMarker>(CreateWrap, actionOnRelease: m => m.Dispose(), defaultCapacity: HOT_USER_MARKERS_PREWARM_COUNT);
                    wrapsPool.Prewarm(HOT_USER_MARKERS_PREWARM_COUNT);

                    var controller = new UsersMarkersHotAreaController(DataStore.i.player.otherPlayers, objectsPool, wrapsPool, configuration.HotUserMarkersRoot, coordsUtils, cullingController, HOT_USER_MARKERS_DRAW_ORDER);

                    await UniTask.WhenAll(objectsPool.PrewarmAsync(CreateObject, HOT_USER_MARKERS_PREWARM_COUNT, 20, cancellationToken), controller.Initialize(cancellationToken));
                    await writer.YieldAsync((MapLayer.HotUsersMarkers, controller));
                }

                await UniTask.WhenAll(CreateAtlas(), CreateColdUserMarkers(), CreateHotUserMarkers() /* List of other creators that can be executed in parallel */);
            });

            return new MapRendererComponents(enumerator, cullingController);
        }
    }
}
