using DCL.Chat.HUD;
using DCL.Chat.Notifications;
using DCL.ECS7;
using DCL.Emotes;
using DCL.EmotesWheel;
using DCL.EquippedEmotes;
using DCL.ExperiencesViewer;
using DCL.Guests.HUD.ConnectWallet;
using DCL.Helpers;
using DCL.Skybox;
using DCL.Social.Friends;
using DCL.Tutorial;
using DCLPlugins.RealmPlugin;
using DCLPlugins.UIRefresherPlugin;
using WorldsFeaturesAnalytics;

namespace DCL
{
    public static class PluginSystemFactory
    {
        public static PluginSystem Create()
        {
            var pluginSystem = new PluginSystem();

            // Ideally the Plugin class itself should be a really small entry point with a parameterless constructor
            // the heavy lifting should be done by another class (following the Humble Object Pattern)

            pluginSystem.Register<DebugPluginFeature>(() => new DebugPluginFeature());
            pluginSystem.Register<ShortcutsFeature>(() => new ShortcutsFeature());
            pluginSystem.Register<ExploreV2Feature>(() => new ExploreV2Feature());
            pluginSystem.Register<DebugShapesBoundingBoxDisplayer>(() => new DebugShapesBoundingBoxDisplayer());
            pluginSystem.Register<TransactionFeature>(() => new TransactionFeature());
            pluginSystem.Register<PreviewMenuPlugin>(() => new PreviewMenuPlugin());
            pluginSystem.Register<SkyboxController>(() => new SkyboxController());
            pluginSystem.Register<GotoPanelPlugin>(() => new GotoPanelPlugin());
            pluginSystem.Register<ExperiencesViewerFeature>(() => new ExperiencesViewerFeature());
            pluginSystem.Register<EmoteAnimationsPlugin>(() => new EmoteAnimationsPlugin());
            pluginSystem.Register<EquippedEmotesInitializerPlugin>(() => new EquippedEmotesInitializerPlugin());
            pluginSystem.Register<EmotesWheelUIPlugin>(() => new EmotesWheelUIPlugin());
            pluginSystem.Register<NFTShapePlugin>(() => new NFTShapePlugin());
            pluginSystem.Register<UUIDEventsPlugin>(() => new UUIDEventsPlugin());
            pluginSystem.Register<UIComponentsPlugin>(() => new UIComponentsPlugin());
            pluginSystem.Register<CoreComponentsPlugin>(() => new CoreComponentsPlugin());
            pluginSystem.Register<PlacesAndEventsFeature>(() => new PlacesAndEventsFeature());
            pluginSystem.Register<JoinChannelModalPlugin>(() => new JoinChannelModalPlugin());
            pluginSystem.Register<ChannelLimitReachedWindowPlugin>(() => new ChannelLimitReachedWindowPlugin());
            pluginSystem.Register<ChannelJoinErrorWindowPlugin>(() => new ChannelJoinErrorWindowPlugin());
            pluginSystem.Register<ChannelLeaveErrorWindowPlugin>(() => new ChannelLeaveErrorWindowPlugin());
            pluginSystem.Register<AvatarModifierAreaFeedbackPlugin>(() => new AvatarModifierAreaFeedbackPlugin());
            pluginSystem.Register<SpawnPointsDisplayerPlugin>(() => new SpawnPointsDisplayerPlugin());
            pluginSystem.Register<UIRefresherPlugin>(() => new UIRefresherPlugin());
            pluginSystem.Register<ChatNotificationsFeature>(() => new ChatNotificationsFeature());
            pluginSystem.Register<ConnectWalletModalPlugin>(() => new ConnectWalletModalPlugin());
            pluginSystem.Register<SendFriendRequestHUDPlugin>(() => new SendFriendRequestHUDPlugin());
            pluginSystem.Register<CancelFriendRequestHUDPlugin>(() => new CancelFriendRequestHUDPlugin());

            pluginSystem.RegisterWithFlag<RealmPlugin>(() => new RealmPlugin(DataStore.i, new WorldsAnalytics(DataStore.i.common, Environment.i.platform.serviceProviders.analytics, Environment.i.platform.updateEventHandler)), "realms_modifier_plugin");
            pluginSystem.RegisterWithFlag<TutorialController>(() => new TutorialController(DataStore.i.common, DataStore.i.settings, DataStore.i.exploreV2), "tutorial");
            pluginSystem.RegisterWithFlag<TextureCompressionTogglePlugin>(() => new TextureCompressionTogglePlugin(), "perf_tex_compression");
            pluginSystem.RegisterWithFlag<ECS7Plugin>(() => new ECS7Plugin(), "ecs7");
            pluginSystem.RegisterWithFlag<BlurFeature>(() => new BlurFeature(), "ui_blur_variant:enabled");
            pluginSystem.RegisterWithFlag<PromoteChannelsToastPlugin>(() => new PromoteChannelsToastPlugin(), "promote_channels_toast");
            pluginSystem.RegisterWithFlag<PlayerPassportPlugin>(() => new PlayerPassportPlugin(), "new_avatar_flow");

            pluginSystem.Register<FriendsNotificationPlugin>(() => new FriendsNotificationPlugin(new DefaultPlayerPrefs(),
                FriendsController.i,
                NotificationScriptableObjects.pendingFriendRequests,
                NotificationScriptableObjects.newApprovedFriends,
                DataStore.i));

            pluginSystem.Register<ABDetectorPlugin>(() => new ABDetectorPlugin());

            pluginSystem.Register<MapTexturePlugin>(() => new MapTexturePlugin());

            pluginSystem.SetFeatureFlagsData(DataStore.i.featureFlags.flags);

            return pluginSystem;
        }
    }
}
