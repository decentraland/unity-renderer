using DCL;
using DCL.Providers;
using DCLServices.MapRendererV2.ComponentsFactory;
using MainScripts.DCL.Controllers.HotScenes;
using System.Collections.Generic;

namespace DCLServices.MapRendererV2.TestScene
{
    public static class MapRendererTestSceneServiceLocatorFactory
    {
        public static (ServiceLocator, IReadOnlyList<IMapRendererTestSceneElementProvider>) Create(MapRendererTestSceneContainer container, int parcelSize, int atlasChunkSize)
        {
            var result = new ServiceLocator();

            var addressableResourceProvider = new AddressableResourceProvider();
            result.Register<IAddressableResourceProvider>(() => addressableResourceProvider);

            var mapRenderer = new MapRenderer(new MapRendererChunkComponentsFactory(parcelSize, atlasChunkSize));

            result.Register<IMapRenderer>(() => mapRenderer);
            result.Register<IHotScenesFetcher>(() => container.hotScenesController);
            result.Register<IWebRequestController>(WebRequestController.Create);

            var elements = new IMapRendererTestSceneElementProvider[]
            {
                new MapRendererTestSceneCameraRentals(mapRenderer),
                new MapRendererTestScenePlayerMarker(DataStore.i.player.playerWorldPosition, CommonScriptableObjects.cameraForward),
                new MapRendererTestSceneHomePoint(DataStore.i.HUDs.homePoint)
            };

            return (result, elements);
        }
    }
}
