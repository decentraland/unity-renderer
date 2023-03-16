using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DCL;
using DCL.Providers;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.PointsOfInterest;
using MainScripts.DCL.Helpers.Utils;
using System.Threading;
using UnityEngine.Pool;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    internal struct SceneOfInterestsMarkersInstaller
    {
        private const int PREWARM_COUNT = 60;
        private const string SCENE_OF_INTEREST_MARKER_ADDRESS = "SceneOfInterestMarker";

        private Service<IAddressableResourceProvider> addressablesProvider;

        public async UniTask Install(
            IAsyncWriter<(MapLayer, IMapLayerController)> writer,
            MapRendererConfiguration configuration,
            ICoordsUtils coordsUtils,
            IMapCullingController cullingController,
            CancellationToken cancellationToken
        )
        {
            var prefab = await GetPrefab(cancellationToken);

            var objectsPool = new UnityObjectPool<SceneOfInterestMarkerObject>(
                prefab,
                configuration.ScenesOfInterestMarkersRoot,
                actionOnCreate: SetSortingOrder,
                defaultCapacity: PREWARM_COUNT);

            var controller = new ScenesOfInterestMarkersController(
                MinimapMetadata.GetMetadata(),
                objectsPool,
                CreateMarker,
                PREWARM_COUNT,
                configuration.ScenesOfInterestMarkersRoot,
                coordsUtils,
                cullingController,
                MapRendererDrawOrder.SCENES_OF_INTEREST
            );

            await controller.Initialize(cancellationToken);
            await writer.YieldAsync((MapLayer.ScenesOfInterest, controller));
        }

        private static ISceneOfInterestMarker CreateMarker(IObjectPool<SceneOfInterestMarkerObject> objectsPool, IMapCullingController cullingController) =>
            new SceneOfInterestMarker(objectsPool, cullingController);

        private void SetSortingOrder(SceneOfInterestMarkerObject obj)
        {
            for (var i = 0; i < obj.renderers.Length; i++)
                obj.renderers[i].sortingOrder = MapRendererDrawOrder.SCENES_OF_INTEREST;

            obj.title.sortingOrder = MapRendererDrawOrder.SCENES_OF_INTEREST;
        }

        internal async UniTask<SceneOfInterestMarkerObject> GetPrefab(CancellationToken cancellationToken) =>
            await addressablesProvider.Ref.GetAddressable<SceneOfInterestMarkerObject>(SCENE_OF_INTEREST_MARKER_ADDRESS, cancellationToken);
    }
}
