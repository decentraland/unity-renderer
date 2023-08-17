using DCL;
using UnityEngine;

public interface IPlacesAndEventsSectionComponentView
{
    /// <summary>
    /// Places sub-section component.
    /// </summary>
    IPlacesSubSectionComponentView PlacesSubSectionView { get; }

    /// <summary>
    /// Events sub-section component.
    /// </summary>
    IEventsSubSectionComponentView EventsSubSectionView { get; }

    /// <summary>
    /// Favorites sub-section component.
    /// </summary>
    IFavoritesSubSectionComponentView FavoritesSubSectionView { get; }

    /// <summary>
    /// Favorites sub-section component.
    /// </summary>
    ISearchSubSectionComponentView SearchSubSectionView { get; }

    SearchBarComponentView SearchBar { get; }

    /// <summary>
    /// Open a sub-section.
    /// </summary>
    /// <param name="subSectionIndex">Sub-section index.</param>
    void GoToSubsection(int subSectionIndex);

    /// <summary>
    /// Activates/deactivates the section.
    /// </summary>
    /// <param name="isActive"></param>
    void SetActive(bool isActive);

    void EnableSearchBar(bool isActive);
}

public class PlacesAndEventsSectionComponentView : BaseComponentView, IPlacesAndEventsSectionComponentView
{
    internal const int PLACES_SUB_SECTION_INDEX = 0;
    internal const int EVENTS_SUB_SECTION_INDEX = 1;
    internal const int FAVORITES_SUB_SECTION_INDEX = 2;
    internal const int SEARCH_SUB_SECTION_INDEX = 3;

    [Header("Top Menu")]
    [SerializeField] internal SectionSelectorComponentView subSectionSelector;

    [Header("Sub-Sections")]
    [SerializeField] internal PlacesSubSectionComponentView placesSubSection;
    [SerializeField] internal EventsSubSectionComponentView eventsSubSection;
    [SerializeField] internal FavoritesSubSectionComponentView favoritesSubSection;
    [SerializeField] internal SearchSubSectionComponentView searchSubSection;
    [SerializeField] internal SearchBarComponentView searchBar;

    private Canvas canvas;
    private int currentSelectedIndex = -1;

    public override void Awake()
    {
        base.Awake();
        canvas = GetComponent<Canvas>();
    }

    public void Start()
    {
        CreateSubSectionSelectorMappings();
        SetActive(false);
    }

    public IPlacesSubSectionComponentView PlacesSubSectionView => placesSubSection;
    public IEventsSubSectionComponentView EventsSubSectionView => eventsSubSection;
    public IFavoritesSubSectionComponentView FavoritesSubSectionView => favoritesSubSection;
    public ISearchSubSectionComponentView SearchSubSectionView => searchSubSection;
    public SearchBarComponentView SearchBar => searchBar;

    public void GoToSubsection(int subSectionIndex) =>
        subSectionSelector.GetSection(subSectionIndex)?.SelectToggle(reselectIfAlreadyOn: true);

    public void SetActive(bool isActive)
    {
        canvas.enabled = isActive;

        //Temporary untill the full feature is released
        if(DataStore.i.HUDs.enableFavoritePlaces.Get())
            subSectionSelector.EnableSection(FAVORITES_SUB_SECTION_INDEX);
        else
            subSectionSelector.DisableSection(FAVORITES_SUB_SECTION_INDEX);

        placesSubSection.SetActive(isActive && currentSelectedIndex == PLACES_SUB_SECTION_INDEX);
        eventsSubSection.SetActive(isActive && currentSelectedIndex == EVENTS_SUB_SECTION_INDEX);
        favoritesSubSection.SetActive(isActive && currentSelectedIndex == FAVORITES_SUB_SECTION_INDEX);
        searchSubSection.SetActive(isActive && currentSelectedIndex == SEARCH_SUB_SECTION_INDEX);

        if (!isActive)
        {
            searchBar.ClearSearch(false);
            searchSubSection.SetHeaderEnabled("");
        }
    }

    public void EnableSearchBar(bool isActive)
    {
        searchBar.gameObject.SetActive(isActive);
    }

    public override void RefreshControl()
    {
        placesSubSection.RefreshControl();
        eventsSubSection.RefreshControl();
        favoritesSubSection.RefreshControl();
        searchSubSection.RefreshControl();
    }

    public override void Dispose()
    {
        base.Dispose();

        RemoveSectionSelectorMappings();
        placesSubSection.Dispose();
        eventsSubSection.Dispose();
        favoritesSubSection.Dispose();
        searchSubSection.Dispose();
    }

    internal void CreateSubSectionSelectorMappings()
    {
        subSectionSelector.GetSection(PLACES_SUB_SECTION_INDEX)?.onSelect.AddListener((isActive) =>
        {
            placesSubSection.SetActive(isActive);
            searchSubSection.SetActive(false);
            currentSelectedIndex = PLACES_SUB_SECTION_INDEX;
        });
        subSectionSelector.GetSection(EVENTS_SUB_SECTION_INDEX)?.onSelect.AddListener((isActive) =>
        {
            eventsSubSection.SetActive(isActive);
            searchSubSection.SetActive(false);
            currentSelectedIndex = EVENTS_SUB_SECTION_INDEX;
        });
        subSectionSelector.GetSection(FAVORITES_SUB_SECTION_INDEX)?.onSelect.AddListener((isActive) =>
        {
            favoritesSubSection.SetActive(isActive);
            searchSubSection.SetActive(false);
            currentSelectedIndex = FAVORITES_SUB_SECTION_INDEX;
        });
        subSectionSelector.GetSection(SEARCH_SUB_SECTION_INDEX)?.onSelect.AddListener((isActive) =>
        {
            searchSubSection.SetActive(isActive);
        });

        searchBar.OnSearchText += SearchTextChanged;

        placesSubSection.SetActive(false);
        eventsSubSection.SetActive(false);
        favoritesSubSection.SetActive(false);
        searchSubSection.SetActive(false);

        subSectionSelector.GetSection(PLACES_SUB_SECTION_INDEX)?.SelectToggle(reselectIfAlreadyOn: true);
    }

    private void SearchTextChanged(string searchText)
    {
        if (string.IsNullOrEmpty(searchText))
        {
            subSectionSelector.GetSection(currentSelectedIndex)?.onSelect?.Invoke(true);
            subSectionSelector.GetSection(SEARCH_SUB_SECTION_INDEX)?.UnSelectToggle(true);
        }
        else
        {
            subSectionSelector.GetSection(currentSelectedIndex)?.onSelect?.Invoke(false);
            subSectionSelector.GetSection(SEARCH_SUB_SECTION_INDEX)?.SelectToggle(true);
        }
    }

    internal void RemoveSectionSelectorMappings()
    {
        subSectionSelector.GetSection(PLACES_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(EVENTS_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(FAVORITES_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(SEARCH_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
    }
}
