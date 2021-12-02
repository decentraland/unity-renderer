using DCL.Controllers;
using DCL.Rendering;
using UnityEngine;

namespace DCL
{
    public static class ServiceLocatorFactory
    {
        public static ServiceLocator CreateDefault()
        {
            var result = new ServiceLocator();

            // Platform
            var memoryManager = result.Register<IMemoryManager>(() => new MemoryManager());
            var cullingController = result.Register<ICullingController>(CullingController.Create);
            var parcelScenesCleaner = result.Register<IParcelScenesCleaner>(() => new ParcelScenesCleaner());
            var clipboard = result.Register<IClipboard>(Clipboard.Create);
            var physicsSyncController = result.Register<IPhysicsSyncController>(() => new PhysicsSyncController());
            var webRequestController = result.Register<IWebRequestController>(WebRequestController.Create);
            var serviceProviders = result.Register<IServiceProviders>(() => new ServiceProviders());

            // World runtime
            var idleChecker = result.Register<IIdleChecker>(() => new IdleChecker());
            var avatarsLODController = result.Register<IAvatarsLODController>(() => new AvatarsLODController());
            var featureFlagController = result.Register<IFeatureFlagController>(() => new FeatureFlagController());
            var sceneController = result.Register<ISceneController>(() => new SceneController());
            var state = result.Register<IWorldState>(() => new WorldState());
            var pointerEventsController = result.Register<IPointerEventsController>(() => new PointerEventsController());
            var sceneBoundsChecker = result.Register<ISceneBoundsChecker>(() => new SceneBoundsChecker());
            var worldBlockersController = result.Register<IWorldBlockersController>(() => new WorldBlockersController());
            var runtimeComponentFactory = result.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());

            var messagingControllersManager = result.Register<IMessagingControllersManager>(() => new MessagingControllersManager());

            // HUD
            var factory = result.Register<IHUDFactory>(() => new HUDFactory());
            var hudController = result.Register<IHUDController>(() => new HUDController());

            return result;
        }
    }
}