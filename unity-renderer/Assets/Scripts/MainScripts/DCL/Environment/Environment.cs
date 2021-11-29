using System;
using System.ComponentModel;
using DCL.Controllers;
using UnityEngine.SceneManagement;

namespace DCL
{
    public class Environment
    {
        public static Model i = new Model(new ServiceLocator());

        /// <summary>
        /// Setup the environment with the configured builder funcs.
        /// </summary>
        public static void Setup(ServiceLocator serviceLocator)
        {
            i = new Model(serviceLocator);
            serviceLocator.Initialize();
        }

        /// <summary>
        /// Wire the system dependencies. We should improve this approach later.
        /// </summary>
        // private static void Initialize()
        // {
        //     Model model = i;
        //
        //     //TODO(Brian): We can move to a RAII scheme + promises later to make this
        //     //             more scalable.
        //
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
        // }

        /// <summary>
        /// Dispose() and Setup() using the current environment configuration.
        /// </summary>
        public static void Reset()
        {
            Dispose();
        }

        /// <summary>
        /// Dispose() all the current environment systems.
        /// </summary>
        public static void Dispose()
        {
            i?.Dispose();
        }

        public class Model : IDisposable
        {
            public readonly ServiceLocator serviceLocator;
            public readonly MessagingContext messaging;
            public readonly PlatformContext platform;
            public readonly WorldRuntimeContext world;
            public readonly HUDContext hud;

            public Model (ServiceLocator serviceLocator)
            {
                this.serviceLocator = serviceLocator;
                messaging = new MessagingContext(serviceLocator);
                platform = new PlatformContext(serviceLocator);
                world = new WorldRuntimeContext(serviceLocator);
                hud = new HUDContext(serviceLocator);
            }

            public void Dispose()
            {
                this.serviceLocator.Dispose();
            }
        }
    }
}