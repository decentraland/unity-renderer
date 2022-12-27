using AvatarSystem;
using DCL.Chat;
using DCL.Chat.Channels;
using DCL.Controllers;
using DCL.Emotes;
using DCL.Rendering;
using DCL.Services;
using DCLServices.Lambdas;
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
            result.Register<IUpdateEventHandler>(() => new UpdateEventHandler());
            result.Register<IRPC>(() => new RPC());

            // World runtime
            result.Register<IIdleChecker>(() => new IdleChecker());
            result.Register<IAvatarsLODController>(() => new AvatarsLODController());
            result.Register<IFeatureFlagController>(() => new FeatureFlagController());
            result.Register<ISceneController>(() => new SceneController());
            result.Register<IWorldState>(() => new WorldState());
            result.Register<IMessagingControllersManager>(() => new MessagingControllersManager());
            result.Register<ISceneBoundsChecker>(() => new SceneBoundsChecker());
            result.Register<IWorldBlockersController>(() => new WorldBlockersController());
            result.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());
            result.Register<IAvatarFactory>(() => new AvatarFactory(result));
            result.Register<ICharacterPreviewFactory>(() => new CharacterPreviewFactory());

            result.Register<IEmotesCatalogService>(() => new EmotesCatalogService(EmotesCatalogBridge.GetOrCreate(), Resources.Load<EmbeddedEmotesSO>("EmbeddedEmotes").emotes));
            result.Register<ITeleportController>(() => new TeleportController());
            result.Register<IApplicationFocusService>(() => new ApplicationFocusService());
            result.Register<IBillboardsController>(BillboardsController.Create);

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
