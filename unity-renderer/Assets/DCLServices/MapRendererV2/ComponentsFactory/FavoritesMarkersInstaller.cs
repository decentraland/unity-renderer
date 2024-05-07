using Cysharp.Threading.Tasks;
using DCL;
using DCL.Providers;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.Favorites;
using DCLServices.PlacesAPIService;
using MainScripts.DCL.Helpers.Utils;
using System.Collections.Generic;
using System.Threading;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    internal struct FavoritesMarkersInstaller
    {
        private const int PREWARM_COUNT = 60;
        private const string FAVORITES_MARKER_ADDRESS = "FavoritesMarker";

        private Service<IAddressableResourceProvider> addressablesProvider;

        public async UniTask Install(
            Dictionary<MapLayer, IMapLayerController> writer,
            List<IZoomScalingLayer> zoomScalingWriter,
            MapRendererConfiguration configuration,
            ICoordsUtils coordsUtils,
            IMapCullingController cullingController,
            IPlacesAPIService placesAPIService,
            CancellationToken cancellationToken
        )
        {
            var prefab = await GetPrefab(cancellationToken);

            var objectsPool = new UnityObjectPool<FavoriteMarkerObject>(
                prefab,
                configuration.FavoritesMarkersRoot,
                actionOnCreate: (obj) =>
                {
                    for (var i = 0; i < obj.renderers.Length; i++)
                        obj.renderers[i].sortingOrder = MapRendererDrawOrder.FAVORITES;

                    obj.title.sortingOrder = MapRendererDrawOrder.FAVORITES;
                    coordsUtils.SetObjectScale(obj);
                },
                defaultCapacity: PREWARM_COUNT);

            var controller = new FavoritesMarkerController(
                placesAPIService,
                objectsPool,
                CreateMarker,
                PREWARM_COUNT,
                configuration.FavoritesMarkersRoot,
                coordsUtils,
                cullingController
            );

            await controller.Initialize(cancellationToken);
            writer.Add(MapLayer.Favorites, controller);
            zoomScalingWriter.Add(controller);
        }

        private static IFavoritesMarker CreateMarker(IUnityObjectPool<FavoriteMarkerObject> objectsPool, IMapCullingController cullingController) =>
            new FavoritesMarker(objectsPool, cullingController);

        internal async UniTask<FavoriteMarkerObject> GetPrefab(CancellationToken cancellationToken) =>
            await addressablesProvider.Ref.GetAddressable<FavoriteMarkerObject>(FAVORITES_MARKER_ADDRESS, cancellationToken);

    }
}
