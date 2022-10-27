using DCL.Chat;
using DCL.Chat.Channels;
using DCL.Controllers;
using DCL.Emotes;
using DCL.Rendering;
using DCL.Services;
using UnityEngine;

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
            result.Register<IUpdateEventHandler>(() => new UpdateEventHandler());

            // World runtime
            result.Register<IIdleChecker>(() => new IdleChecker());
            result.Register<IAvatarsLODController>(() => new AvatarsLODController());
            result.Register<IFeatureFlagController>(() => new FeatureFlagController());
            result.Register<ISceneController>(() => new SceneController());
            result.Register<IWorldState>(() => new WorldState());
            result.Register<ISceneBoundsChecker>(() => new SceneBoundsChecker());
            result.Register<IWorldBlockersController>(() => new WorldBlockersController());
            result.Register<IRuntimeComponentFactory>(() => new RuntimeComponentFactory());

            result.Register<IMessagingControllersManager>(() => new MessagingControllersManager());
            result.Register<IEmotesCatalogService>(() => new EmotesCatalogService(EmotesCatalogBridge.GetOrCreate(), Resources.Load<EmbeddedEmotesSO>("EmbeddedEmotes").emotes));
            result.Register<ITeleportController>(() => new TeleportController());
            result.Register<IApplicationFocusService>(() => new ApplicationFocusService());

            // HUD
            result.Register<IHUDFactory>(() => new HUDFactory());
            result.Register<IHUDController>(() => new HUDController());
            result.Register<IChannelsFeatureFlagService>(() =>
                new ChannelsFeatureFlagService(DataStore.i, new UserProfileWebInterfaceBridge()));
            
            result.Register<IAudioDevicesService>(() => new WebBrowserAudioDevicesService(WebBrowserAudioDevicesBridge.GetOrCreate()));

            return result;
        }
    }
}