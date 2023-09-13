using AvatarSystem;
using DCL.Chat;
using DCL.Chat.Channels;
using DCL.Controllers;
using DCL.Helpers;
using DCL.ProfanityFiltering;
using DCL.Providers;
using DCL.Rendering;
using DCL.Services;
using DCL.Social.Chat;
using DCl.Social.Friends;
using DCL.Social.Friends;
using DCL.World.PortableExperiences;
using DCLServices.CameraReelService;
using DCLServices.CopyPaste.Analytics;
using DCLServices.CustomNftCollection;
using DCLServices.DCLFileBrowser;
using DCLServices.DCLFileBrowser.DCLFileBrowserFactory;
using DCLServices.EmotesCatalog;
using DCLServices.EmotesCatalog.EmotesCatalogService;
using DCLServices.EnvironmentProvider;
using DCLServices.Lambdas;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using DCLServices.MapRendererV2;
using DCLServices.MapRendererV2.ComponentsFactory;
using DCLServices.PlacesAPIService;
using DCLServices.PortableExperiences.Analytics;
using DCLServices.ScreencaptureCamera.Service;
using DCLServices.WearablesCatalogService;
using DCLServices.WorldsAPIService;
using MainScripts.DCL.Controllers.AssetManager;
using MainScripts.DCL.Controllers.FriendsController;
using MainScripts.DCL.Controllers.HotScenes;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using MainScripts.DCL.Helpers.SentryUtils;
using MainScripts.DCL.WorldRuntime.Debugging.Performance;
using System.Collections.Generic;
using WorldsFeaturesAnalytics;

namespace DCL
{
    public static class ServiceLocatorFactory
    {
        private static BaseVariable<FeatureFlag> featureFlagsDataStore;

