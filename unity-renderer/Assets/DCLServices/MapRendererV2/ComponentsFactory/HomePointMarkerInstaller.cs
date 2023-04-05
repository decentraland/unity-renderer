using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using DCL;
using DCL.Providers;
using DCLServices.MapRendererV2.CoordsUtils;
using DCLServices.MapRendererV2.Culling;
using DCLServices.MapRendererV2.MapLayers;
using DCLServices.MapRendererV2.MapLayers.HomePoint;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCLServices.MapRendererV2.ComponentsFactory
{
    internal struct HomePointMarkerInstaller
    {
        private const string HOME_POINT_MARKER_ADDRESS = "HomePoint";

        private Service<IAddressableResourceProvider> addressablesProvider;

        public async UniTask Install(
            Dictionary<MapLayer, IMapLayerController> writer,
            MapRendererConfiguration configuration,
            ICoordsUtils coordsUtils,
            IMapCullingController cullingController,
            CancellationToken cancellationToken
        )
        {
            var prefab = await GetPrefab(cancellationToken);

            IHomePointMarker CreateMarker(Transform parent)
            {
                var obj = Object.Instantiate(prefab, configuration.HomePointRoot);
                obj.spriteRenderer.sortingOrder = MapRendererDrawOrder.HOME_POINT;
                coordsUtils.SetObjectScale(obj);
                return new HomePointMarker(obj);
            }

            var controller = new HomePointMarkerController(
                DataStore.i.HUDs.homePoint,
                CreateMarker,
                configuration.HomePointRoot,
                coordsUtils,
                cullingController);

            controller.Initialize();
            writer.Add(MapLayer.HomePoint, controller);
        }

        internal async UniTask<HomePointMarkerObject> GetPrefab(CancellationToken cancellationToken) =>
            await addressablesProvider.Ref.GetAddressable<HomePointMarkerObject>(HOME_POINT_MARKER_ADDRESS, cancellationToken);
    }
}
