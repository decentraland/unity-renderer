using AvatarSystem;
using DCL.Emotes;
using DCL.EmotesCustomization;
using DCL.EmotesWheel;
using DCL.EquippedEmotes;
using DCL.ExperiencesViewer;
using DCL.Skybox;
using DCL.Tutorial;

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
            pluginSystem.Register(() => new SkyboxController());
            pluginSystem.Register(() => new GotoPanelPlugin());
            pluginSystem.Register(() => new ExperiencesViewerFeature());
            pluginSystem.Register(() => new EmoteAnimationsPlugin(DataStore.i.emotes, new EmoteAnimationLoaderFactory(), new WearableItemResolver()));
            pluginSystem.Register(() => new EquippedEmotesPlugin());
            pluginSystem.Register(() => new EmotesWheelUIPlugin());
            pluginSystem.RegisterWithFlag(() => new EmotesCustomizationUIPlugin(), "emotes_customization");
            pluginSystem.RegisterWithFlag(() => new BuilderInWorldPlugin(), "builder_in_world");
            pluginSystem.RegisterWithFlag(() => new TutorialController(), "tutorial");
            pluginSystem.RegisterWithFlag(() => new PlacesAndEventsFeature(), "explorev2");
            pluginSystem.RegisterWithFlag(() => new SkyboxController(), "procedural_skybox");

            pluginSystem.SetFeatureFlagsData(DataStore.i.featureFlags.flags);

            return pluginSystem;
        }
    }
}
