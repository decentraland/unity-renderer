using System;
using System.Collections.Generic;
using UnityEngine;

internal enum SectionId
{
    SCENES_MAIN,
    SCENES_DEPLOYED,
    SCENES_PROJECT,
    LAND,
    SETTINGS_PROJECT_GENERAL,
    SETTINGS_PROJECT_CONTRIBUTORS,
    SETTINGS_PROJECT_ADMIN
}

internal interface ISectionsController : IDisposable
{
    event Action<SectionBase> OnSectionLoaded;
    event Action<SectionBase> OnSectionShow;
    event Action<SectionBase> OnSectionHide;
    event Action OnRequestContextMenuHide;
    event Action<SectionId> OnOpenSectionId;
    event Action<string, SceneDataUpdatePayload> OnRequestUpdateSceneData;
    event Action<string, SceneContributorsUpdatePayload> OnRequestUpdateSceneContributors;
    event Action<string, SceneAdminsUpdatePayload> OnRequestUpdateSceneAdmins;
    event Action<string, SceneBannedUsersUpdatePayload> OnRequestUpdateSceneBannedUsers;
    event Action<string> OnRequestOpenUrl;
    event Action<Vector2Int> OnRequestGoToCoords;
    event Action<Vector2Int> OnRequestEditSceneAtCoords;
    void OpenSection(SectionId id);
    void SetFetchingDataStart();
    void SetFetchingDataEnd();
}

/// <summary>
/// This class is in charge of handling open/close of the different menu sections
/// </summary>
internal class SectionsController : ISectionsController
{
    public event Action<SectionBase> OnSectionLoaded;
    public event Action<SectionBase> OnSectionShow;
    public event Action<SectionBase> OnSectionHide;
    public event Action OnRequestContextMenuHide;
    public event Action<SectionId> OnOpenSectionId;
    public event Action<string, SceneDataUpdatePayload> OnRequestUpdateSceneData;
    public event Action<string, SceneContributorsUpdatePayload> OnRequestUpdateSceneContributors;
    public event Action<string, SceneAdminsUpdatePayload> OnRequestUpdateSceneAdmins;
    public event Action<string, SceneBannedUsersUpdatePayload> OnRequestUpdateSceneBannedUsers;
    public event Action<string> OnRequestOpenUrl;
    public event Action<Vector2Int> OnRequestGoToCoords;
    public event Action<Vector2Int> OnRequestEditSceneAtCoords;

    private Dictionary<SectionId, SectionBase> loadedSections = new Dictionary<SectionId, SectionBase>();
    private Transform sectionsParent;
    private ISectionFactory sectionFactory;
    private SectionBase currentOpenSection;
    private bool isLoading = false;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="sectionsParent">container for the different sections view</param>
    public SectionsController(Transform sectionsParent) : this(new SectionFactory(), sectionsParent)
    {
    }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="sectionFactory">factory that instantiates menu sections</param>
    /// <param name="sectionsParent">container for the different sections view</param>
    public SectionsController(ISectionFactory sectionFactory, Transform sectionsParent)
    {
        this.sectionsParent = sectionsParent;
        this.sectionFactory = sectionFactory;
    }

    /// <summary>
    /// Get (load if not already loaded) the controller for certain menu section id
    /// </summary>
    /// <param name="id">id of the controller to get</param>
    /// <returns></returns>
    public SectionBase GetOrLoadSection(SectionId id)
    {
        if (loadedSections.TryGetValue(id, out SectionBase section))
        {
            return section;
        }

        section = sectionFactory.GetSectionController(id);
        if (section != null)
        {
            section.SetViewContainer(sectionsParent);
            section.SetFetchingDataState(isLoading);
            SubscribeEvents(section);
        }

        loadedSections.Add(id, section);
        OnSectionLoaded?.Invoke(section);
        return section;
    }

    /// <summary>
    /// Opens (make visible) a menu section. It will load it if necessary.
    /// Closes (hides) the previously open section.
    /// </summary>
    /// <param name="id">id of the section to show</param>
    public void OpenSection(SectionId id)
    {
        var section = GetOrLoadSection(id);
        var success = OpenSection(section);
        if (success)
        {
            OnOpenSectionId?.Invoke(id);
        }
    }

    public void SetFetchingDataStart()
    {
        SetIsLoading(true);
    }
    
