using DCL.Rendering;

namespace DCL
{
    public static class PlatformContextFactory
    {
        public static PlatformContext CreateDefault()
        {
            return new PlatformContext(
                memoryManager: new MemoryManager(),
                cullingController: CullingController.Create(),
                clipboard: Clipboard.Create(),
                physicsSyncController: new PhysicsSyncController(),
                parcelScenesCleaner: new ParcelScenesCleaner(),
                webRequest: WebRequestController.Create(),
                serviceProviders: new ServiceProviders(),
                idleChecker: new IdleChecker(),
                avatarsLODController: new AvatarsLODController(),
                featureFlagController: new FeatureFlagController(),
                globalEvents: new GlobalEvents());
        }
    }
}