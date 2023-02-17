using DCL.Emotes;
using DCL.EmotesWheel;
using DCL.EquippedEmotes;
using DCL.ExperiencesViewer;
using DCL.Tutorial;
using DCL.Skybox;
using DCL.LogReport;

namespace DCL
{
    public static class PluginSystemFactoryDesktop
    {
        public static PluginSystem Create()
        {
            var pluginSystem = PluginSystemFactory.Create();

            pluginSystem.Unregister<TextureCompressionTogglePlugin>();
            pluginSystem.Register<ExploreV2Feature>(() => new ExploreV2FeatureDesktop());
            pluginSystem.Register<LogReportPlugin>(() => new LogReportPlugin());

            pluginSystem.SetFeatureFlagsData(DataStore.i.featureFlags.flags);

            return pluginSystem;
        }
    }
}
