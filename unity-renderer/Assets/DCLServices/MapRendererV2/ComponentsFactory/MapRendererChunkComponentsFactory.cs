using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DCL;
using DCL.Providers;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.Atlas;
using MainScripts.DCL.Controllers.HotScenes;
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
        private const string MAP_CONFIGURATION_ADDRESS = "MapRendererConfiguration";
        private const string MAP_CAMERA_OBJECT_ADDRESS = "MapCameraObject";

        private const int ATLAS_CHUNK_SIZE = 250;
        private const int PARCEL_SIZE = 20;

        private Service<IAddressableResourceProvider> addressablesProvider;
        private Service<IHotScenesFetcher> hotScenesFetcher;

        internal ColdUsersMarkersInstaller coldUsersMarkersInstaller { get; }
        internal SceneOfInterestsMarkersInstaller sceneOfInterestsMarkersInstaller { get; }
        internal HomePointMarkerInstaller homePointMarkerInstaller { get; }
        internal HotUsersMarkersInstaller hotUsersMarkersInstaller { get; }

        private IAddressableResourceProvider AddressableProvider => addressablesProvider.Ref;

        async UniTask<MapRendererComponents> IMapRendererComponentsFactory.Create(CancellationToken cancellationToken)
        {
            var configuration = Object.Instantiate(await AddressableProvider.GetAddressable<MapRendererConfiguration>(MAP_CONFIGURATION_ADDRESS, cancellationToken));
            var coordsUtils = new ChunkCoordsUtils(PARCEL_SIZE);

            // TODO implement Culling Controller
            IMapCullingController cullingController = null;

            var mapCameraObjectPrefab = await AddressableProvider.GetAddressable<MapCameraObject>(MAP_CAMERA_OBJECT_ADDRESS, cancellationToken);

            IMapCameraControllerInternal CameraControllerBuilder() =>
                MapCameraController.MapCameraController.Create(mapCameraObjectPrefab, configuration.MapCamerasRoot, coordsUtils);

            IObjectPool<IMapCameraControllerInternal> pool = new ObjectPool<IMapCameraControllerInternal>(
                CameraControllerBuilder,
                x => x.SetActive(true),
                x => x.SetActive(false),
                x => x.Dispose()
            );

            var enumerator = UniTaskAsyncEnumerable.Create<(MapLayer, IMapLayerController)>(async (writer, token) =>
            {

                async UniTask CreateAtlas()
                {
                    var atlasChunkPrefab = await GetAtlasChunkPrefab(cancellationToken);

                    var chunkAtlas = new ChunkAtlasController(configuration.AtlasRoot, atlasChunkPrefab, MapRendererDrawOrder.ATLAS, ATLAS_CHUNK_SIZE,
                        coordsUtils, cullingController, ChunkController.CreateChunk);

                    // initialize Atlas but don't block the flow (to accelerate loading time)
                    chunkAtlas.Initialize(cancellationToken).SuppressCancellationThrow().Forget();

                    await writer.YieldAsync((MapLayer.Atlas, chunkAtlas));
                }

                await UniTask.WhenAll(
                    CreateAtlas(),
                    coldUsersMarkersInstaller.Install(writer, configuration, coordsUtils, cullingController, cancellationToken),
                    sceneOfInterestsMarkersInstaller.Install(writer, configuration, coordsUtils, cullingController, cancellationToken),
                    homePointMarkerInstaller.Install(writer, configuration, coordsUtils, cullingController, cancellationToken),
                    hotUsersMarkersInstaller.Install(writer, configuration, coordsUtils, cullingController, cancellationToken)
                    /* List of other creators that can be executed in parallel */);
            });

            return new MapRendererComponents(enumerator, cullingController, pool);
        }

        internal async Task<SpriteRenderer> GetAtlasChunkPrefab(CancellationToken cancellationToken) =>
            await AddressableProvider.GetAddressable<SpriteRenderer>(ATLAS_CHUNK_ADDRESS, cancellationToken);
    }
}
