using DCL.Controllers;
using DCL.Rendering;
using NSubstitute;

namespace DCL.Tests
{
    public static class ServiceLocatorFactory
    {
        public static ServiceLocator CreateMocked()
        {
            var result = new ServiceLocator();

            // Platform
            result.Set<IMemoryManager>(Substitute.For<IMemoryManager>());
            result.Set<ICullingController>(Substitute.For<ICullingController>());
            result.Set<IParcelScenesCleaner>(Substitute.For<IParcelScenesCleaner>());
            result.Set<IClipboard>(Substitute.For<IClipboard>());
            result.Set<IPhysicsSyncController>(Substitute.For<IPhysicsSyncController>());
            result.Set<IWebRequestController>(Substitute.For<IWebRequestController>());
            result.Set<IServiceProviders>(Substitute.For<IServiceProviders>());

            // World runtime
            result.Set<IIdleChecker>(Substitute.For<IIdleChecker>());
            result.Set<IAvatarsLODController>(Substitute.For<IAvatarsLODController>());
            result.Set<IFeatureFlagController>(Substitute.For<IFeatureFlagController>());
            result.Set<IMessagingControllersManager>(Substitute.For<IMessagingControllersManager>());
            result.Set<IWorldState>(Substitute.For<IWorldState>());
            result.Set<ISceneController>(Substitute.For<ISceneController>());
            result.Set<IPointerEventsController>(Substitute.For<IPointerEventsController>());
            result.Set<ISceneBoundsChecker>(Substitute.For<ISceneBoundsChecker>());
            result.Set<IWorldBlockersController>(Substitute.For<IWorldBlockersController>());
            result.Set<IRuntimeComponentFactory>(Substitute.For<IRuntimeComponentFactory>());

            // HUD
            result.Set<IHUDFactory>(Substitute.For<IHUDFactory>());
            result.Set<IHUDController>(Substitute.For<IHUDController>());

            return result;
        }
    }
}