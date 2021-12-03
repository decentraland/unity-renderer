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
            result.Register<IMemoryManager>(() => new MemoryManager());
            result.Register<ICullingController>(CullingController.Create);
            result.Register<IParcelScenesCleaner>(() => new ParcelScenesCleaner());
            result.Register<IClipboard>(Clipboard.Create);
            result.Register<IPhysicsSyncController>(() => new PhysicsSyncController());
            result.Register<IWebRequestController>(WebRequestController.Create);
            result.Register<IServiceProviders>(() => new ServiceProviders());
            result.Register<IUpdateEventHandler>(() => new UpdateEventHandler());

            // World runtime
            result.Register<IIdleChecker>(() => new IdleChecker());
            result.Register<IAvatarsLODController>(() => new AvatarsLODController());
            result.Register<IFeatureFlagController>(() => new FeatureFlagController());
            result.Register<ISceneController>(() => new SceneController());
            result.Register<IWorldState>(() => new WorldState());
            result.Register<IPointerEventsController>(() => new PointerEventsController());
            result.Register<ISceneBoundsChecker>(() => new SceneBoundsChecker());
            result.Register<IWorldBlockersController>(() => new WorldBlockersController());
            result.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());

            result.Register<IMessagingControllersManager>(() => new MessagingControllersManager());

            // HUD
            result.Register<IHUDFactory>(() => new HUDFactory());
            result.Register<IHUDController>(() => new HUDController());

            return result;
        }
    }
}