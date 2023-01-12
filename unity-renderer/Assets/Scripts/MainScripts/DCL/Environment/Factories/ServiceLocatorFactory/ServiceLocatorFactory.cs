using AvatarSystem;
using DCL.Chat;
using DCL.Chat.Channels;
using DCL.Controllers;
using DCL.Emotes;
using DCL.ProfanityFiltering;
using DCL.Rendering;
using DCL.Services;
using DCLServices.Lambdas;
using DCLServices.Lambdas.LandsService;
using DCLServices.Lambdas.NamesService;
using MainScripts.DCL.Controllers.HUD.CharacterPreview;
using UnityEngine;
using WorldsFeaturesAnalytics;

namespace DCL
{
    public static class ServiceLocatorFactory
    {
        public static ServiceLocator CreateDefault()
        {
            var result = new ServiceLocator();

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

            result.Register<IMessagingControllersManager>(() => new MessagingControllersManager());
            result.Register<IEmotesCatalogService>(() => new EmotesCatalogService(EmotesCatalogBridge.GetOrCreate(), Resources.Load<EmbeddedEmotesSO>("EmbeddedEmotes").emotes));
            result.Register<ITeleportController>(() => new TeleportController());
            result.Register<IApplicationFocusService>(() => new ApplicationFocusService());
            result.Register<IBillboardsController>(BillboardsController.Create);

            result.Register<IProfanityFilter>(() => new ThrottledRegexProfanityFilter(
                // Check https://github.com/decentraland/unity-renderer/issues/2201 for more info about partitionSize
                new ProfanityWordProviderFromResourcesJson("Profanity/badwords"), 20));

            // HUD
            result.Register<IHUDFactory>(() => new HUDFactory());
            result.Register<IHUDController>(() => new HUDController());
            result.Register<IChannelsFeatureFlagService>(() =>
                new ChannelsFeatureFlagService(DataStore.i, new UserProfileWebInterfaceBridge()));

            result.Register<IAudioDevicesService>(() => new WebBrowserAudioDevicesService(WebBrowserAudioDevicesBridge.GetOrCreate()));

            // Analytics

            result.Register<IWorldsAnalytics>(() => new WorldsAnalytics(DataStore.i.common, DataStore.i.realm, Environment.i.platform.serviceProviders.analytics));

            return result;
        }
    }
}
