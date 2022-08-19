using DCL.ECS7;
using DCL.Emotes;
using DCL.EmotesWheel;
using DCL.EquippedEmotes;
using DCL.ExperiencesViewer;
using DCL.Helpers;
using DCL.Skybox;
using DCL.Tutorial;
using DCLPlugins.UIRefresherPlugin;

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
            pluginSystem.Register<AvatarModifierAreaFeedbackPlugin>(() => new AvatarModifierAreaFeedbackPlugin());
            pluginSystem.Register<SpawnPointsDisplayerPlugin>(() => new SpawnPointsDisplayerPlugin());
            pluginSystem.Register<UIRefresherPlugin>(() => new UIRefresherPlugin());
            pluginSystem.RegisterWithFlag<BuilderInWorldPlugin>(() => new BuilderInWorldPlugin(), "builder_in_world");
            pluginSystem.RegisterWithFlag<TutorialController>(() => new TutorialController(), "tutorial");
            pluginSystem.RegisterWithFlag<TextureCompressionTogglePlugin>(() => new TextureCompressionTogglePlugin(), "perf_tex_compression");
            pluginSystem.RegisterWithFlag<ECS7Plugin>(() => new ECS7Plugin(), "ecs7");
            pluginSystem.RegisterWithFlag<ChatNotificationsFeature>(() => new ChatNotificationsFeature(), "notification_chat");
            pluginSystem.Register<FriendsNotificationPlugin>(() => new FriendsNotificationPlugin(new DefaultPlayerPrefs(),
                FriendsController.i,
                NotificationScriptableObjects.pendingFriendRequests,
                NotificationScriptableObjects.newApprovedFriends,
                DataStore.i));

            pluginSystem.SetFeatureFlagsData(DataStore.i.featureFlags.flags);

            return pluginSystem;
        }
    }
}
