using System;
using DCL.Builder;

internal class SectionsHandler : IDisposable
{
    private readonly ISectionsController sectionsController;
    private readonly IPlacesViewController placesViewController;
    private readonly SearchBarView searchBarView;
    private readonly ILandsController landsController;
    private readonly IProjectsController projectsController;

    public SectionsHandler(ISectionsController sectionsController, IPlacesViewController placesViewController, ILandsController landsController, IProjectsController projectsController, SearchBarView searchBarView)
    {
        this.sectionsController = sectionsController;
        this.placesViewController = placesViewController;
        this.searchBarView = searchBarView;
        this.landsController = landsController;
        this.projectsController = projectsController;

        sectionsController.OnSectionShow += OnSectionShow;
        sectionsController.OnSectionHide += OnSectionHide;
        placesViewController.OnProjectSelected += SelectProject;
    }

    public void Dispose()
    {
        sectionsController.OnSectionShow -= OnSectionShow;
        sectionsController.OnSectionHide -= OnSectionHide;
        placesViewController.OnProjectSelected -= SelectProject;
    }

    void OnSectionShow(SectionBase sectionBase)
    {
        if (sectionBase is IPlaceListener deployedSceneListener)
        {
            placesViewController.AddListener(deployedSceneListener);
        }

        if (sectionBase is IProjectListener projectSceneListener)
        {
            placesViewController.AddListener(projectSceneListener);
        }

        if (sectionBase is ISelectPlaceListener selectSceneListener)
        {
            placesViewController.AddListener(selectSceneListener);
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
        if (sectionBase is IPlaceListener deployedSceneListener)
        {
            placesViewController.RemoveListener(deployedSceneListener);
        }

        if (sectionBase is IProjectListener projectSceneListener)
        {
            placesViewController.RemoveListener(projectSceneListener);
        }

        if (sectionBase is ISelectPlaceListener selectSceneListener)
        {
            placesViewController.RemoveListener(selectSceneListener);
        }

        if (sectionBase is ILandsListener landsListener)
        {
            landsController.RemoveListener(landsListener);
        }

        searchBarView.SetSearchBar(null, null);
    }

    void SelectProject(IPlaceCardView placeCardView) { sectionsController.OpenSection(SectionId.SETTINGS_PROJECT_GENERAL); }
}