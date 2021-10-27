using System;

internal class SectionsHandler : IDisposable
{
    private readonly ISectionsController sectionsController;
    private readonly IScenesViewController scenesViewController;
    private readonly SearchBarView searchBarView;
    private readonly ILandController landController;
    private readonly IProjectsController projectsController;

    public SectionsHandler(ISectionsController sectionsController, IScenesViewController scenesViewController, ILandController landController, IProjectsController projectsController, SearchBarView searchBarView)
    {
        this.sectionsController = sectionsController;
        this.scenesViewController = scenesViewController;
        this.searchBarView = searchBarView;
        this.landController = landController;
        this.projectsController = projectsController;

        sectionsController.OnSectionShow += OnSectionShow;
        sectionsController.OnSectionHide += OnSectionHide;
        scenesViewController.OnSceneSelected += OnSelectScene;
    }

    public void Dispose()
    {
        sectionsController.OnSectionShow -= OnSectionShow;
        sectionsController.OnSectionHide -= OnSectionHide;
        scenesViewController.OnSceneSelected -= OnSelectScene;
    }

    void OnSectionShow(SectionBase sectionBase)
    {
        if (sectionBase is IDeployedSceneListener deployedSceneListener)
        {
            scenesViewController.AddListener(deployedSceneListener);
        }

        if (sectionBase is IScenesListener projectSceneListener)
        {
            scenesViewController.AddListener(projectSceneListener);
        }

        if (sectionBase is ISelectSceneListener selectSceneListener)
        {
            scenesViewController.AddListener(selectSceneListener);
        }

        if (sectionBase is ILandsListener landsListener)
        {
            landController.AddListener(landsListener);
        }

        if (sectionBase is IProjectsListener projectsListener)
        {
            projectsController.AddListener(projectsListener);
        }

        searchBarView.SetSearchBar(sectionBase.searchHandler, sectionBase.searchBarConfig);
    }

    void OnSectionHide(SectionBase sectionBase)
    {
        if (sectionBase is IDeployedSceneListener deployedSceneListener)
        {
            scenesViewController.RemoveListener(deployedSceneListener);
        }

        if (sectionBase is IScenesListener projectSceneListener)
        {
            scenesViewController.RemoveListener(projectSceneListener);
        }

        if (sectionBase is ISelectSceneListener selectSceneListener)
        {
            scenesViewController.RemoveListener(selectSceneListener);
        }

        if (sectionBase is ILandsListener landsListener)
        {
            landController.RemoveListener(landsListener);
        }

        searchBarView.SetSearchBar(null, null);
    }

    void OnSelectScene(ISceneCardView sceneCardView) { sectionsController.OpenSection(SectionId.SETTINGS_PROJECT_GENERAL); }
}