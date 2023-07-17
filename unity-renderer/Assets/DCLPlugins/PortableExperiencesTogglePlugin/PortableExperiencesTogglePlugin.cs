namespace DCL.PortableExperiencesToggle
{
    public class PortableExperiencesTogglePlugin : IPlugin
    {
        private readonly PortableExperiencesToggleController controller;

        public PortableExperiencesTogglePlugin()
        {
            controller = new PortableExperiencesToggleController(CommonScriptableObjects.sceneNumber,
                Environment.i.world.state,
                new WebInterfacePortableExperiencesBridge(),
                DataStore.i.world.portableExperienceIds,
                DataStore.i.world.disabledPortableExperienceIds,
                DataStore.i.HUDs.isSceneUiEnabled);
        }

        public void Dispose()
        {
            controller.Dispose();
        }
    }
}
