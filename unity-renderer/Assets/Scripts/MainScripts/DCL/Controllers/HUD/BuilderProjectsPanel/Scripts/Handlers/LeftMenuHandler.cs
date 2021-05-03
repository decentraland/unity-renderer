using System;

internal class LeftMenuHandler : IDisposable
{
    private readonly IBuilderProjectsPanelView view;
    private readonly ISectionsController sectionsController;

    private bool isMainPanel = false;
    private SectionId lastMainSectionId;

    public LeftMenuHandler(IBuilderProjectsPanelView view, ISectionsController sectionsController)
    {
        this.view = view;
        this.sectionsController = sectionsController;

        view.OnBackToMainMenuPressed += OnSceneSettingsBackPressed;

        sectionsController.OnOpenSectionId += OnOpenSectionId;
        LeftMenuButtonToggleView.OnToggleOn += OnToggleOn;
    }

    public void Dispose()
    {
        view.OnBackToMainMenuPressed -= OnSceneSettingsBackPressed;

        sectionsController.OnOpenSectionId -= OnOpenSectionId;
        LeftMenuButtonToggleView.OnToggleOn -= OnToggleOn;
    }

    public void SetToPreviousMainPanel()
    {
        if (isMainPanel)
            return;

        sectionsController.OpenSection(lastMainSectionId);
    }

    void OnToggleOn(LeftMenuButtonToggleView toggle)
    {
        sectionsController.OpenSection(toggle.openSection);
    }

    void OnOpenSectionId(SectionId sectionId)
    {
        view.SetTogglOnWithoutNotify(sectionId);

        bool isMainPanelSection = sectionId == SectionId.SCENES_MAIN ||
                                  sectionId == SectionId.SCENES_DEPLOYED ||
                                  sectionId == SectionId.SCENES_PROJECT ||
                                  sectionId == SectionId.LAND;

        if (isMainPanelSection)
        {
            lastMainSectionId = sectionId;
            SetMainLeftPanel();
        }
        else
        {
            SetSettingsLeftPanel();
        }
    }

    void OnSceneSettingsBackPressed()
    {
        SetToPreviousMainPanel();
    }

    void SetMainLeftPanel()
    {
        if (isMainPanel)
            return;

        isMainPanel = true;
        view.SetMainLeftPanel();
    }

    void SetSettingsLeftPanel()
    {
        if (!isMainPanel)
            return;

        isMainPanel = false;
        view.SetProjectSettingsLeftPanel();
    }
}
