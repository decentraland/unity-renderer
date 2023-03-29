using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DCL;
using DCL.Providers;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapCameraController;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.Atlas;
using DCLServices.MapRendererV2.MapLayers.ParcelHighlight;
using MainScripts.DCL.Controllers.HotScenes;
using MainScripts.DCL.Helpers.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    public class MapRendererChunkComponentsFactory : IMapRendererComponentsFactory
    {
        private readonly int parcelSize;
        private readonly int atlasChunkSize;
        private readonly float cullingBounds;

        private const string ATLAS_CHUNK_ADDRESS = "AtlasChunk";
        private const string MAP_CONFIGURATION_ADDRESS = "MapRendererConfiguration";
        private const string MAP_CAMERA_OBJECT_ADDRESS = "MapCameraObject";
        private const string PARCEL_HIGHLIGHT_OBJECT_ADDRESS = "MapParcelHighlightMarker";

        private Service<IAddressableResourceProvider> addressablesProvider;
        private Service<IHotScenesFetcher> hotScenesFetcher;

        public MapRendererChunkComponentsFactory(int parcelSize, int atlasChunkSize, float cullingBounds)
        {
            this.parcelSize = parcelSize;
            this.atlasChunkSize = atlasChunkSize;
            this.cullingBounds = cullingBounds;
        }

        internal ColdUsersMarkersInstaller coldUsersMarkersInstaller { get; }
        internal SceneOfInterestsMarkersInstaller sceneOfInterestsMarkersInstaller { get; }
        internal HomePointMarkerInstaller homePointMarkerInstaller { get; }
        internal HotUsersMarkersInstaller hotUsersMarkersInstaller { get; }
        internal PlayerMarkerInstaller playerMarkerInstaller { get; }

        private IAddressableResourceProvider AddressableProvider => addressablesProvider.Ref;

        async UniTask<MapRendererComponents> IMapRendererComponentsFactory.Create(CancellationToken cancellationToken)
        {
            var configuration = Object.Instantiate(await AddressableProvider.GetAddressable<MapRendererConfiguration>(MAP_CONFIGURATION_ADDRESS, cancellationToken), new Vector3(10000, 10000, 0), Quaternion.identity);
            var coordsUtils = new ChunkCoordsUtils(parcelSize);

            IMapCullingController cullingController = new MapCullingController(new MapCullingRectVisibilityChecker(cullingBounds * parcelSize));

            var highlightMarkerPrefab = await GetParcelHighlightMarkerPrefab(cancellationToken);

            var highlightMarkersPool = new ObjectPool<IParcelHighlightMarker>(
                () =>
                {
                    var obj = Object.Instantiate(highlightMarkerPrefab, configuration.ParcelHighlightRoot);
                    obj.spriteRenderer.sortingOrder = MapRendererDrawOrder.PARCEL_HIGHLIGHT;
                    obj.text.sortingOrder = MapRendererDrawOrder.PARCEL_HIGHLIGHT;
                    coordsUtils.SetObjectScale(obj);
                    return new ParcelHighlightMarker(obj);
                },
                _ => {},
                marker => marker.Deactivate(),
                marker => marker.Dispose()
            );

            highlightMarkersPool.Prewarm(1);

            var mapCameraObjectPrefab = await AddressableProvider.GetAddressable<MapCameraObject>(MAP_CAMERA_OBJECT_ADDRESS, cancellationToken);

            IMapCameraControllerInternal CameraControllerBuilder()
            {
                var instance = Object.Instantiate(mapCameraObjectPrefab, configuration.MapCamerasRoot);

                var interactivityController = new MapCameraInteractivityController(
                    configuration.MapCamerasRoot,
                    instance.mapCamera,
                    highlightMarkersPool,
                    coordsUtils);

                return new MapCameraController.MapCameraController(interactivityController, instance, coordsUtils, cullingController);
            }

            IObjectPool<IMapCameraControllerInternal> cameraControllersPool = new ObjectPool<IMapCameraControllerInternal>(
                CameraControllerBuilder,
                x => x.SetActive(true),
                x => x.SetActive(false),
                x => x.Dispose()
            );

            var layers = new Dictionary<MapLayer, IMapLayerController>();

            async UniTask CreateAtlas()
            {
                var atlasChunkPrefab = await GetAtlasChunkPrefab(cancellationToken);

                async UniTask<IChunkController> CreateChunk(
                    Vector3 chunkLocalPosition,
                    Vector2Int coordsCenter,
                    Transform parent,
                    CancellationToken ct)
                {
                    var chunk = new ChunkController(atlasChunkPrefab, chunkLocalPosition, coordsCenter, parent);
                    chunk.SetDrawOrder(MapRendererDrawOrder.ATLAS);
                    await chunk.LoadImage(atlasChunkSize, parcelSize, coordsCenter, ct);
                    return chunk;
                }

                var chunkAtlas = new ChunkAtlasController(configuration.AtlasRoot, atlasChunkPrefab, atlasChunkSize,
                    coordsUtils, cullingController, CreateChunk);

                // initialize Atlas but don't block the flow (to accelerate loading time)
                chunkAtlas.Initialize(cancellationToken).SuppressCancellationThrow().Forget();

                layers.Add(MapLayer.Atlas, chunkAtlas);
            }

            await UniTask.WhenAll(
                CreateAtlas(),
                coldUsersMarkersInstaller.Install(layers, configuration, coordsUtils, cullingController, cancellationToken),
                sceneOfInterestsMarkersInstaller.Install(layers, configuration, coordsUtils, cullingController, cancellationToken),
                playerMarkerInstaller.Install(layers, configuration, coordsUtils, cullingController, cancellationToken),
                homePointMarkerInstaller.Install(layers, configuration, coordsUtils, cullingController, cancellationToken),
                hotUsersMarkersInstaller.Install(layers, configuration, coordsUtils, cullingController, cancellationToken)
                /* List of other creators that can be executed in parallel */);

            return new MapRendererComponents(configuration, layers, cullingController, cameraControllersPool);
        }

        internal async Task<SpriteRenderer> GetAtlasChunkPrefab(CancellationToken cancellationToken) =>
            await AddressableProvider.GetAddressable<SpriteRenderer>(ATLAS_CHUNK_ADDRESS, cancellationToken);

        internal async UniTask<ParcelHighlightMarkerObject> GetParcelHighlightMarkerPrefab(CancellationToken cancellationToken) =>
            await AddressableProvider.GetAddressable<ParcelHighlightMarkerObject>(PARCEL_HIGHLIGHT_OBJECT_ADDRESS, cancellationToken);
    }
}
