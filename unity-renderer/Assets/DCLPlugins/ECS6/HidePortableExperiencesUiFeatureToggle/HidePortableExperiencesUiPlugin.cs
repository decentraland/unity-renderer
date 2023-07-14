using DCL;

namespace DCLPlugins.ECS6.HidePortableExperiencesUiFeatureToggle
{
    public class HidePortableExperiencesUiPlugin : IPlugin
    {
        private readonly HidePortableExperiencesUiController controller;

        public HidePortableExperiencesUiPlugin()
        {
            controller = new HidePortableExperiencesUiController(CommonScriptableObjects.sceneNumber,
                Environment.i.world.state,
                DataStore.i.world.portableExperienceIds,
                DataStore.i.HUDs.isSceneUiEnabled);
        }

        public void Dispose()
        {
            controller.Dispose();
        }
    }
}
