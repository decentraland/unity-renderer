namespace DCL
{
    public class Environment
    {
        public static readonly Environment i = new Environment();

        public readonly MessagingControllersManager messagingControllersManager;
        public readonly PointerEventsController pointerEventsController;
        public readonly MemoryManager memoryManager;
        public InteractionHoverCanvasController interactionHoverCanvasController { get; private set; }

        /*
         * TODO: Continue moving static instances to this class. Each static instance should be converted to a local instance inside this class.
         *
        ParcelScenesCleaner parcelScenesCleaner; // This is a static member of ParcelScene
        PoolManager poolManager; // This should be created through a Factory, and that factopry should execute the code in the method EnsureEntityPool

        */

        private Environment()
        {
            messagingControllersManager = new MessagingControllersManager();
            pointerEventsController = new PointerEventsController();
            memoryManager = new MemoryManager();
        }

        public void Initialize(IMessageProcessHandler messageHandler, bool isTesting = false)
        {
            messagingControllersManager.Initialize(messageHandler);
            pointerEventsController.Initialize(isTesting);
            memoryManager.Initialize();
        }

        public void SetInteractionHoverCanvasController(InteractionHoverCanvasController controller)
        {
            interactionHoverCanvasController = controller;
        }

        public void Cleanup()
        {
            messagingControllersManager.Cleanup();
            memoryManager.CleanupPoolsIfNeeded(true);
            pointerEventsController.Cleanup();
        }

        public void Restart(IMessageProcessHandler messageHandler, bool isTesting = false)
        {
            Cleanup();

            Initialize(messageHandler, isTesting);
        }
    }
}