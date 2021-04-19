using System.ComponentModel;
using DCL.Controllers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DCL
{
    public class Environment
    {
        public static Model i = new Model();
        private static System.Func<MessagingContext> messagingBuilder;
        private static System.Func<PlatformContext> platformBuilder;
        private static System.Func<WorldRuntimeContext> worldRuntimeBuilder;

        /// <summary>
        /// Configure and setup the environment with custom implementations given by each func passed as parameter.
        ///
        /// The funcs are stored. So the same environment can be replicated by just calling Setup() later.
        /// </summary>
        /// <param name="messagingBuilder">A func returning a MessagingContext to be used by the environment.</param>
        /// <param name="platformBuilder">A func returning a PlatformContext to be used by the environment.</param>
        /// <param name="worldRuntimeBuilder">A func returning a WorldRuntimeContext to be used by the environment.</param>
        public static void SetupWithBuilders(
            System.Func<MessagingContext> messagingBuilder = null,
            System.Func<PlatformContext> platformBuilder = null,
            System.Func<WorldRuntimeContext> worldRuntimeBuilder = null)
        {
            Environment.messagingBuilder = messagingBuilder ?? MessagingContextFactory.CreateDefault;
            Environment.platformBuilder = platformBuilder ?? PlatformContextFactory.CreateDefault;
            Environment.worldRuntimeBuilder = worldRuntimeBuilder ?? WorldRuntimeContextFactory.CreateDefault;
            Setup();
        }

        /// <summary>
        /// Setup the environment with the configured builder funcs.
        /// </summary>
        public static void Setup()
        {
            i = new Model(messagingBuilder, platformBuilder, worldRuntimeBuilder);
            Initialize();
        }

        /// <summary>
        /// Wire the system dependencies. We should improve this approach later.
        /// </summary>
        private static void Initialize()
        {
            Model model = i;

            //TODO(Brian): We can move to a RAII scheme + promises later to make this
            //             more scalable.

            // World context systems
            model.world.sceneController.Initialize();
            model.world.pointerEventsController.Initialize();
            model.world.state.Initialize();
            model.world.blockersController.InitializeWithDefaultDependencies(
                model.world.state);
            model.world.sceneBoundsChecker.Start();
            model.world.componentFactory.Initialize();

            // Platform systems
            model.platform.memoryManager.Initialize();
            model.platform.parcelScenesCleaner.Start();
            model.platform.cullingController.Start();

            // Messaging systems
            model.messaging.manager.Initialize(i.world.sceneController);
        }

        /// <summary>
        /// Dispose() and Setup() using the current environment configuration.
        /// </summary>
        public static void Reset()
        {
            Dispose();
            Setup();
        }

        /// <summary>
        /// Dispose() all the current environment systems.
        /// </summary>
        public static void Dispose()
        {
            i?.Dispose();
        }

        public class Model
        {
            public readonly MessagingContext messaging;
            public readonly PlatformContext platform;
            public readonly WorldRuntimeContext world;

            public Model(System.Func<MessagingContext> messagingBuilder = null,
                System.Func<PlatformContext> platformBuilder = null,
                System.Func<WorldRuntimeContext> worldBuilder = null)
            {
                messagingBuilder = messagingBuilder ?? MessagingContextFactory.CreateDefault;
                platformBuilder = platformBuilder ?? PlatformContextFactory.CreateDefault;
                worldBuilder = worldBuilder ?? WorldRuntimeContextFactory.CreateDefault;

                this.messaging = messagingBuilder();
                this.platform = platformBuilder();
                this.world = worldBuilder();
            }

            public void Dispose()
            {
                messaging?.Dispose();
                world?.Dispose();
                platform?.Dispose();
            }
        }
    }
}