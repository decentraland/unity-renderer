namespace DCL
{
    public class Environment
    {
        public static readonly Environment i = new Environment();

        public readonly MessagingControllersManager messagingControllersManager;

        /*
         * TODO: Continue moving static instances to this class. Each static instance should be converted to a local instance inside this class.
         *
        MemoryManager memoryManager;
        PointerEventsController pointerEventsController;
        ParcelScenesCleaner parcelScenesCleaner; // This is a static member of ParcelScene
        PoolManager poolManager; // This should be created through a Factory, and that factopry should execute the code in the method EnsureEntityPool

        */

        private Environment()
        {
            messagingControllersManager = new MessagingControllersManager();
        }

        public void Initialize(IMessageProcessHandler messageHandler)
        {
            messagingControllersManager.Initialize(messageHandler);
        }

        public void Restart(IMessageProcessHandler messageHandler)
        {
            messagingControllersManager.Cleanup();

            this.Initialize(messageHandler);
        }
    }
}