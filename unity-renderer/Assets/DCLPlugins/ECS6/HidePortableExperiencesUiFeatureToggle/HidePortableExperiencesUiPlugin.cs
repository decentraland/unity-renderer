using DCL;

namespace DCLPlugins.ECS6.HidePortableExperiencesUiFeatureToggle
{
    public class HidePortableExperiencesUiPlugin : IPlugin
    {
        private readonly HidePortableExperiencesUiController controller;

        public HidePortableExperiencesUiPlugin()
        {
            controller = new HidePortableExperiencesUiController(
                Environment.i.world.state,
                DataStore.i.HUDs.isSceneUiEnabled,
                DataStore.i.world.portableExperienceIds);
        }

        public void Dispose()
        {
            controller.Dispose();
        }
    }
}