    public void SetFetchingDataEnd()
    {
        SetIsLoading(false);
    }

    private void SetIsLoading(bool isLoading)
    {
        this.isLoading = isLoading;
        using (var iterator = loadedSections.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.SetFetchingDataState(isLoading);
            }
        }
    }

    private bool OpenSection(SectionBase section)
    {
        if (currentOpenSection == section)
            return false;

        if (currentOpenSection != null)
        {
            currentOpenSection.SetVisible(false);
            OnSectionHide?.Invoke(currentOpenSection);
        }

        currentOpenSection = section;

        if (currentOpenSection != null)
        {
            currentOpenSection.SetVisible(true);
            OnSectionShow?.Invoke(currentOpenSection);
        }

        return true;
    }

    public void Dispose()
    {
        using (var iterator = loadedSections.GetEnumerator())
        {
            while (iterator.MoveNext())
            {
                iterator.Current.Value.Dispose();
            }
        }

        loadedSections.Clear();
    }

    private void OnHideContextMenuRequested()
    {
        OnRequestContextMenuHide?.Invoke();
    }

    private void OnUpdateSceneDataRequested(string id, SceneDataUpdatePayload payload)
    {
        OnRequestUpdateSceneData?.Invoke(id, payload);
    }
    
    private void OnUpdateSceneContributorsRequested(string id, SceneContributorsUpdatePayload payload)
    {
        OnRequestUpdateSceneContributors?.Invoke(id, payload);
    }
    
    private void OnUpdateSceneAdminsRequested(string id, SceneAdminsUpdatePayload payload)
    {
        OnRequestUpdateSceneAdmins?.Invoke(id, payload);
    }
    
    private void OnUpdateSceneBannedUsersRequested(string id, SceneBannedUsersUpdatePayload payload)
    {
        OnRequestUpdateSceneBannedUsers?.Invoke(id, payload);
    }
    
    private void OnOpenUrlRequested(string url)
    {
        OnRequestOpenUrl?.Invoke(url);
    }
    
    private void OnGoToCoordsRequested(Vector2Int coords)
    {
        OnRequestGoToCoords?.Invoke(coords);
    }
    
    private void OnEditSceneAtCoordsRequested(Vector2Int coords)
    {
        OnRequestEditSceneAtCoords?.Invoke(coords);
    }

    private void SubscribeEvents(SectionBase sectionBase)
    {
        if (sectionBase is ISectionOpenSectionRequester openSectionRequester)
        {
            openSectionRequester.OnRequestOpenSection += OpenSection;
        }
        if (sectionBase is ISectionHideContextMenuRequester hideContextMenuRequester)
        {
            hideContextMenuRequester.OnRequestContextMenuHide += OnHideContextMenuRequested;
        }
        if (sectionBase is ISectionUpdateSceneDataRequester updateSceneDataRequester)
        {
            updateSceneDataRequester.OnRequestUpdateSceneData += OnUpdateSceneDataRequested;
        }       
        if (sectionBase is ISectionUpdateSceneContributorsRequester updateSceneContributorsRequester)
        {
            updateSceneContributorsRequester.OnRequestUpdateSceneContributors += OnUpdateSceneContributorsRequested;
        }
        if (sectionBase is ISectionUpdateSceneAdminsRequester updateSceneAdminsRequester)
        {
            updateSceneAdminsRequester.OnRequestUpdateSceneAdmins += OnUpdateSceneAdminsRequested;
        }
        if (sectionBase is ISectionUpdateSceneBannedUsersRequester updateSceneBannedUsersRequester)
        {
            updateSceneBannedUsersRequester.OnRequestUpdateSceneBannedUsers += OnUpdateSceneBannedUsersRequested;
        }
        if (sectionBase is ISectionOpenURLRequester openURLRequester)
        {
            openURLRequester.OnRequestOpenUrl += OnOpenUrlRequested;
        }
        if (sectionBase is ISectionGotoCoordsRequester goToRequester)
        {
            goToRequester.OnRequestGoToCoords += OnGoToCoordsRequested;
        }
        if (sectionBase is ISectionEditSceneAtCoordsRequester editSceneRequester)
        {
            editSceneRequester.OnRequestEditSceneAtCoords += OnEditSceneAtCoordsRequested;
        }
    }
}