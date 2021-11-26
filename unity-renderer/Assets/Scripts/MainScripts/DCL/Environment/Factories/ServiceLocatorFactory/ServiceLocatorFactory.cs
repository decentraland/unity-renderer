using DCL.Controllers;
using DCL.Rendering;
using NSubstitute;
using UnityEngine;

namespace DCL
{
    public static class ServiceLocatorFactory
    {
        public static ServiceLocator CreateDefault()
        {
            var result = new ServiceLocator();

            // Platform
            var memoryManager = result.Set<IMemoryManager>(new MemoryManager());
            var cullingController = result.Set<ICullingController>(CullingController.Create());
            var parcelScenesCleaner = result.Set<IParcelScenesCleaner>(new ParcelScenesCleaner());
            var clipboard = result.Set<IClipboard>(Clipboard.Create());
            var physicsSyncController = result.Set<IPhysicsSyncController>(new PhysicsSyncController());
            var webRequestController = result.Set<IWebRequestController>(WebRequestController.Create());
            var serviceProviders = result.Set<IServiceProviders>(new ServiceProviders());

            // World runtime
            var idleChecker = result.Set<IIdleChecker>(new IdleChecker());
            var avatarsLODController = result.Set<IAvatarsLODController>(new AvatarsLODController());
            var featureFlagController = result.Set<IFeatureFlagController>(new FeatureFlagController());
            var sceneController = result.Set<ISceneController>(new SceneController());
            var state = result.Set<IWorldState>(new WorldState());
            var pointerEventsController = result.Set<IPointerEventsController>(new PointerEventsController());
            var sceneBoundsChecker = result.Set<ISceneBoundsChecker>(new SceneBoundsChecker());
            var worldBlockersController = result.Set<IWorldBlockersController>(WorldBlockersController.CreateWithDefaultDependencies(state, cullingController));
            var runtimeComponentFactory = result.Set<IRuntimeComponentFactory>(new RuntimeComponentFactory());

            var messagingControllersManager = result.Set<IMessagingControllersManager>(new MessagingControllersManager(sceneController));

            // HUD
            var factory = result.Set<IHUDFactory>(new HUDFactory());
            var hudController = result.Set<IHUDController>(new HUDController(factory));

            //     // Messaging systems
            //     model.messaging.manager.Initialize(i.world.sceneController);
            //
            //     // Platform systems
            //     model.platform.cullingController.Start();
            //     model.platform.parcelScenesCleaner.Initialize();
            //
            //     // World context systems
            //     model.world.sceneController.Initialize();
            //     model.world.pointerEventsController.Initialize();
            //     model.world.state.Initialize();
            //     model.world.blockersController.InitializeWithDefaultDependencies(
            //         model.world.state, model.platform.cullingController);
            //     model.world.sceneBoundsChecker.Start();
            //     model.world.componentFactory.Initialize();
            //
            //     // HUD context system
            //     model.hud.controller.Initialize(model.hud.factory);

            return result;
        }
    }
}