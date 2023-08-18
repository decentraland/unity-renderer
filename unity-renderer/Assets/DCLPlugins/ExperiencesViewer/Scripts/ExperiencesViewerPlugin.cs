namespace DCL.ExperiencesViewer
{
    /// <summary>
    /// Plugin feature that initialize the Experiences Viewer feature.
    /// </summary>
    public class ExperiencesViewerPlugin : IPlugin
    {
        private readonly ExperiencesViewerController controller;

        public ExperiencesViewerPlugin()
        {
            controller = new ExperiencesViewerController(ExperiencesViewerComponentView.Create(),
                DataStore.i, Environment.i.world.state, new WebInterfacePortableExperiencesBridge());
        }

        public void Dispose()
        {
            controller.Dispose();
        }
    }
}
