using DCL.Tutorial;

namespace DCL
{
    public static class PluginSystemFactory
    {
        public static PluginSystem Create()
        {
            var pluginSystem = new PluginSystem();

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