        public static ServiceLocator CreateDefault()
        {
            var result = new ServiceLocator();
            IRPC irpc = new RPC();

            featureFlagsDataStore = DataStore.i.featureFlags.flags;

            var userProfileWebInterfaceBridge = new UserProfileWebInterfaceBridge();

            // Addressable Resource Provider
            var addressableResourceProvider = new AddressableResourceProvider();
            result.Register<IAddressableResourceProvider>(() => addressableResourceProvider);

            // Platform
            result.Register<IProfilerRecordsService>(() => new ProfilerRecordsService());
            result.Register<IMemoryManager>(() => new MemoryManager());
            result.Register<ICullingController>(CullingController.Create);
            result.Register<IParcelScenesCleaner>(() => new ParcelScenesCleaner());
            result.Register<IClipboard>(Clipboard.Create);
            result.Register<IPhysicsSyncController>(() => new PhysicsSyncController());
            result.Register<IRPC>(() => irpc);
            UnityEnvironmentProviderService environmentProviderService = new UnityEnvironmentProviderService(KernelConfig.i);
            result.Register<IEnvironmentProviderService>(() => environmentProviderService);

            var webRequestController = new WebRequestController(
                new GetWebRequestFactory(),
                new WebRequestAssetBundleFactory(),
                new WebRequestTextureFactory(),
                new WebRequestAudioFactory(),
                new PostWebRequestFactory(),
                new PutWebRequestFactory(),
                new PatchWebRequestFactory(),
                new DeleteWebRequestFactory(),
                new RPCSignRequest(irpc)
            );
            result.Register<IWebRequestController>(() => webRequestController);

            result.Register<IServiceProviders>(() => new ServiceProviders());
            result.Register<ILambdasService>(() => new LambdasService());
            result.Register<INamesService>(() => new NamesService());
            result.Register<ILandsService>(() => new LandsService());
            result.Register<IUpdateEventHandler>(() => new UpdateEventHandler());
            result.Register<IWebRequestMonitor>(() => new SentryWebRequestMonitor());

            // World runtime
            result.Register<IIdleChecker>(() => new IdleChecker());
            result.Register<IAvatarsLODController>(() => new AvatarsLODController());
            result.Register<IFeatureFlagController>(() => new FeatureFlagController());
            result.Register<IGPUSkinningThrottlerService>(() => GPUSkinningThrottlerService.Create(true));
            result.Register<ISceneController>(() => new SceneController(new PlayerPrefsConfirmedExperiencesRepository(new DefaultPlayerPrefs())));
            result.Register<IWorldState>(() => new WorldState());
            result.Register<ISceneBoundsChecker>(() => new SceneBoundsChecker());
            result.Register<IWorldBlockersController>(() => new WorldBlockersController());
            result.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
            result.Register<IAvatarFactory>(() => new AvatarFactory(result));
            result.Register<ICharacterPreviewFactory>(() => new CharacterPreviewFactory());
            result.Register<IChatController>(() => new ChatController(WebInterfaceChatBridge.GetOrCreate(), DataStore.i));

            result.Register<ISocialApiBridge>(() =>
            {
                var rpcSocialApiBridge = new RPCSocialApiBridge(MatrixInitializationBridge.GetOrCreate(),
                    userProfileWebInterfaceBridge,
                    new RPCSocialClientProvider(KernelConfig.i));

                return new ProxySocialApiBridge(rpcSocialApiBridge, DataStore.i);
            });

            result.Register<IFriendsController>(() =>
            {
                // TODO: remove when the all the friendship responsibilities are migrated to unity
                WebInterfaceFriendsApiBridge webInterfaceFriendsApiBridge = WebInterfaceFriendsApiBridge.GetOrCreate();

                return new FriendsController(
                    RPCFriendsApiBridge.CreateSharedInstance(irpc, webInterfaceFriendsApiBridge),
                    result.Get<ISocialApiBridge>(),
                    DataStore.i, userProfileWebInterfaceBridge);
            });

            result.Register<IMessagingControllersManager>(() => new MessagingControllersManager());

            result.Register<IEmotesCatalogService>(() =>
            {
                var emotesRequest = new EmotesRequestWeb(
                    result.Get<ILambdasService>(),
                    result.Get<IServiceProviders>(),
                    featureFlagsDataStore);
                var lambdasEmotesCatalogService = new LambdasEmotesCatalogService(emotesRequest, addressableResourceProvider,
                    result.Get<IServiceProviders>().catalyst, result.Get<ILambdasService>(), DataStore.i);
                var webInterfaceEmotesCatalogService = new WebInterfaceEmotesCatalogService(EmotesCatalogBridge.GetOrCreate(), addressableResourceProvider);
                return new EmotesCatalogServiceProxy(lambdasEmotesCatalogService, webInterfaceEmotesCatalogService, featureFlagsDataStore, KernelConfig.i);
            });

            result.Register<ITeleportController>(() => new TeleportController());

            result.Register<IApplicationFocusService>(() => new ApplicationFocusService());

            result.Register<IBillboardsController>(BillboardsController.Create);

            result.Register<IWearablesCatalogService>(() => new WearablesCatalogServiceProxy(
                new LambdasWearablesCatalogService(DataStore.i.common.wearables,
                    result.Get<ILambdasService>(),
                    result.Get<IServiceProviders>(),
                    featureFlagsDataStore,
                    DataStore.i),
                WebInterfaceWearablesCatalogService.Instance,
                DataStore.i.common.wearables,
                KernelConfig.i,
                new WearablesWebInterfaceBridge(),
                featureFlagsDataStore));

            result.Register<ICustomNftCollectionService>(() => WebInterfaceCustomNftCatalogBridge.GetOrCreate());

            result.Register<IProfanityFilter>(() => new ThrottledRegexProfanityFilter(
                new ProfanityWordProviderFromResourcesJson("Profanity/badwords"), 20));

            // Asset Providers
            result.Register<ITextureAssetResolver>(() => new TextureAssetResolver(new Dictionary<AssetSource, ITextureAssetProvider>
            {
                {
                    AssetSource.EMBEDDED, new EmbeddedTextureProvider()
                },

                {
                    AssetSource.WEB, new AssetTextureWebLoader()
                },
            }, DataStore.i.featureFlags));

            result.Register<IAssetBundleResolver>(() => new AssetBundleResolver(new Dictionary<AssetSource, IAssetBundleProvider>
            {
                {
                    AssetSource.WEB, new AssetBundleWebLoader(DataStore.i.featureFlags, DataStore.i.performance)
                },
            }, new EditorAssetBundleProvider(), DataStore.i.featureFlags));

            result.Register<IFontAssetResolver>(() => new FontAssetResolver(new Dictionary<AssetSource, IFontAssetProvider>
            {
                {
                    AssetSource.EMBEDDED, new EmbeddedFontProvider()
                },

                {
                    AssetSource.ADDRESSABLE, new AddressableFontProvider(addressableResourceProvider)
                },
            }, DataStore.i.featureFlags));

            // Map
            result.Register<IHotScenesFetcher>(() => new HotScenesFetcher(60f, 60f * 5f));

            const int ATLAS_CHUNK_SIZE = 1020;
            const int PARCEL_SIZE = 20;

            // it is quite expensive to disable TextMeshPro so larger bounds should help keeping the right balance
            const float CULLING_BOUNDS_IN_PARCELS = 10;

            result.Register<IMapRenderer>(() => new MapRenderer(new MapRendererChunkComponentsFactory(PARCEL_SIZE, ATLAS_CHUNK_SIZE, CULLING_BOUNDS_IN_PARCELS)));

            // HUD
            result.Register<IHUDFactory>(() => new HUDFactory(addressableResourceProvider));
            result.Register<IHUDController>(() => new HUDController(DataStore.i));

            result.Register<IChannelsFeatureFlagService>(() =>
                new ChannelsFeatureFlagService(DataStore.i, userProfileWebInterfaceBridge));

            result.Register<IAudioDevicesService>(() => new WebBrowserAudioDevicesService(WebBrowserAudioDevicesBridge.GetOrCreate()));

            result.Register<IPlacesAPIService>(() => new PlacesAPIService(new PlacesAPIClient(webRequestController)));
            result.Register<IWorldsAPIService>(() => new WorldsAPIService(new WorldsAPIClient(webRequestController)));
            result.Register<ICameraReelStorageService>(() => new CameraReelNetworkStorageService(new CameraReelWebRequestClient(webRequestController, environmentProviderService)));

            var screencaptureCameraExternalDependencies = new ScreencaptureCameraExternalDependencies(CommonScriptableObjects.allUIHidden,
                CommonScriptableObjects.cameraModeInputLocked, DataStore.i.camera.leftMouseButtonCursorLock, CommonScriptableObjects.cameraBlocked,
                CommonScriptableObjects.featureKeyTriggersBlocked, CommonScriptableObjects.userMovementKeysBlocked, CommonScriptableObjects.isScreenshotCameraActive);

            result.Register<IScreencaptureCameraService>(() => new ScreencaptureCameraService(addressableResourceProvider, featureFlagsDataStore, DataStore.i.player, userProfileWebInterfaceBridge, screencaptureCameraExternalDependencies));

            // Analytics
            result.Register<ICameraReelAnalyticsService>(() => new CameraReelAnalyticsService(Environment.i.platform.serviceProviders.analytics));
            result.Register<IWorldsAnalytics>(() => new WorldsAnalytics(DataStore.i.common, DataStore.i.realm, Environment.i.platform.serviceProviders.analytics));
            result.Register<IDCLFileBrowserService>(DCLFileBrowserFactory.GetFileBrowserService);
            result.Register<ICopyPasteAnalyticsService>(() => new CopyPasteAnalyticsService(Environment.i.platform.serviceProviders.analytics, new UserProfileWebInterfaceBridge()));
            result.Register<IPortableExperiencesAnalyticsService>(() => new PortableExperiencesAnalyticsService(Environment.i.platform.serviceProviders.analytics, new UserProfileWebInterfaceBridge()));
            return result;
        }
    }
}
