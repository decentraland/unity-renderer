﻿using System.ComponentModel;
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
        private static System.Func<HUDContext> hudBuilder;

        /// <summary>
        /// Configure and setup the environment with custom implementations given by each func passed as parameter.
        ///
        /// The funcs are stored. So the same environment can be replicated by just calling Setup() later.
        /// </summary>
        /// <param name="messagingBuilder">A func returning a MessagingContext to be used by the environment.</param>
        /// <param name="platformBuilder">A func returning a PlatformContext to be used by the environment.</param>
        /// <param name="worldRuntimeBuilder">A func returning a WorldRuntimeContext to be used by the environment.</param>
        public static void SetupWithBuilders(
            System.Func<MessagingContext> messagingBuilder,
            System.Func<PlatformContext> platformBuilder,
            System.Func<WorldRuntimeContext> worldRuntimeBuilder,
            System.Func<HUDContext> hudBuilder)
        {
            Environment.messagingBuilder = messagingBuilder;
            Environment.platformBuilder = platformBuilder;
            Environment.worldRuntimeBuilder = worldRuntimeBuilder;
            Environment.hudBuilder = hudBuilder;
            Setup();
        }

        /// <summary>
        /// Setup the environment with the configured builder funcs.
        /// </summary>
        public static void Setup()
        {
            i = new Model(messagingBuilder, platformBuilder, worldRuntimeBuilder, hudBuilder);
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

            // Messaging systems
            model.messaging.manager.Initialize(i.world.sceneController);

            // Platform systems
            model.platform.parcelScenesCleaner.Start();
            model.platform.memoryManager.Initialize(model.platform.parcelScenesCleaner);
            model.platform.cullingController.Start();

            // World context systems
            model.world.sceneController.Initialize();
            model.world.pointerEventsController.Initialize();
            model.world.state.Initialize();
            model.world.blockersController.InitializeWithDefaultDependencies(
                model.world.state, model.platform.cullingController);
            model.world.sceneBoundsChecker.Start();
            model.world.componentFactory.Initialize();
            
            // HUD context system
            model.hud.controller.Initialize(model.hud.factory);
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
        public static void Dispose() { i?.Dispose(); }

        public class Model
        {
            public readonly MessagingContext messaging;
            public readonly PlatformContext platform;
            public readonly WorldRuntimeContext world;
            public readonly HUDContext hud;

            public Model () { }

            public Model(System.Func<MessagingContext> messagingBuilder,
                System.Func<PlatformContext> platformBuilder,
                System.Func<WorldRuntimeContext> worldBuilder,
                System.Func<HUDContext> hudBuilder)
            {
                this.messaging = messagingBuilder();
                this.platform = platformBuilder();
                this.world = worldBuilder();
                this.hud = hudBuilder();
            }

            public void Dispose()
            {
                messaging?.Dispose();
                world?.Dispose();
                platform?.Dispose();
                hud?.Dispose();
            }
        }
    }
}