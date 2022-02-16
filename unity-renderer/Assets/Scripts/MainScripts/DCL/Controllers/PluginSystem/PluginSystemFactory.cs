using DCL.Tutorial;
using DCL.Skybox;
using DCL.ExperiencesViewer;

namespace DCL
{
    public static class PluginSystemFactory
    {
        public static PluginSystem Create()
        {
            var pluginSystem = new PluginSystem();

            pluginSystem.Register(() => new DebugPluginFeature());
            pluginSystem.Register(() => new ShortcutsFeature());
            pluginSystem.Register(() => new ExploreV2Feature());
            pluginSystem.Register(() => new DebugShapesBoundingBoxDisplayer());
            pluginSystem.Register(() => new TransactionFeature());
            pluginSystem.Register(() => new PreviewMenuPlugin());
            pluginSystem.RegisterWithFlag(() => new BuilderInWorldPlugin(), "builder_in_world");
            pluginSystem.RegisterWithFlag(() => new TutorialController(), "tutorial");
            pluginSystem.RegisterWithFlag(() => new PlacesAndEventsFeature(), "explorev2");
            pluginSystem.RegisterWithFlag(() => new SkyboxController(), "procedural_skybox");
            pluginSystem.RegisterWithFlag(() => new ExperiencesViewerFeature(), "portable_experiences");

            pluginSystem.SetFeatureFlagsData(DataStore.i.featureFlags.flags);

            return pluginSystem;
        }
    }
}
