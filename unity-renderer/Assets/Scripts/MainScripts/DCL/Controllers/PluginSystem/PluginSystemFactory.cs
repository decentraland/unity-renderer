using AvatarSystem;
using DCL.Emotes;
using DCL.Tutorial;
using DCL.Skybox;
using EmotesCustomization;
using DCL.ExperiencesViewer;

namespace DCL
{
    public static class PluginSystemFactory
    {
        public static PluginSystem Create()
        {
            var pluginSystem = new PluginSystem();

            pluginSystem.Register<DebugPluginFeature>(() => new DebugPluginFeature());
            pluginSystem.Register<ShortcutsFeature>(() => new ShortcutsFeature());
            pluginSystem.Register<ExploreV2Feature>(() => new ExploreV2Feature());
            pluginSystem.Register<DebugShapesBoundingBoxDisplayer>(() => new DebugShapesBoundingBoxDisplayer());
            pluginSystem.Register<TransactionFeature>(() => new TransactionFeature());
            pluginSystem.Register<PreviewMenuPlugin>(() => new PreviewMenuPlugin());
            pluginSystem.Register<SkyboxController>(() => new SkyboxController());
            pluginSystem.Register<GotoPanelPlugin>(() => new GotoPanelPlugin());
            pluginSystem.Register<ExperiencesViewerFeature>(() => new ExperiencesViewerFeature());
            pluginSystem.Register<EmoteAnimationsPlugin>(() => new EmoteAnimationsPlugin(DataStore.i.emotes, new EmoteAnimationLoaderFactory(), new WearableItemResolver()));
            pluginSystem.RegisterWithFlag<BuilderInWorldPlugin>(() => new BuilderInWorldPlugin(), "builder_in_world");
            pluginSystem.RegisterWithFlag<TutorialController>(() => new TutorialController(), "tutorial");
            pluginSystem.RegisterWithFlag<PlacesAndEventsFeature>(() => new PlacesAndEventsFeature(), "explorev2");
            pluginSystem.RegisterWithFlag<SkyboxController>(() => new SkyboxController(), "procedural_skybox");
            pluginSystem.RegisterWithFlag<EmotesCustomizationFeature>(() => new EmotesCustomizationFeature(), "emotes_customization");
            pluginSystem.Register<EmotesWheelFeature>(() => new EmotesWheelFeature());

            pluginSystem.SetFeatureFlagsData(DataStore.i.featureFlags.flags);

            return pluginSystem;
        }
    }
}
