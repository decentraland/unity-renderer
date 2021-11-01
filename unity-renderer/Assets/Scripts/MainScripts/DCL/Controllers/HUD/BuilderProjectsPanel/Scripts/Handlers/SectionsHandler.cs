using System;
using DCL.Builder;

internal class SectionsHandler : IDisposable
{
    private readonly ISectionsController sectionsController;
    private readonly IScenesViewController scenesViewController;
    private readonly SearchBarView searchBarView;
    private readonly ILandsController landsController;
    private readonly IProjectsController projectsController;

    public SectionsHandler(ISectionsController sectionsController, IScenesViewController scenesViewController, ILandsController landsController, IProjectsController projectsController, SearchBarView searchBarView)
    {
        this.sectionsController = sectionsController;
        this.scenesViewController = scenesViewController;
        this.searchBarView = searchBarView;
        this.landsController = landsController;
        this.projectsController = projectsController;

        sectionsController.OnSectionShow += OnSectionShow;
        sectionsController.OnSectionHide += OnSectionHide;
        scenesViewController.OnProjectSelected += SelectProject;
    }

    public void Dispose()
    {
        sectionsController.OnSectionShow -= OnSectionShow;
        sectionsController.OnSectionHide -= OnSectionHide;
        scenesViewController.OnProjectSelected -= SelectProject;
    }

    void OnSectionShow(SectionBase sectionBase)
    {
        if (sectionBase is ISceneListener deployedSceneListener)
        {
            scenesViewController.AddListener(deployedSceneListener);
        }

        if (sectionBase is IProjectListener projectSceneListener)
        {
            scenesViewController.AddListener(projectSceneListener);
        }

        if (sectionBase is ISelectSceneListener selectSceneListener)
        {
            scenesViewController.AddListener(selectSceneListener);
        }

        if (sectionBase is ILandsListener landsListener)
        {
            landsController.AddListener(landsListener);
        }

        if (sectionBase is IProjectsListener projectsListener)
        {
            projectsController.AddListener(projectsListener);
        }

        searchBarView.SetSearchBar(sectionBase.searchHandler, sectionBase.searchBarConfig);
    }

    void OnSectionHide(SectionBase sectionBase)
    {
        if (sectionBase is ISceneListener deployedSceneListener)
        {
            scenesViewController.RemoveListener(deployedSceneListener);
        }

        if (sectionBase is IProjectListener projectSceneListener)
        {
            scenesViewController.RemoveListener(projectSceneListener);
        }

        if (sectionBase is ISelectSceneListener selectSceneListener)
        {
            scenesViewController.RemoveListener(selectSceneListener);
        }

        if (sectionBase is ILandsListener landsListener)
        {
            landsController.RemoveListener(landsListener);
        }

        searchBarView.SetSearchBar(null, null);
    }

    void SelectProject(ISceneCardView sceneCardView) { sectionsController.OpenSection(SectionId.SETTINGS_PROJECT_GENERAL); }
}