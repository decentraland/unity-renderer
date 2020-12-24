using DCL.Rendering;

namespace DCL
{
    /// <summary>
    /// Context related to specific platform handling.
    /// Memory, rendering, input, IO and debug systems belong here.
    /// </summary>
    public class PlatformContext : System.IDisposable
    {
        public readonly MemoryManager memoryManager;
        public readonly ICullingController cullingController;
        public readonly IParcelScenesCleaner parcelScenesCleaner;
        public readonly Clipboard clipboard;
        public readonly PhysicsSyncController physicsSyncController;
        public readonly DebugController debugController;

        public PlatformContext(MemoryManager memoryManager,
            CullingController cullingController,
            Clipboard clipboard,
            PhysicsSyncController physicsSyncController,
            IParcelScenesCleaner parcelScenesCleaner,
            DebugController debugController)
        {
            this.memoryManager = memoryManager;
            this.cullingController = cullingController;
            this.clipboard = clipboard;
            this.physicsSyncController = physicsSyncController;
            this.parcelScenesCleaner = parcelScenesCleaner;
            this.debugController = debugController;
        }

        public void Dispose()
        {
            memoryManager.Dispose();
            parcelScenesCleaner.Dispose();
            cullingController.Dispose();
            debugController.Dispose();
        }
    }
}