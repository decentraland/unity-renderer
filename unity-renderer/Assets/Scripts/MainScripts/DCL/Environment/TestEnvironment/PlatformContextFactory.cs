using DCL.Rendering;
using NSubstitute;

namespace DCL.Tests
{
    public static class PlatformContextFactory
    {
        public static PlatformContext CreateMocked() { return CreateWithCustomMocks(); }

        public static PlatformContext CreateWithGenericMocks(params object[] mocks)
        {
            IMemoryManager memoryManager = Substitute.For<IMemoryManager>();
            ICullingController cullingController = Substitute.For<ICullingController>();
            IParcelScenesCleaner parcelScenesCleaner = Substitute.For<IParcelScenesCleaner>();
            IClipboard clipboard = Substitute.For<IClipboard>();
            IPhysicsSyncController physicsSyncController = Substitute.For<IPhysicsSyncController>();
            IWebRequestController webRequest = Substitute.For<IWebRequestController>();
            IServiceProviders serviceProviders = Substitute.For<IServiceProviders>();
            IIdleChecker idleChecker = Substitute.For<IIdleChecker>();
            IAvatarsLODController avatarsLODController = Substitute.For<IAvatarsLODController>();
            IFeatureFlagController featureFlagController = Substitute.For<IFeatureFlagController>();
            IUpdateEventHandler updateEventHandler = Substitute.For<IUpdateEventHandler>();

            foreach (var mock in mocks)
            {
                switch ( mock )
                {
                    case ICullingController cc:
                        cullingController = cc;
                        break;
                    case IMemoryManager mm:
                        memoryManager = mm;
                        break;
                    case IClipboard c:
                        clipboard = c;
                        break;
                    case IPhysicsSyncController psc:
                        physicsSyncController = psc;
                        break;
                    case IParcelScenesCleaner pscc:
                        parcelScenesCleaner = pscc;
                        break;
                    case IWebRequestController wrc:
                        webRequest = wrc;
                        break;
                    case IServiceProviders sp:
                        serviceProviders = sp;
                        break;
                    case IIdleChecker idl:
                        idleChecker = idl;
                        break;
                    case IAvatarsLODController alodc:
                        avatarsLODController = alodc;
                        break;
                    case IFeatureFlagController iffc:
                        featureFlagController = iffc;
                        break;
                    case IUpdateEventHandler ueh:
                        updateEventHandler = ueh;
                        break;
                }
            }

            return new PlatformContext(
                memoryManager,
                cullingController,
                clipboard,
                physicsSyncController,
                parcelScenesCleaner,
                webRequest,
                serviceProviders,
                idleChecker,
                avatarsLODController,
                featureFlagController,
                updateEventHandler);
        }

        public static PlatformContext CreateWithCustomMocks(
            IMemoryManager memoryManager = null,
            ICullingController cullingController = null,
            IClipboard clipboard = null,
            IPhysicsSyncController physicsSyncController = null,
            IParcelScenesCleaner parcelScenesCleaner = null,
            IWebRequestController webRequestController = null,
            IServiceProviders serviceProviders = null,
            IIdleChecker idleChecker = null,
            IAvatarsLODController avatarsLODController = null,
            IFeatureFlagController featureFlagController = null,
            IUpdateEventHandler updateEventHandler = null)
        {
            return new PlatformContext(
                memoryManager: memoryManager ?? Substitute.For<IMemoryManager>(),
                cullingController: cullingController ?? Substitute.For<ICullingController>(),
                clipboard: clipboard ?? Substitute.For<IClipboard>(),
                physicsSyncController: physicsSyncController ?? Substitute.For<IPhysicsSyncController>(),
                parcelScenesCleaner: parcelScenesCleaner ?? Substitute.For<IParcelScenesCleaner>(),
                webRequest: webRequestController ?? GetWebRequestControllerMock(),
                serviceProviders: serviceProviders ?? GetServiceProvidersMock(),
                idleChecker: idleChecker ?? Substitute.For<IIdleChecker>(),
                avatarsLODController: avatarsLODController ?? Substitute.For<IAvatarsLODController>(),
                featureFlagController: featureFlagController ?? Substitute.For<IFeatureFlagController>(),
                updateEventHandler: updateEventHandler ?? Substitute.For<IUpdateEventHandler>());
        }

        private static IWebRequestController GetWebRequestControllerMock()
        {
            IWebRequestController webRequestControllerMock = Substitute.For<IWebRequestController>();
            webRequestControllerMock.Initialize(
                genericWebRequest: Substitute.For<IWebRequest>(),
                assetBundleWebRequest: Substitute.For<IWebRequestAssetBundle>(),
                textureWebRequest: Substitute.For<IWebRequestTexture>(),
                audioWebRequest: Substitute.For<IWebRequestAudio>());

            return webRequestControllerMock;
        }

        private static IServiceProviders GetServiceProvidersMock()
        {
            IServiceProviders serviceProviders = Substitute.For<IServiceProviders>();

            ITheGraph theGraph = Substitute.For<ITheGraph>();
            serviceProviders.theGraph.Returns(theGraph);

            ICatalyst catalyst = Substitute.For<ICatalyst>();
            serviceProviders.catalyst.Returns(catalyst);

            return serviceProviders;
        }
    }
}