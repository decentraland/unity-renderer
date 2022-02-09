namespace DCL.ExperiencesViewer
{
    /// <summary>
    /// Plugin feature that initialize the Experiences Viewer feature.
    /// </summary>
    public class ExperiencesViewerFeature : IPlugin
    {
        public IExperiencesViewerComponentController experiencesViewerComponentController;

        public ExperiencesViewerFeature()
        {
            experiencesViewerComponentController = CreateController();
            experiencesViewerComponentController.Initialize(Environment.i.world.sceneController);
        }

        internal virtual IExperiencesViewerComponentController CreateController() => new ExperiencesViewerComponentController();

        public void Dispose()
        {
            experiencesViewerComponentController.Dispose();
        }
    }
}