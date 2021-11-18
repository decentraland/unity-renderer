using DCL.Tutorial;

namespace DCL
{
    public static class PluginSystemFactory
    {
        public static PluginSystemV2 Create()
        {
            var pluginSystem = new PluginSystemV2();

            pluginSystem.Register(new DebugPluginFeature());
            pluginSystem.Register(new ShortcutsFeature());
            pluginSystem.RegisterWithFlag(new BuilderInWorldPlugin(), "builder_in_world");
            pluginSystem.RegisterWithFlag(new TutorialController(), "tutorial");
            pluginSystem.RegisterWithFlag(new ExploreV2Feature(), "explorev2");

            pluginSystem.SetFeatureFlagsData(DataStore.i.featureFlags.flags);

            return pluginSystem;
        }
    }
}