using DCL.Rendering;
using NSubstitute;

namespace DCL.Tests
{
    public static class PlatformContextFactory
    {
        public static PlatformContext CreateMocked() { return CreateWithCustomMocks(); }

        public static PlatformContext CreateWithCustomMocks(
            IMemoryManager memoryManager = null,
            ICullingController cullingController = null,
            IClipboard clipboard = null,
            IPhysicsSyncController physicsSyncController = null,
            IParcelScenesCleaner parcelScenesCleaner = null,
            IDebugController debugController = null,
            IWebRequestController webRequestController = null,
            IServiceProviders serviceProviders = null)
        {
            return new PlatformContext(
                memoryManager: memoryManager ?? Substitute.For<IMemoryManager>(),
                cullingController: cullingController ?? Substitute.For<ICullingController>(),
                clipboard: clipboard ?? Substitute.For<IClipboard>(),
                physicsSyncController: physicsSyncController ?? Substitute.For<IPhysicsSyncController>(),
                parcelScenesCleaner: parcelScenesCleaner ?? Substitute.For<ParcelScenesCleaner>(),
                debugController: debugController ?? Substitute.For<IDebugController>(),
                webRequest: webRequestController ?? GetWebRequestControllerMock(),
                serviceProviders: serviceProviders ?? GetServiceProvidersMock());
        }

        private static IWebRequestController GetWebRequestControllerMock()
        {
            IWebRequestController webRequestControllerMock = Substitute.For<IWebRequestController>();
            webRequestControllerMock.Initialize(
                genericWebRequest: Substitute.For<IWebRequest>(),
                assetBundleWebRequest: Substitute.For<IWebRequest>(),
                textureWebRequest: Substitute.For<IWebRequest>(),
                audioWebRequest: Substitute.For<IWebRequestAudio>());

            return webRequestControllerMock;
        }
        
        private static IServiceProviders GetServiceProvidersMock()
        {
            IServiceProviders serviceProviders = Substitute.For<IServiceProviders>();
            
            ITheGraph theGraph = Substitute.For<ITheGraph>();
            serviceProviders.theGraph.Returns(theGraph);
            
            return serviceProviders;
        }
    }
}