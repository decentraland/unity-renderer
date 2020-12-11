using DCL.Configuration;
using DCL.Controllers;
using DCL.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public class Environment
    {
        public static readonly Environment i = new Environment();

        public SceneController sceneController { get; private set; }
        public DebugController debugController { get; private set; }
        public readonly WorldState worldState;
        public readonly MessagingControllersManager messagingControllersManager;
        public readonly PointerEventsController pointerEventsController;
        public readonly MemoryManager memoryManager;

        public SceneBoundsChecker sceneBoundsChecker { get; private set; }
        public WorldBlockersController worldBlockersController { get; private set; }
        public ICullingController cullingController { get; private set; }
        public InteractionHoverCanvasController interactionHoverCanvasController { get; private set; }

        public IParcelScenesCleaner parcelScenesCleaner { get; private set; }


        public Clipboard clipboard { get; }

        public PerformanceMetricsController performanceMetricsController { get; private set; }

        public PhysicsSyncController physicsSyncController { get; private set; }


        private bool initialized;

        private Environment()
        {
            messagingControllersManager = new MessagingControllersManager();
            pointerEventsController = new PointerEventsController();
            memoryManager = new MemoryManager();
            physicsSyncController = new PhysicsSyncController();
            performanceMetricsController = new PerformanceMetricsController();
            clipboard = Clipboard.Create();
            sceneBoundsChecker = new SceneBoundsChecker();
            parcelScenesCleaner = new ParcelScenesCleaner();
            cullingController = CullingController.Create();
            worldState = new WorldState();
            debugController = new DebugController();
            sceneController = new SceneController();
        }

        public void Initialize()
        {
            if (initialized)
                return;

            sceneController.Initialize();
            messagingControllersManager.Initialize(sceneController);
            pointerEventsController.Initialize();
            memoryManager.Initialize();
            worldState.Initialize();
            worldBlockersController = WorldBlockersController.CreateWithDefaultDependencies(worldState, DCLCharacterController.i.characterPosition);
            parcelScenesCleaner.Start();
            cullingController.Start();
            sceneBoundsChecker.Start();
            initialized = true;
        }

        public void SetInteractionHoverCanvasController(InteractionHoverCanvasController controller)
        {
            interactionHoverCanvasController = controller;
        }

        public void Cleanup()
        {
            if (!initialized)
                return;

            initialized = false;

            messagingControllersManager.Cleanup();
            memoryManager.CleanupPoolsIfNeeded(true);
            pointerEventsController.Cleanup();
            sceneBoundsChecker.Stop();
            worldBlockersController.Dispose();
            parcelScenesCleaner.Dispose();
            cullingController.Dispose();
            debugController.Dispose();
        }

        public void Restart()
        {
            Cleanup();
            Initialize();
        }
    }
}