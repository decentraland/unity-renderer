using DCL.Bots;
using DCL.Rendering;

namespace DCL
{
    /// <summary>
    /// Context related to specific platform handling.
    /// Memory, rendering, input, IO and debug systems belong here.
    /// </summary>
    [System.Obsolete("This is kept for retrocompatibilty and will be removed in the future. Use Environment.i.serviceLocator instead.")]
    public class PlatformContext
    {
        public ServiceLocator serviceLocator;
        public IMemoryManager memoryManager => serviceLocator.Get<IMemoryManager>();
        public ICullingController cullingController  => serviceLocator.Get<ICullingController>();
        public IParcelScenesCleaner parcelScenesCleaner  => serviceLocator.Get<IParcelScenesCleaner>();
        public IClipboard clipboard  => serviceLocator.Get<IClipboard>();
        public IPhysicsSyncController physicsSyncController  => serviceLocator.Get<IPhysicsSyncController>();
        public IWebRequestController webRequest => serviceLocator.Get<IWebRequestController>();
        public IServiceProviders serviceProviders => serviceLocator.Get<IServiceProviders>();
        public IIdleChecker idleChecker => serviceLocator.Get<IIdleChecker>();
        public IAvatarsLODController avatarsLODController => serviceLocator.Get<IAvatarsLODController>();
        public IFeatureFlagController featureFlagController => serviceLocator.Get<IFeatureFlagController>();
        public IUpdateEventHandler updateEventHandler => serviceLocator.Get<IUpdateEventHandler>();

        public PlatformContext (ServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }
    }
}