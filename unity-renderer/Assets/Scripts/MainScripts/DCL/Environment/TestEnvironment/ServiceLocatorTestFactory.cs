using DCL.Controllers;
using DCL.Helpers.NFT.Markets;
using DCL.Rendering;
using NSubstitute;

namespace DCL
{
    public static class ServiceLocatorTestFactory
    {
        public static ServiceLocator CreateMocked()
        {
            var result = new ServiceLocator();

            // Platform
            result.Register<IMemoryManager>(() => Substitute.For<IMemoryManager>());
            result.Register<ICullingController>(() => Substitute.For<ICullingController>());
            result.Register<IParcelScenesCleaner>(() => Substitute.For<IParcelScenesCleaner>());
            result.Register<IClipboard>(() => Substitute.For<IClipboard>());
            result.Register<IPhysicsSyncController>(() => Substitute.For<IPhysicsSyncController>());
            result.Register<IWebRequestController>(() => Substitute.For<IWebRequestController>());
            result.Register<IServiceProviders>(
                () =>
                {
                    var mockedProviders = Substitute.For<IServiceProviders>();
                    mockedProviders.theGraph.Returns( Substitute.For<ITheGraph>() );
                    mockedProviders.analytics.Returns( Substitute.For<IAnalytics>() );
                    mockedProviders.catalyst.Returns( Substitute.For<ICatalyst>() );
                    mockedProviders.openSea.Returns( Substitute.For<INFTMarket>() );
                    return mockedProviders;
                });

            result.Register<IUpdateEventHandler>(() => Substitute.For<IUpdateEventHandler>());

            // World runtime
            result.Register<IIdleChecker>(() => Substitute.For<IIdleChecker>());
            result.Register<IAvatarsLODController>(() => Substitute.For<IAvatarsLODController>());
            result.Register<IFeatureFlagController>(() => Substitute.For<IFeatureFlagController>());
            result.Register<IMessagingControllersManager>(() => Substitute.For<IMessagingControllersManager>());
            result.Register<IWorldState>(() => Substitute.For<IWorldState>());
            result.Register<ISceneController>(() => Substitute.For<ISceneController>());
            result.Register<ITeleportController>(() => Substitute.For<ITeleportController>());
            result.Register<ISceneBoundsChecker>(() => Substitute.For<ISceneBoundsChecker>());
            result.Register<IWorldBlockersController>(() => Substitute.For<IWorldBlockersController>());
            result.Register<IRuntimeComponentFactory>(() => Substitute.For<IRuntimeComponentFactory>());

            // HUD
            result.Register<IHUDFactory>(() => Substitute.For<IHUDFactory>());
            result.Register<IHUDController>(() => Substitute.For<IHUDController>());

            return result;
        }
    }
}