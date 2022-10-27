using UnityEngine;

public interface IPlacesAndEventsSectionComponentView
{
    /// <summary>
    /// Highlights sub-section component.
    /// </summary>
    IHighlightsSubSectionComponentView HighlightsSubSectionView { get; }

    /// <summary>
    /// Places sub-section component.
    /// </summary>
    IPlacesSubSectionComponentView PlacesSubSectionView { get; }

    /// <summary>
    /// Events sub-section component.
    /// </summary>
    IEventsSubSectionComponentView EventsSubSectionView { get; }

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
}

public class PlacesAndEventsSectionComponentView : BaseComponentView, IPlacesAndEventsSectionComponentView
{
    internal const int HIGHLIGHTS_SUB_SECTION_INDEX = 0;
    internal const int PLACES_SUB_SECTION_INDEX = 1;
    internal const int EVENTS_SUB_SECTION_INDEX = 2;

    [Header("Top Menu")]
    [SerializeField] internal SectionSelectorComponentView subSectionSelector;

    [Header("Sub-Sections")]
    [SerializeField] internal HighlightsSubSectionComponentView highlightsSubSection;
    [SerializeField] internal PlacesSubSectionComponentView placesSubSection;
    [SerializeField] internal EventsSubSectionComponentView eventsSubSection;
    
    private Canvas canvas;

    public override void Awake()
    {
        base.Awake();
        canvas = GetComponent<Canvas>();
    }

    public override void Start()
    {
        CreateSubSectionSelectorMappings();
        SetActive(false);
    }

    public IHighlightsSubSectionComponentView HighlightsSubSectionView => highlightsSubSection;
    public IPlacesSubSectionComponentView PlacesSubSectionView => placesSubSection;
    public IEventsSubSectionComponentView EventsSubSectionView => eventsSubSection;

    public void GoToSubsection(int subSectionIndex) =>
        subSectionSelector.GetSection(subSectionIndex)?.SelectToggle(reselectIfAlreadyOn: true);

    public void SetActive(bool isActive) => 
        canvas.enabled = isActive;

    public override void RefreshControl()
    {
        highlightsSubSection.RefreshControl();
        placesSubSection.RefreshControl();
        eventsSubSection.RefreshControl();
    }

    public override void Dispose()
    {
        base.Dispose();

        RemoveSectionSelectorMappings();
        highlightsSubSection.Dispose();
        placesSubSection.Dispose();
        eventsSubSection.Dispose();
    }

    internal void CreateSubSectionSelectorMappings()
    {
        subSectionSelector.GetSection(HIGHLIGHTS_SUB_SECTION_INDEX)?.onSelect.AddListener(highlightsSubSection.SetActive);
        subSectionSelector.GetSection(PLACES_SUB_SECTION_INDEX)?.onSelect.AddListener(placesSubSection.SetActive);
        subSectionSelector.GetSection(EVENTS_SUB_SECTION_INDEX)?.onSelect.AddListener(eventsSubSection.SetActive);

        placesSubSection.SetActive(false);
        eventsSubSection.SetActive(false);
        highlightsSubSection.SetActive(false);
           
        subSectionSelector.GetSection(HIGHLIGHTS_SUB_SECTION_INDEX)?.SelectToggle(reselectIfAlreadyOn: true);
    }

    internal void RemoveSectionSelectorMappings()
    {
        subSectionSelector.GetSection(HIGHLIGHTS_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(PLACES_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
        subSectionSelector.GetSection(EVENTS_SUB_SECTION_INDEX)?.onSelect.RemoveAllListeners();
    }
}