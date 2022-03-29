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

            // Ideally the Plugin class itself should be a really small entry point with a parameterless constructor
            // the heavy lifting should be done by another class (following the Humble Object Pattern)

            pluginSystem.Register(() => new DebugPluginFeature());
            pluginSystem.Register(() => new ShortcutsFeature());
            pluginSystem.Register(() => new ExploreV2Feature());
            pluginSystem.Register(() => new DebugShapesBoundingBoxDisplayer());
            pluginSystem.Register(() => new TransactionFeature());
            pluginSystem.Register(() => new PreviewMenuPlugin());
            pluginSystem.Register(() => new SkyboxController());
            pluginSystem.Register(() => new GotoPanelPlugin());
            pluginSystem.Register(() => new ExperiencesViewerFeature());
            pluginSystem.Register(() => new EmoteAnimationsPlugin());
            pluginSystem.RegisterWithFlag(() => new BuilderInWorldPlugin(), "builder_in_world");
            pluginSystem.RegisterWithFlag(() => new TutorialController(), "tutorial");
            pluginSystem.RegisterWithFlag(() => new PlacesAndEventsFeature(), "explorev2");
            pluginSystem.RegisterWithFlag(() => new SkyboxController(), "procedural_skybox");
            pluginSystem.RegisterWithFlag(() => new EmotesCustomizationFeature(), "emotes_customization");
            pluginSystem.Register(() => new EmotesWheelFeature());

            pluginSystem.SetFeatureFlagsData(DataStore.i.featureFlags.flags);

            return pluginSystem;
        }
    }
}
