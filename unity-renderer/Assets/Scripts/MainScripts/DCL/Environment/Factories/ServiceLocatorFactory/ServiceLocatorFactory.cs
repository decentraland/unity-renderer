using AvatarSystem;
using DCL.Chat;
using DCL.Chat.Channels;
using DCL.Controllers;
using DCL.Emotes;
using DCL.ProfanityFiltering;
using DCL.Providers;
using DCL.Rendering;
using DCL.Services;
using DCL.Social.Chat;
using DCLServices.Lambdas;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using DCLServices.WearablesCatalogService;
using MainScripts.DCL.Controllers.AssetManager;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using MainScripts.DCL.Helpers.SentryUtils;
using System.Collections.Generic;
using UnityEngine;
using WorldsFeaturesAnalytics;

namespace DCL
{
    public static class ServiceLocatorFactory
    {
        public static ServiceLocator CreateDefault()
        {
            var result = new ServiceLocator();

            //Addressable Resource Provider
            var addressableResourceProvider = new AddressableResourceProvider();
            result.Register<IAddressableResourceProvider>(() => addressableResourceProvider);

            // Platform
            result.Register<IMemoryManager>(() => new MemoryManager());
            result.Register<ICullingController>(CullingController.Create);
            result.Register<IParcelScenesCleaner>(() => new ParcelScenesCleaner());
            result.Register<IClipboard>(Clipboard.Create);
            result.Register<IPhysicsSyncController>(() => new PhysicsSyncController());
            result.Register<IWebRequestController>(WebRequestController.Create);
            result.Register<IServiceProviders>(() => new ServiceProviders());
            result.Register<ILambdasService>(() => new LambdasService());
            result.Register<INamesService>(() => new NamesService());
            result.Register<ILandsService>(() => new LandsService());
            result.Register<IUpdateEventHandler>(() => new UpdateEventHandler());
            result.Register<IRPC>(() => new RPC());
            result.Register<IWebRequestMonitor>(() => new SentryWebRequestMonitor());
            result.Register<IWearablesCatalogService>(() => new WearablesCatalogServiceProxy(
                new LambdasWearablesCatalogService(DataStore.i.common.wearables),
                WebInterfaceWearablesCatalogService.Instance,
                DataStore.i.common.wearables,
                KernelConfig.i,
                new WearablesWebInterfaceBridge()));

            // World runtime
            result.Register<IIdleChecker>(() => new IdleChecker());
            result.Register<IAvatarsLODController>(() => new AvatarsLODController());
            result.Register<IFeatureFlagController>(() => new FeatureFlagController());
            result.Register<ISceneController>(() => new SceneController());
            result.Register<IWorldState>(() => new WorldState());
            result.Register<ISceneBoundsChecker>(() => new SceneBoundsChecker());
            result.Register<IWorldBlockersController>(() => new WorldBlockersController());
            result.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
            result.Register<IAvatarFactory>(() => new AvatarFactory(result));
            result.Register<ICharacterPreviewFactory>(() => new CharacterPreviewFactory());
            result.Register<IChatController>(() => new ChatController(WebInterfaceChatBridge.GetOrCreate(), DataStore.i));
            result.Register<IMessagingControllersManager>(() => new MessagingControllersManager());
            result.Register<IEmotesCatalogService>(() => new EmotesCatalogService(EmotesCatalogBridge.GetOrCreate(), addressableResourceProvider));
            result.Register<ITeleportController>(() => new TeleportController());
            result.Register<IApplicationFocusService>(() => new ApplicationFocusService());
            result.Register<IBillboardsController>(BillboardsController.Create);

            result.Register<IProfanityFilter>(() => new ThrottledRegexProfanityFilter(
                new ProfanityWordProviderFromResourcesJson("Profanity/badwords"), 20));

            // Asset Providers
            result.Register<ITextureAssetResolver>(() => new TextureAssetResolver(new Dictionary<AssetSource, ITextureAssetProvider>
            {
                { AssetSource.EMBEDDED, new EmbeddedTextureProvider() },
                { AssetSource.WEB, new AssetTextureWebLoader() },
            }, DataStore.i.featureFlags));

            result.Register<IAssetBundleResolver>(() => new AssetBundleResolver(new Dictionary<AssetSource, IAssetBundleProvider>
            {
                { AssetSource.WEB, new AssetBundleWebLoader(DataStore.i.featureFlags, DataStore.i.performance) },
            }, new EditorAssetBundleProvider(), DataStore.i.featureFlags));

            result.Register<IFontAssetResolver>(() => new FontAssetResolver(new Dictionary<AssetSource, IFontAssetProvider>
            {
                { AssetSource.EMBEDDED, new EmbeddedFontProvider() },
                { AssetSource.ADDRESSABLE, new AddressableFontProvider(addressableResourceProvider) },
            }, DataStore.i.featureFlags));

            // HUD
            result.Register<IHUDFactory>(() => new HUDFactory());
            result.Register<IHUDController>(() => new HUDController(DataStore.i.featureFlags));

            result.Register<IChannelsFeatureFlagService>(() =>
                new ChannelsFeatureFlagService(DataStore.i, new UserProfileWebInterfaceBridge()));

            result.Register<IAudioDevicesService>(() => new WebBrowserAudioDevicesService(WebBrowserAudioDevicesBridge.GetOrCreate()));

            // Analytics

            result.Register<IWorldsAnalytics>(() => new WorldsAnalytics(DataStore.i.common, DataStore.i.realm, Environment.i.platform.serviceProviders.analytics));

            return result;
        }
    }
}
